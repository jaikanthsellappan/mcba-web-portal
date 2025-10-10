using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace mcbaAdminPortal.Controllers
{
    public class BillPayController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _apiBase = "https://localhost:7149/api/BillPay";

        public BillPayController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        private HttpClient CreateClientWithToken()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            var client = _clientFactory.CreateClient();
            if (token != null)
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // ----------------- LIST ALL BILLPAY ENTRIES -----------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWTToken");
            if (token == null)
                return RedirectToAction("Login", "Auth");

            try
            {
                var client = CreateClientWithToken();
                var response = await client.GetAsync(_apiBase);

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

        // ----------------- BLOCK A BILL -----------------
        [HttpPost]
        public async Task<IActionResult> Block(int id)
        {
            var client = CreateClientWithToken();
            var response = await client.PutAsync($"{_apiBase}/block/{id}", null);

            TempData["Msg"] = response.IsSuccessStatusCode
                ? "✅ Payment successfully blocked."
                : "❌ Failed to block payment.";

            return RedirectToAction("Index");
        }

        // ----------------- UNBLOCK A BILL -----------------
        [HttpPost]
        public async Task<IActionResult> Unblock(int id)
        {
            var client = CreateClientWithToken();
            var response = await client.PutAsync($"{_apiBase}/unblock/{id}", null);

            TempData["Msg"] = response.IsSuccessStatusCode
                ? "✅ Payment successfully unblocked."
                : "❌ Failed to unblock payment.";

            return RedirectToAction("Index");
        }
    }

    // ----------------- BILLPAY VIEWMODEL -----------------
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
