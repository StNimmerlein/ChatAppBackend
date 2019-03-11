using Amazon.DynamoDBv2.DataModel;

namespace DataModels
{
    [DynamoDBTable("ConnectionIds")]
    public class RequestContext
    {
        [DynamoDBHashKey]
        public string ConnectionId { get; set; }
    }
}