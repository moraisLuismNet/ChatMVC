using System.Diagnostics;
using ChatMVC.Models;
using ChatMVC.Services;
using Microsoft.AspNetCore.Mvc;

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

                // Add assistant response
                model.Messages.Add(new ChatMessage { Role = "assistant", Content = response });

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message");
                return Json("Sorry, an error occurred while processing your message");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
