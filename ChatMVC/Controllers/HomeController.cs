using System.Diagnostics;
using ChatMVC.Models;
using ChatMVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace ChatMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMistralService _mistralService;

        public HomeController(ILogger<HomeController> logger, IMistralService mistralService)
        {
            _logger = logger;
            _mistralService = mistralService;
        }

        public IActionResult Index()
        {
            var viewModel = new ChatViewModel();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatViewModel model)
        {
            if (string.IsNullOrEmpty(model.NewMessage))
                return Json("Por favor, escribe un mensaje.");

            if (model.Messages == null)
                model.Messages = new List<ChatMessage>();

            try
            {
                // Add user message
                model.Messages.Add(new ChatMessage { Role = "user", Content = model.NewMessage });

                // Get a response from Mistral
                var response = await _mistralService.GetChatCompletionAsync(model.Messages);

                // Clean the response of any time patterns added by the server
                string cleanedResponse = response;
                cleanedResponse = Regex.Replace(cleanedResponse, @"\s*\(Hora:\s*\d{2}:\d{2}\)$", "").Trim();
                cleanedResponse = Regex.Replace(cleanedResponse, @"^\s*\d{1,2}:\d{2,3}\s*", "").Trim();

                // Add assistant response
                model.Messages.Add(new ChatMessage { Role = "assistant", Content = cleanedResponse });

                return Json(cleanedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message");
                // The literal error message should not contain the server time, so it is not cleaned up here
                return Json("Lo siento, hubo un error al procesar tu mensaje");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
