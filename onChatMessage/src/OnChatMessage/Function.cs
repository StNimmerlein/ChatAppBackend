using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Aws4RequestSigner;
using DataModels;
using Newtonsoft.Json;
using JsonSerializer = Amazon.Lambda.Serialization.Json.JsonSerializer;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace OnChatMessage
{
    public class Function
    {
        private AmazonDynamoDBClient client;

        public Function()
        {
            var config = new AmazonDynamoDBConfig {RegionEndpoint = RegionEndpoint.EUWest1};
            client = new AmazonDynamoDBClient(config);
        }

        public async Task<HttpResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
        {
            var message = JsonConvert.DeserializeObject<TmpMessage>(input.Body);
            var table = Table.LoadTable(client, "ConnectionIds");

            var scanFilter = new ScanFilter();
            var search = table.Scan(scanFilter);
            var remaining = await search.GetRemainingAsync();

            var httpClient = new HttpClient();

            // Function no longer in use, therefore no up-to-date keys
            var signer = new AWS4RequestSigner("AccessKey", "SecretKey");
            foreach (var doc in remaining)
            {
                var connectionId = doc["ConnectionId"];
                var endpoint =
                    $"https://{input.RequestContext.DomainName}/{input.RequestContext.Stage}/@connections/{connectionId}";

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(endpoint),
                    Content = new StringContent(message.Message)
                };

                request = await signer.Sign(request, "execute-api", "eu-west-1");
                var response = await httpClient.SendAsync(request);
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }

            return new HttpResponse
            {
                statusCode = HttpStatusCode.OK
            };
        }
    }
}