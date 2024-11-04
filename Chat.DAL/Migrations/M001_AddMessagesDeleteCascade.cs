using FluentMigrator;
using System.Data;

namespace Chat.DAL.Migrations
{
    [Core.Attributes.Migration(2024, 10, 18, 17, 30)]
    public class M001_AddMessagesDeleteCascade : Migration
    {
        public override void Up()
        {
            Delete.ForeignKey("FK_MessagesInGroupChats_MessageId_Messages_Id").OnTable("MessagesInGroupChats");
            Create.ForeignKey("FK_MessagesInGroupChats_MessageId_Messages_Id")
                .FromTable("MessagesInGroupChats").ForeignColumn("MessageId")
                .ToTable("Messages").PrimaryColumn("Id").OnDelete(Rule.Cascade);

            Delete.ForeignKey("FK_MessagesInPrivateChats_MessageId_Messages_Id").OnTable("MessagesInPrivateChats");
            Create.ForeignKey("FK_MessagesInPrivateChats_MessageId_Messages_Id")
                .FromTable("MessagesInPrivateChats").ForeignColumn("MessageId")
                .ToTable("Messages").PrimaryColumn("Id").OnDelete(Rule.Cascade);
            
            Delete.ForeignKey("FK_DeletedMessages_MessageId_Messages_Id").OnTable("DeletedMessages");
            Create.ForeignKey("FK_DeletedMessages_MessageId_Messages_Id")
                .FromTable("DeletedMessages").ForeignColumn("MessageId")
                .ToTable("Messages").PrimaryColumn("Id").OnDelete(Rule.Cascade);
        }

        public override void Down()
        {
            Delete.ForeignKey("FK_MessagesInGroupChats_MessageId_Messages_Id").OnTable("MessagesInGroupChats");
            Create.ForeignKey("FK_MessagesInGroupChats_MessageId_Messages_Id")
                .FromTable("MessagesInGroupChats").ForeignColumn("MessageId")
                .ToTable("Messages").PrimaryColumn("Id");

            Delete.ForeignKey("FK_MessagesInPrivateChats_MessageId_Messages_Id").OnTable("MessagesInPrivateChats");
            Create.ForeignKey("FK_MessagesInPrivateChats_MessageId_Messages_Id")
                .FromTable("MessagesInPrivateChats").ForeignColumn("MessageId")
                .ToTable("Messages").PrimaryColumn("Id");

            Delete.ForeignKey("FK_DeletedMessages_MessageId_Messages_Id").OnTable("DeletedMessages");
            Create.ForeignKey("FK_DeletedMessages_MessageId_Messages_Id")
                .FromTable("DeletedMessages").ForeignColumn("MessageId")
                .ToTable("Messages").PrimaryColumn("Id");
        }
    }
}
