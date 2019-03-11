using System;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using DataModels;
using Newtonsoft.Json;
using JsonSerializer = Amazon.Lambda.Serialization.Json.JsonSerializer;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace ConnectWebSocket
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
            var table = Table.LoadTable(client, "ConnectionIds");
            var id = new Document {["ConnectionId"] = input.RequestContext.ConnectionId};
            await table.PutItemAsync(id);
            return new HttpResponse
            {
                statusCode = HttpStatusCode.OK
            };
        }
    }
}