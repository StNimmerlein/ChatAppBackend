namespace DataModels
{
    public class MessageEvent
    {
        public MessageEventType Type { get; set; }
        public MessageKeys Keys { get; set; }
        public MessageOutput Message { get; set; }
    }
}