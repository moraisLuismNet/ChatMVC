using ChatMVC.Models;

namespace ChatMVC.Services
{
    public interface IMistralService
    {
        Task<string> GetChatCompletionAsync(List<ChatMessage> messages);
    }
}
