using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using DataModels;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace DisconnectWebSocket
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
            var table = Table.LoadTable(client, Environment.GetEnvironmentVariable("CONNECTION_ID_TABLE"));
            var id = new Document {["ConnectionId"] = input.RequestContext.ConnectionId};
            await table.DeleteItemAsync(id);
            return new HttpResponse
            {
                statusCode = HttpStatusCode.OK
            };
        }
    }
}