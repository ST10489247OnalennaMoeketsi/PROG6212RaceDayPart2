using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RaceDay.Web.ViewModels;
using System.Security.Claims;

namespace RaceDay.Web.Controllers
{
    [Authorize(Roles = "Participant")]
    public class ParticipantController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ParticipantController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET: /Participant/Index (Home page showing upcoming events)
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var token = User.FindFirst("JwtToken")?.Value;
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/events";
            var response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var allEvents = JsonConvert.DeserializeObject<List<EventViewModel>>(jsonResponse);
                return View(allEvents);
            }

            return View(new List<EventViewModel>());
        }

        // POST: /Participant/Enrol/5
        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId)
        {
            var client = _httpClientFactory.CreateClient();
            var token = User.FindFirst("JwtToken")?.Value;
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // We need to send a CategoryId. Since we don't have a UI to select one right now, we will hardcode CategoryId = 1 for this demo.
            var apiUrl = _configuration["ApiSettings:BaseUrl"] + $"/events/{eventId}/enrolments";
            var content = new StringContent(JsonConvert.SerializeObject(new { CategoryId = 1 }), System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync(apiUrl, content);

            // Whether it succeeds or fails, go back to the events page
            return RedirectToAction(nameof(Index));
        }
    }
}