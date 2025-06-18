using ChatMVC.Models;

namespace ChatMVC.Services
{
    public class MistralService : IMistralService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly ILogger<MistralService> _logger;
        private const int MaxTokens = 2000; // We limit to 2000 tokens to be safe
        private const double Temperature = 0.7; // We added temperature to control creativity
        private const double TopP = 0.9; // We add top_p to control diversity

        public MistralService(IConfiguration configuration, HttpClient httpClient, ILogger<MistralService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenRouter:ApiKey"];
            _model = configuration["OpenRouter:Model"];
            _logger = logger;

            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new ArgumentNullException(nameof(_apiKey), "The OpenRouter API key is not configured.");
            }

            if (string.IsNullOrEmpty(_model))
            {
                throw new ArgumentNullException(nameof(_model), "The OpenRouter model is not configured");
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://github.com/moraisLuismNet/ChatMVC");
            _httpClient.DefaultRequestHeaders.Add("X-Title", "ChatMVC");
        }

        public async Task<string> GetChatCompletionAsync(List<Models.ChatMessage> messages)
        {
            try
            {
                var request = new
                {
                    model = _model,
                    messages = messages.Select(m => new { role = m.Role, content = m.Content }).ToList(),
                    max_tokens = MaxTokens,
                    temperature = Temperature,
                    top_p = TopP
                };

                _logger.LogInformation("Sending request to OpenRouter with model: {Model}", _model);
                var response = await _httpClient.PostAsJsonAsync("https://openrouter.ai/api/v1/chat/completions", request);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<OpenRouterResponse>();
                    if (result?.Choices?.Any() == true)
                    {
                        return result.Choices.First().Message.Content;
                    }
                    _logger.LogWarning("OpenRouter's response contains no messages");
                    return "Sorry, I couldn't process your message correctly";
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error in OpenRouter response. Status: {Status}, Content: {Content}", 
                    response.StatusCode, errorContent);

                // Specific handling of common errors
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    return "Sorry, you've reached the request limit for the free plan. Please wait a few minutes before trying again.";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return "Authentication error. Please check your API key settings.";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.PaymentRequired)
                {
                    return "The free plan has reached its usage limit. Please consider upgrading your plan or waiting until the next billing period.";
                }

                return $"Sorry, there was an error processing your message. (Code: {response.StatusCode})";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error communicating with OpenRouter");
                return "Sorry, there was an error processing your message";
            }
        }
    }

}