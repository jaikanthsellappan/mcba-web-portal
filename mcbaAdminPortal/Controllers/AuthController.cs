using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace mcbaAdminPortal.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public AuthController(IHttpClientFactory clientFactory) => _clientFactory = clientFactory;

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var client = _clientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(new { username, password }),
                                            Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://localhost:7149/api/Auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var token = JsonDocument.Parse(json).RootElement.GetProperty("token").GetString();
                HttpContext.Session.SetString("JWTToken", token!);
                return RedirectToAction("Index", "Payees");
            }

            ViewBag.Error = "Invalid login credentials.";
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
