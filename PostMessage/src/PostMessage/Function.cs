using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using DataModels;

[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace PostMessage
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

        public async Task<string> FunctionHandler(MessageInput messageInput, ILambdaContext context)
        {
            try
            {
                await dbContext.SaveAsync(new MessageDbDto(messageInput));
                return "success";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "error";
            }
        }
    }
}