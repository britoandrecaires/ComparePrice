using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Threading.Tasks;

namespace SistemaPrecos.Web.Pages.Conta
{
    public class CancelarContaModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public CancelarContaModel(IHttpClientFactory factory)
        {
            // Este cliente já deve ter sido configurado em Program.cs como “Api” com BaseAddress = http://localhost:5000/
            _httpClient = factory.CreateClient("Api");
        }

        // Mostra o nome do utilizador na página de confirmação
        public string NomeUtilizador { get; set; } = string.Empty;

        // Guarda o ID numérico do utilizador (vindo do cookie “userid”)
        public int UserId { get; set; }

        /// <summary>
        /// OnGet: Carrega o nome e o ID a partir dos cookies. 
        /// Se faltar qualquer cookie, redireciona ao login.
        /// </summary>
        public void OnGet()
        {
            // 1) Tenta ler o cookie “username” e atribuir a NomeUtilizador
            if (Request.Cookies.TryGetValue("username", out var nomeCookie) &&
                !string.IsNullOrEmpty(nomeCookie))
            {
                NomeUtilizador = nomeCookie;
            }
            else
            {
                // Se não existir cookie de “username”, força logout e redireciona
                Response.Redirect("/Login");
                return;
            }

            // 2) Tenta ler o cookie “userid” e converter para int
            if (Request.Cookies.TryGetValue("userid", out var idCookie) &&
                int.TryParse(idCookie, out var id))
            {
                UserId = id;
            }
            else
            {
                // Se não existir cookie de “userid” ou não converter, força logout
                Response.Redirect("/Login");
                return;
            }
        }

        /// <summary>
        /// OnPostAsync: Quando o utilizador clica em “Sim, cancelar conta”, faz uma chamada PATCH
        /// para api/auth/desativar-conta/{UserId} e, se der sucesso, apaga cookies e volta ao login.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            // Revalida o cookie de “userid” (caso tenha expirado ou sido removido no meio do caminho)
            if (!Request.Cookies.TryGetValue("userid", out var idCookie) ||
                !int.TryParse(idCookie, out var id))
            {
                // Se não encontrar ou não converter, volta ao login
                return RedirectToPage("/Login");
            }

            UserId = id;

            // Monta a mensagem HTTP de tipo PATCH para chamar “api/auth/desativar-conta/{UserId}”
            var requestMessage = new HttpRequestMessage(HttpMethod.Patch, $"auth/desativar-conta/{UserId}");
            var resposta = await _httpClient.SendAsync(requestMessage);

            if (resposta.IsSuccessStatusCode)
            {
                // Se a API devolveu 200 OK, elimina os cookies e redireciona para /Login
                Response.Cookies.Delete("username");
                Response.Cookies.Delete("tipo");
                Response.Cookies.Delete("userid");
                return RedirectToPage("/Login");
            }

            // Se falhar, mantém‐se na mesma página e exibe erro simples
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao cancelar a conta.");
            return Page();
        }
    }
}
