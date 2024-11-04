﻿namespace Chat.BLL.Models
{
    public class EditMessageResponse
    {
        public required long EditedByUserId { get; init; }
        public required long OtherUserId { get; init; }
        public required long MessageId { get; init; }
        public required string Content { get; init; }
    }
}
