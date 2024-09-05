using azure_openai_quickstart_rate_limiting.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace azure_openai_quickstart_rate_limiting.Controllers
{
    public class BioController : Controller
    {
        private readonly GenerativeAIService _generativeAIService;
        private readonly ILogger<BioController> _logger;

        public BioController(GenerativeAIService generativeAIService, ILogger<BioController> logger)
        {
            _generativeAIService = generativeAIService;
            _logger = logger;
        }

        [Route("generate-bio")]
        public IActionResult Index(string message = null)
        {
            if (message == "rate-limit-exceeded")
            {
                ViewBag.ErrorMessage = "You have exceeded the number of allowed requests. Please try again later.";
            }

            return View();
        }

        [HttpPost]
        [EnableRateLimiting("GenerateBioPolicy")]
        [Route("generate-bio")]
        public async Task<IActionResult> GenerateBio([FromForm] string userPrompt)
        {
            _logger.LogInformation("Generating bio...");

            // Generate the bio
            var generatedBio = await _generativeAIService.GenerateBioAsync(userPrompt);

            _logger.LogInformation("Bio generated: {0}", generatedBio);

            // Return the view with the generated bio
            return View("Index", generatedBio);
        }
    }
}