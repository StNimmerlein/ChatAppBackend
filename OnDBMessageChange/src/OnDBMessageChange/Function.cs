using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Aws4RequestSigner;
using DataModels;
using Newtonsoft.Json;
using JsonSerializer = Amazon.Lambda.Serialization.Json.JsonSerializer;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace OnDBMessageChange
{
    public class Function
    {
        private readonly string ACCESS_KEY = Environment.GetEnvironmentVariable("ACCESS_KEY");
        private readonly string SECRET_KEY = Environment.GetEnvironmentVariable("SECRET_KEY");
        private readonly string WEB_SOCKET_URL = Environment.GetEnvironmentVariable("WEB_SOCKET_URL");
        private readonly AWS4RequestSigner signer;

        private readonly HttpClient httpClient = new HttpClient();
        private readonly Table table;

        public Function()
        {
            signer = new AWS4RequestSigner(ACCESS_KEY, SECRET_KEY);
            var config = new AmazonDynamoDBConfig {RegionEndpoint = RegionEndpoint.EUWest1};
            var client = new AmazonDynamoDBClient(config);
            table = Table.LoadTable(client, Environment.GetEnvironmentVariable("CONNECTION_ID_TABLE"));
        }

        public Object FunctionHandler(DynamoDBEvent input, ILambdaContext context)
        {
            Console.WriteLine(JsonConvert.SerializeObject(input));
            foreach (var record in input.Records)
            {
                var message = CreateMessageEvent(record);
                Console.WriteLine("Message created");
                SendMessageEventToAllConnectedClients(message);
            }

            Console.WriteLine(JsonConvert.SerializeObject(input));
            return input;
        }

        private MessageEvent CreateMessageEvent(DynamoDBEvent.DynamodbStreamRecord record)
        {
            MessageOutput message = null;
            var newImage = record.Dynamodb.NewImage;
            if (newImage.Count > 0)
            {
                message = new MessageOutput
                {
                    Author = newImage.ContainsKey("Author") ? newImage["Author"].S : "",
                    Id = int.Parse(newImage["Id"].N),
                    Text = newImage.ContainsKey("Text") ? newImage["Text"].S : "",
                    Time = DateTime.Parse(newImage["Time"].S)
                };
            }

            var messageEvent = new MessageEvent
            {
                Keys = new MessageKeys
                {
                    Id = int.Parse(record.Dynamodb.Keys["Id"].N),
                    Time = DateTime.Parse(record.Dynamodb.Keys["Time"].S)
                },
                Message = message
            };
            if (record.EventName == OperationType.INSERT)
            {
                messageEvent.Type = MessageEventType.INSERT;
            }

            if (record.EventName == OperationType.REMOVE)
            {
                messageEvent.Type = MessageEventType.REMOVE;
            }

            if (record.EventName == OperationType.MODIFY)
            {
                messageEvent.Type = MessageEventType.MODIFY;
            }

            return messageEvent;
        }

        private void SendMessageEventToAllConnectedClients(MessageEvent message)
        {
            var scanFilter = new ScanFilter();
            var search = table.Scan(scanFilter);
            var getTask = search.GetRemainingAsync();
            getTask.Wait();
            var remaining = getTask.Result;
            var tasks = new List<Task>();

            Console.WriteLine($"Connected ids: {remaining.Count}");

            foreach (var doc in remaining)
            {
                var connectionId = doc["ConnectionId"];
                Console.WriteLine($"Current id: {connectionId}");
                var endpoint =
                    $"https://{WEB_SOCKET_URL}/@connections/{connectionId}";

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(endpoint),
                    Content = new StringContent(JsonConvert.SerializeObject(message))
                };
//                request.Headers.Add("Content-Type", "application/json");

                Console.WriteLine($"Sign {connectionId}.");
                tasks.Add(signer.Sign(request, "execute-api", "eu-west-1")
                    .ContinueWith(signedRequest =>
                    {
                        Console.WriteLine($"Sending to {connectionId}.");
                        httpClient.SendAsync(signedRequest.Result).Wait();
                    }));
            }

            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("Everything sent.");
        }
    }
}