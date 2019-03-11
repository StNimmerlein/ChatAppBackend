using System;
using Amazon.DynamoDBv2.DataModel;

namespace DataModels
{
    [DynamoDBTable("Messages")]
    public class MessageDbDto
    {
        public MessageDbDto()
        {
        }

        public MessageDbDto(MessageInput messageInput)
        {
            Id = messageInput.GetHashCode();
            Text = messageInput.Text;
            Author = messageInput.Author;
            Time = DateTime.UtcNow;
        }

        [DynamoDBHashKey] public int Id { get; set; }
        public string Text { get; set; }
        public string Author { get; set; }
        [DynamoDBRangeKey]
        public DateTime Time { get; set; }

        public MessageOutput ToOutput()
        {
            return new MessageOutput
            {
                Id = Id,
                Time = Time,
                Author = Author,
                Text = Text
            };
        }
    }
}