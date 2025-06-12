namespace ChatMVC.Models
{
    public class ChatViewModel
    {
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public string NewMessage { get; set; } = string.Empty;
    }
}
