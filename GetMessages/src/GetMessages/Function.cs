using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using DataModels;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace GetMessages
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

        public async Task<List<MessageOutput>> FunctionHandler(GetMessagesQuery query, ILambdaContext context)
        {
            var scanConditions = new List<ScanCondition>();
            scanConditions.Add(new ScanCondition("Time", ScanOperator.GreaterThan, query.StartingAfter));

            var asyncSearch = dbContext.ScanAsync<MessageDbDto>(scanConditions, new DynamoDBOperationConfig
            {
                OverrideTableName = Environment.GetEnvironmentVariable("MESSAGES_TABLE")
            });
            var messages = await asyncSearch.GetRemainingAsync();
            return messages.Select(message => message.ToOutput()).OrderBy(message => message.Time).ToList();
        }
    }
}