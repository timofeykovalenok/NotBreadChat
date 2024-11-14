﻿namespace Chat.BLL.Models.GroupChat
{
    public class UpdateChatRequest
    {
        public required long ChatId { get; init; }
        public required string Title { get; init; }
        public required string? Image { get; init; }
    }
}