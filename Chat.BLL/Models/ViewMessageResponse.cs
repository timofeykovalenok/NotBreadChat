﻿namespace Chat.BLL.Models
{
    public class ViewMessageResponse
    {
        public required long MessageAuthorId { get; init; }
        public required long ViewedByUserId { get; init; }
        public required int UnviewedMessagesLeft { get; init; }
    }
}
