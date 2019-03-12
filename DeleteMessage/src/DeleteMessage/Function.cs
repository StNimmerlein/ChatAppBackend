using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using DataModels;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace DeleteMessage
{
    public class Function
    {
        private DynamoDBContext dbContext;

        public Function()
        {
            var config = new AmazonDynamoDBConfig {RegionEndpoint = RegionEndpoint.EUWest1};
            var client = new AmazonDynamoDBClient(config);
            dbContext = new DynamoDBContext(client);
        }

        
        public async Task<HttpResponse> FunctionHandler(MessageKeys key, ILambdaContext context)
        {
            var message = new MessageDbDto
            {
                Id = key.Id,
                Time = key.Time
            };
            await dbContext.DeleteAsync(message, new DynamoDBOperationConfig
            {
                OverrideTableName = Environment.GetEnvironmentVariable("MESSAGES_TABLE")
            });
            return new HttpResponse
            {
                statusCode = HttpStatusCode.OK
            };
        }
    }
}