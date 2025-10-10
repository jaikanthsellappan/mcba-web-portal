using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace mcbaAdminPortal.Controllers
{
    public class PayeesController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public PayeesController(IHttpClientFactory clientFactory) => _clientFactory = clientFactory;

        public async Task<IActionResult> Index(string? postcode)
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (token == null)
                return RedirectToAction("Login", "Auth");

            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string url = string.IsNullOrWhiteSpace(postcode)
                ? "https://localhost:7149/api/Payees"
                : $"https://localhost:7149/api/Payees/postcode/{postcode}";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Failed to fetch Payees.";
                return View();
            }

            var json = await response.Content.ReadAsStringAsync();
            var payees = JsonSerializer.Deserialize<List<Payee>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(payees);
        }
    }

    public class Payee
    {
        public int PayeeID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Postcode { get; set; }
        public string? Phone { get; set; }
    }
}
