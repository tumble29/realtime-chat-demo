using System.Text.Json;

namespace ChatClient.Models
{
    public enum MessageType
    {
        All,
        Group,
        User,
    }

    abstract class MessageContent
    {
        public string Sender { get; set; }
        public string Content { get; set; }
        //Types are "All", "Group", "User"
        public string MessageType { get; set; }
        public MessageContent(string Sender, MessageType messageType, string Content)
        {
            this.Sender= Sender;
            switch (messageType)
            {
                case Models.MessageType.All:
                    this.MessageType = "All";
                    break;
                case Models.MessageType.Group:
                    this.MessageType = "Group";
                    break;
                case Models.MessageType.User:
                    this.MessageType = "User";
                    break;
                default:
                    this.MessageType = "Error";
                    break;
            }
            this.Content= Content;
        }

        public abstract override string ToString();
        public static MessageContent FromJson(string json)
        {
            MessageContent content = JsonSerializer.Deserialize<MessageContent>(json);
            return content;
        }

        public static string ToJson(MessageContent content)
        {
            return JsonSerializer.Serialize(content);
        }
    }
}
