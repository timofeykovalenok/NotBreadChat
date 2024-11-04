using FluentMigrator;
using System.Data;

namespace Chat.DAL.Migrations
{
    [Core.Attributes.Migration(2024, 09, 23, 16, 00)]
    public class M000_InitMigration : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table("Users")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("Login").AsString(64).Unique()
                .WithColumn("Name").AsString(64)
                .WithColumn("Image").AsString().Nullable()
                .WithColumn("PasswordHash").AsString()
                .WithColumn("PasswordSalt").AsString();

            Create.Table("GroupChats")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("CreatorId").AsInt64().ForeignKey("Users", "Id")
                .WithColumn("Title").AsString(64)
                .WithColumn("Image").AsString().Nullable();

            Create.Table("PrivateChats")
                .WithColumn("OwnerId").AsInt64().PrimaryKey().ForeignKey("Users", "Id")
                .WithColumn("OtherUserId").AsInt64().PrimaryKey().ForeignKey("Users", "Id")
                .WithColumn("LastViewedMessageCreateAt").AsDateTime().Nullable();

            Create.Table("UsersInChats")
                .WithColumn("ChatId").AsInt64().PrimaryKey().ForeignKey("GroupChats", "Id")
                .WithColumn("UserId").AsInt64().PrimaryKey().ForeignKey("Users", "Id")
                .WithColumn("CustomTitle").AsString(64).Nullable()
                .WithColumn("CustomImage").AsString().Nullable()
                .WithColumn("LastViewedMessageCreateAt").AsDateTime().Nullable();

            Create.Table("Contacts")
                .WithColumn("ContactUserId").AsInt64().PrimaryKey().ForeignKey("Users", "Id")
                .WithColumn("UserId").AsInt64().PrimaryKey().ForeignKey("Users", "Id")
                .WithColumn("CustomTitle").AsString(64).Nullable()
                .WithColumn("CustomImage").AsString().Nullable();

            Create.Table("Messages")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("AuthorId").AsInt64().ForeignKey("Users", "Id")
                .WithColumn("Content").AsString()
                .WithColumn("CreateAt").AsDateTime()
                .WithColumn("ModifyAt").AsDateTime().Nullable();

            Create.Table("MessagesInGroupChats")
                .WithColumn("MessageId").AsInt64().PrimaryKey().ForeignKey("Messages", "Id")
                .WithColumn("ChatId").AsInt64().ForeignKey("GroupChats", "Id");
            
            Create.Table("MessagesInPrivateChats")
                .WithColumn("MessageId").AsInt64().PrimaryKey().ForeignKey("Messages", "Id")
                .WithColumn("ReceiverUserId").AsInt64().ForeignKey("Users", "Id");

            Create.Table("DeletedMessages")
                .WithColumn("MessageId").AsInt64().PrimaryKey().ForeignKey("Messages", "Id")
                .WithColumn("UserId").AsInt64().PrimaryKey().ForeignKey("Users", "Id");
        }
    }
}
