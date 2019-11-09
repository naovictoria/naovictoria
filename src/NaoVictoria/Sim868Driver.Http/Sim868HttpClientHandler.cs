using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NaoVictoria.Sim868Driver.Http
{
    /// <summary>
    /// An implementation of HttpClientHandler that uses the very limited HTTP API on the SM868.
    /// </summary>
    public class Sim868HttpClientHandler : HttpClientHandler
    {
        private const int BearerId = 1;

        private Driver _driver;
        private string _apn;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="apn">Access Point Name, note this is dependend on the service provider.</param>
        public Sim868HttpClientHandler(Driver driver, string apn)
        {
            _driver = driver;
            _apn = apn;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // First, we're going to query to see if the connection is already opened.
            (int bearerId, BearerStatus status, string ipAddress) = await _driver.QueryBearerStatusAsync(BearerId);

            if (status == BearerStatus.Closed)
            {
                // Configure connection type and APN (Access Point Name)
                await _driver.SetBearerParamAsync(BearerId, Driver.BearerParamConnType, "GPRS");
                await _driver.SetBearerParamAsync(BearerId, Driver.BearerParamApn, $"{_apn}");

                // Open the GPRS context.
                for (int i = 0; i < 5; i++)
                {
                    if (await _driver.OpenBearerAsync(BearerId))
                    {
                        break;
                    }
                    await Task.Delay(1000);
                }

                // Let's wait until a network connection has been stablished.
                for (int i = 0; i < 6; i++)
                {
                    (_, BearerStatus connectStatus, _) = await _driver.QueryBearerStatusAsync(BearerId);

                    if(connectStatus == BearerStatus.Connected)
                    {
                        break;
                    }
                    await Task.Delay(5000);
                }
            }

            // Also initialize HTTP service.
            await _driver.ActivateHttpModuleAsync();

            // Set the bearer ID for HTTP
            await _driver.SetHttpParamAsync(Driver.HttpParamCid, BearerId);
            await _driver.SetHttpParamAsync(Driver.HttpParamUrl, request.RequestUri.AbsoluteUri);

            HttpMethod httpMethod;

            StringContent requestContent = request.Content as StringContent;

            if (requestContent != null)
            {
                await _driver.SetHttpParamAsync(Driver.HttpParamContent, requestContent.Headers.ContentType.MediaType);
                await _driver.ExecuteHttpDataAsync(await requestContent.ReadAsByteArrayAsync(), TimeSpan.FromMilliseconds(5000));
            }

            if (request.Method == System.Net.Http.HttpMethod.Get)
            {
                httpMethod = HttpMethod.Get;
            }
            else if (request.Method == System.Net.Http.HttpMethod.Post)
            {
                httpMethod = HttpMethod.Post;
            }
            else
            {
                throw new NotSupportedException("Only GET and POST methods are supported for now.");
            }

            (HttpMethod method, int statusCode, int dataLength) = await _driver.ExecuteHttpActionAsync(httpMethod);


            // Construct response message
            var responseMessage = new HttpResponseMessage()
            {
                StatusCode = (HttpStatusCode)statusCode
            };

            if (dataLength > 0)
            {
                string responseBody = await _driver.ReadHttpResponseAsync();
                responseMessage.Content = new StringContent(responseBody);
            }
            else
            {
                responseMessage.Content = new StringContent(string.Empty);
            }

            await _driver.TerminateHttpModuleAsync();

            return responseMessage;
        }
    }
}