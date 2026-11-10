using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RaceDay.Web.ViewModels;
using System.Security.Claims;
using System.Text;

namespace RaceDay.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient();
            var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/auth/login";

            var jsonContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(apiUrl, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<LoginApiResponse>(jsonResponse);

                if (result != null)
                {
                    // Create the user's identity claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, result.User.UserId.ToString()),
                        new Claim(ClaimTypes.Name, result.User.FirstName + " " + result.User.LastName),
                        new Claim(ClaimTypes.Email, model.Email),
                        new Claim(ClaimTypes.Role, result.User.Role),
                        new Claim("JwtToken", result.Token) // Store the JWT token inside the cookie!
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // Sign the user in (creates the cookie)
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    // Redirect based on role
                    if (result.User.Role == "Organiser") return RedirectToAction("Dashboard", "Organiser");
                    if (result.User.Role == "Participant") return RedirectToAction("Index", "Participant");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpClientFactory.CreateClient();
            var apiUrl = _configuration["ApiSettings:BaseUrl"] + "/auth/register";

            var jsonContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(apiUrl, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                // Success, redirect to login
                return RedirectToAction(nameof(Login));
            }

            ModelState.AddModelError(string.Empty, "Registration failed. Email might already exist.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();
    }

    // Helper class to map the API JSON response
    public class LoginApiResponse
    {
        [JsonProperty("token")]
        public string Token { get; set; } = string.Empty;

        [JsonProperty("user")]
        public UserDto User { get; set; } = new UserDto();
    }

    public class UserDto
    {
        [JsonProperty("userId")]
        public int UserId { get; set; }
        [JsonProperty("firstName")]
        public string FirstName { get; set; } = string.Empty;
        [JsonProperty("lastName")]
        public string LastName { get; set; } = string.Empty;
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;
    }
}