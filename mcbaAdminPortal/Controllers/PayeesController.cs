using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using mcbaAdminPortal.Models;

namespace mcbaAdminPortal.Controllers
{
    public class PayeesController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _apiBaseUrl;

        public PayeesController(IHttpClientFactory clientFactory, IConfiguration config)
        {
            _clientFactory = clientFactory;
            _apiBaseUrl = config["ApiSettings:BaseUrl"] ?? "";
        }

        private HttpClient CreateClientWithToken()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            var client = _clientFactory.CreateClient();
            if (!string.IsNullOrWhiteSpace(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // ---------- LIST + FILTER ----------
        [HttpGet]
        public async Task<IActionResult> Index(string? postcode)
        {
            if (HttpContext.Session.GetString("JWTToken") == null)
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken();

            var url = string.IsNullOrWhiteSpace(postcode)
                ? $"{_apiBaseUrl}api/Payees"
                : $"{_apiBaseUrl}api/Payees/postcode/{postcode}";

            try
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Failed to fetch Payees.";
                    return View(new List<Payee>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var payees = JsonSerializer.Deserialize<List<Payee>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                ViewBag.Postcode = postcode;
                return View(payees ?? new List<Payee>());
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
                return View(new List<Payee>());
            }
        }

        // ---------- EDIT (GET) ----------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetString("JWTToken") == null)
                return RedirectToAction("Login", "Auth");

            var client = CreateClientWithToken();
            var resp = await client.GetAsync($"{_apiBaseUrl}api/Payees/{id}");
            if (!resp.IsSuccessStatusCode)
            {
                TempData["Msg"] = "Failed to load payee.";
                return RedirectToAction(nameof(Index));
            }

            var json = await resp.Content.ReadAsStringAsync();
            var payee = JsonSerializer.Deserialize<Payee>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(payee);
        }

        // ---------- EDIT (POST) ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Payee payee)
        {
            if (HttpContext.Session.GetString("JWTToken") == null)
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid) return View(payee);

            var client = CreateClientWithToken();

            var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null // Preserve PascalCase property names
            };
            var payload = new StringContent(
            JsonSerializer.Serialize(payee, options),
            Encoding.UTF8,
            "application/json"
        );

            Console.WriteLine($"[Portal] PUT URL: {_apiBaseUrl}api/Payees/{payee.PayeeID}");

            var resp = await client.PutAsync($"{_apiBaseUrl}api/Payees/{payee.PayeeID}", payload);
            if (!resp.IsSuccessStatusCode)
            {
                ViewBag.Error = "Update failed.";
                return View(payee);
            }

            TempData["Msg"] = "✅ Payee updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }

    
    
}
