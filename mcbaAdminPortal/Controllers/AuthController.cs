using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace mcbaAdminPortal.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _apiBaseUrl;

        public AuthController(IHttpClientFactory clientFactory, IConfiguration config)
        {
            _clientFactory = clientFactory;
            _apiBaseUrl = config["ApiSettings:BaseUrl"] 
                          ?? throw new Exception("API Base URL missing in appsettings.json");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var client = _clientFactory.CreateClient();
            var content = new StringContent(
                JsonSerializer.Serialize(new { username, password }),
                Encoding.UTF8,
                "application/json");

            try
            {
                var response = await client.PostAsync($"{_apiBaseUrl}api/Auth/login", content);

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Invalid login credentials.";
                    return View();
                }

                var json = await response.Content.ReadAsStringAsync();
                var token = JsonDocument.Parse(json).RootElement.GetProperty("token").GetString();

                HttpContext.Session.SetString("JWTToken", token!);
                return RedirectToAction("Index", "Payees");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Connection failed: {ex.Message}";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
