using System;
using System.IO.Ports;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Sim868HttpClient
{
    public class Sim868HttpClientHandler : HttpClientHandler
    {
        private const int BearerId = 1;
        Sim868Client _client;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="apn">Access Point Name, note this is dependend on the service provider.</param>
        public Sim868HttpClientHandler(Sim868Client client, string apn)
        {
            _client = client;

            // First, we're going to query to see if the connection is already opened.
            (int bearerId, BearerStatus status, string ipAddress) = _client.QueryBearerStatus(BearerId);

            if(status == BearerStatus.Closed)
            {
                // Configure connection type and APN (Access Point Name)
                _client.SetBearerParam(BearerId, Sim868Client.BearerParamConnType, "GPRS");
                _client.SetBearerParam(BearerId, Sim868Client.BearerParamApn, $"{apn}");

                // Open the GPRS context.
                _client.OpenBearer(BearerId);

                // Also initialize HTTP service.
                _client.InitHttp();

                // Set the bearer ID for HTTP
                _client.SetHttpParam(Sim868Client.HttpParamCid, BearerId);

                _client.ClearBuffer();
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _client.SetHttpParam(Sim868Client.HttpParamUrl, request.RequestUri.AbsoluteUri);           
            (HttpMethod method, int statusCode, int dataLength) = _client.ExecuteHttpAction(HttpMethod.Get);
            string responseBody = _client.ReadHttpResponse();
            
            // Construct response message
            var responseMessage = new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(responseBody),
            };

            responseMessage.Headers.Add("Testing", "Test");

            return responseMessage;
        }
    }
}