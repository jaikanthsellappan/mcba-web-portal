using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace mcbaAdminPortal.Controllers
{
    public class BillPayController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _apiBaseUrl;

        public BillPayController(IHttpClientFactory clientFactory, IConfiguration config)
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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("JWTToken") == null)
                return RedirectToAction("Login", "Auth");

            try
            {
                var client = CreateClientWithToken();
                var response = await client.GetAsync($"{_apiBaseUrl}api/BillPays");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Failed to load BillPay list from API.";
                    return View(new List<BillPayVM>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var bills = JsonSerializer.Deserialize<List<BillPayVM>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return View(bills ?? new List<BillPayVM>());
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
                return View(new List<BillPayVM>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Block(int id)
        {
            var client = CreateClientWithToken();
            var response = await client.PutAsync($"{_apiBaseUrl}api/BillPays/block/{id}", null);

            TempData["Msg"] = response.IsSuccessStatusCode
                ? "✅ Payment successfully blocked."
                : "❌ Failed to block payment.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(int id)
        {
            var client = CreateClientWithToken();
            var response = await client.PutAsync($"{_apiBaseUrl}api/BillPays/unblock/{id}", null);

            TempData["Msg"] = response.IsSuccessStatusCode
                ? "✅ Payment successfully unblocked."
                : "❌ Failed to unblock payment.";

            return RedirectToAction("Index");
        }
    }

    public class BillPayVM
    {
        public int BillPayID { get; set; }
        public int AccountNumber { get; set; }
        public int PayeeID { get; set; }
        public decimal Amount { get; set; }
        public DateTime ScheduleTimeUtc { get; set; }
        public string Period { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
