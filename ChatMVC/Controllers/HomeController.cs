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
            var viewModel = new ChatViewModel
            {
                Messages = new List<ChatMessage>
                {
                    new ChatMessage { Role = "assistant", Content = "¡Hola! ¿En qué puedo ayudarte?" }
                }
            };
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

                // *** Importante: Limpiar la respuesta de cualquier patrón de hora añadido por el servidor ***
                string cleanedResponse = response;
                // Remover " (Hora: HH:MM)" del final
                cleanedResponse = Regex.Replace(cleanedResponse, @"\s*\(Hora:\s*\d{2}:\d{2}\)$", "").Trim();
                // Remover "HH:MMM " o similar del principio
                cleanedResponse = Regex.Replace(cleanedResponse, @"^\s*\d{1,2}:\d{2,3}\s*", "").Trim();

                // Add assistant response
                model.Messages.Add(new ChatMessage { Role = "assistant", Content = cleanedResponse });

                return Json(cleanedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message");
                // El mensaje de error literal no debería contener la hora del servidor, así que no se limpia aquí.
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
