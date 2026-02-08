using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GroqApiLibrary;
using System.Text.Json.Nodes;

namespace WebApplication1.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _config;
        private static JsonArray _chatHistory = new JsonArray();
        
        public JsonArray FullHistory => _chatHistory;
        
        public IndexModel(IConfiguration config)
        {
            _config = config;
        }

        [BindProperty]
        public string Question { get; set; }
        public string Answer { get; set; }
        
        

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Question)) return new BadRequestResult();

            string api = _config.GetValue<string>("GroqSettings:ApiKey");
            string modelName = _config.GetValue<string>("GroqSettings:ModelName");

            var groqClient = new GroqApiClient(api);

            var userMessage = new JsonObject
            {
                ["role"] = "user",
                ["content"] = Question
            };
            _chatHistory.Add(userMessage);

            var request = new JsonObject
            {
                ["model"] = modelName,
                ["messages"] = new JsonArray
                {
                    new JsonObject { ["role"] = "user", ["content"] = Question }
                }
            };

            try 
            {
                var result = await groqClient.CreateChatCompletionAsync(request);
                Answer = result?["choices"]?[0]?["message"]?["content"]?.ToString();
                var assistantMessage = new JsonObject
                {
                    ["role"] = "assistant",
                    ["content"] = Answer
                };
                _chatHistory.Add(assistantMessage);
                return new JsonResult(new { answer = Answer });
            }
            catch (Exception ex)
            {
                Answer = "Ошибка связи с ИИ: " + ex.Message;
                return new JsonResult(new { answer = "Ошибка: " + ex.Message });
            }
        }
    }
}
