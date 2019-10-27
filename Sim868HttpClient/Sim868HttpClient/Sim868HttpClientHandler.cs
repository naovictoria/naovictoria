using System;
using System.IO.Ports;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Sim868HttpClient
{
    /// <summary>
    /// An implementation of HttpClientHandler that uses the very limited HTTP API on the SM868.
    /// </summary>
    public class Sim868HttpClientHandler : HttpClientHandler
    {
        private const int BearerId = 1;
        private Sim868Client _client;
        private string _apn;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="apn">Access Point Name, note this is dependend on the service provider.</param>
        public Sim868HttpClientHandler(Sim868Client client, string apn)
        {
            _client = client;
            _apn = apn;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // First, we're going to query to see if the connection is already opened.
            (int bearerId, BearerStatus status, string ipAddress) = await _client.QueryBearerStatusAsync(BearerId);

            if (status == BearerStatus.Closed)
            {
                // Configure connection type and APN (Access Point Name)
                await _client.SetBearerParamAsync(BearerId, Sim868Client.BearerParamConnType, "GPRS");
                await _client.SetBearerParamAsync(BearerId, Sim868Client.BearerParamApn, $"{_apn}");

                // Open the GPRS context.
                if(!await _client.OpenBearerAsync(BearerId))
                {
                    throw new InvalidOperationException("Cannot open GPRS context.");
                }

                // Let's query the bearer.
                (_, BearerStatus connectStatus, _) = await _client.QueryBearerStatusAsync(BearerId);

                // Also initialize HTTP service.
                await _client.InitHttpAsync();

                // Set the bearer ID for HTTP
                await _client.SetHttpParamAsync(Sim868Client.HttpParamCid, BearerId);
            }

            await _client.SetHttpParamAsync(Sim868Client.HttpParamUrl, request.RequestUri.AbsoluteUri);
            (HttpMethod method, int statusCode, int dataLength) = await _client.ExecuteHttpActionAsync(HttpMethod.Get);
            string responseBody = await _client.ReadHttpResponseAsync();

            // Construct response message
            var responseMessage = new HttpResponseMessage()
            {
                StatusCode = (HttpStatusCode)statusCode,
                Content = new StringContent(responseBody)
            };

            responseMessage.Headers.Add("Testing", "Test");

            return responseMessage;
        }
    }
}