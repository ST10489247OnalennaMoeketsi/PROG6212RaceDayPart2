using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RaceDay.Web.ViewModels;
using System.Security.Claims;

namespace RaceDay.Web.Controllers
{
    [Authorize(Roles = "Organiser")]
    public class OrganiserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public OrganiserController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IActionResult> Dashboard()
        {
            var client = _httpClientFactory.CreateClient();
            var token = User.FindFirst("JwtToken")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/events";

            try
            {
                var response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var allEvents = JsonConvert.DeserializeObject<List<EventViewModel>>(jsonResponse);
                    var organiserName = User.Identity?.Name;
                    var myEvents = allEvents?.Where(e => e.OrganiserName == organiserName).ToList() ?? new List<EventViewModel>();
                    return View(myEvents);
                }
            }
            catch (Exception) { return View(new List<EventViewModel>()); }

            return View(new List<EventViewModel>());
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreateEventViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient();
            var token = User.FindFirst("JwtToken")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Account");

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/events";

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.EventName ?? ""), "EventName");
            content.Add(new StringContent(model.Description ?? ""), "Description");
            content.Add(new StringContent(model.Location ?? ""), "Location");
            content.Add(new StringContent(model.EventDate.ToString("yyyy-MM-ddTHH:mm:ss")), "EventDate");
            content.Add(new StringContent(model.RegistrationDeadline.ToString("yyyy-MM-ddTHH:mm:ss")), "RegistrationDeadline");
            content.Add(new StringContent("Open"), "Status");

            try
            {
                var response = await client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Dashboard));
                }
                var errorMsg = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"API Error: {errorMsg}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Connection Error: {ex.Message}");
            }

            return View(model);
        }
    }
}