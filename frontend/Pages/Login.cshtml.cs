using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaPrecos.Web.ViewModels;
using System.Net.Http.Json;

namespace SistemaPrecos.Web.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string? Erro { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                Erro = "Preenche todos os campos.";
                return Page();
            }

            var client = _httpClientFactory.CreateClient("Api");

            var login = new LoginRequest
            {
                Username = Username.Trim(),
                Password = Password
            };

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsJsonAsync("auth/login", login);
            }
            catch
            {
                Erro = "Não foi possível contactar a API. Verifica se o backend está a correr.";
                return Page();
            }

            if (!response.IsSuccessStatusCode)
            {
                Erro = "Credenciais inválidas. Verifica o nome de utilizador e a password.";
                return Page();
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result is null)
            {
                Erro = "Resposta inválida da API (sem dados).";
                return Page();
            }

            // Normalizar tipo (API pode devolver: ADMIN/USER ou Administrador/Utilizador)
            var tipo = (result.Tipo ?? string.Empty).Trim();

            var isAdmin =
                string.Equals(tipo, "ADMIN", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(tipo, "Administrador", StringComparison.OrdinalIgnoreCase);

            // Cookies com opções seguras + disponíveis em todo o site
            var cookieOptions = new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps // em localhost http fica false automaticamente
            };

            Response.Cookies.Append("username", result.Nome ?? string.Empty, cookieOptions);
            Response.Cookies.Append("tipo", tipo, cookieOptions);
            Response.Cookies.Append("userid", result.UtilizadorId.ToString(), cookieOptions);

            // Redirecionar com base no tipo
            return isAdmin
                ? RedirectToPage("/MainAdmin")
                : RedirectToPage("/MainUtilizador");
        }
    }
}
