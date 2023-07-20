using System;

namespace MinimalChattApp.Model
    {
        public class MessageResponse
        {
            public int MessageId { get; set; }
            public int SenderId { get; set; }
            public int ReceiverId { get; set; }
            public string Content { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
