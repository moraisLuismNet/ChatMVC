namespace ChatMVC.Models
{
    public class ChatMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }

        // Parameterless constructor for deserialization
        public ChatMessage()
        {
        }

        // Constructor with parameters
        public ChatMessage(string role, string content)
        {
            Role = role;
            Content = content;
        }
    }

}