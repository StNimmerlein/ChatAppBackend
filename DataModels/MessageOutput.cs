using System;

namespace DataModels
{
    public class MessageOutput
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public string Author { get; set; }
        public string Text { get; set; }
    }
}