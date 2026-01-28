using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaPrecos.Web.ViewModels;
using System.Net.Http;
using System.Net.Http.Json;

namespace SistemaPrecos.Web.Pages.Produtos
{
    public class CompararUtlModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public CompararUtlModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
        }

        public List<Produto> Produtos { get; set; } = new();
        public List<ComparacaoPrecoViewModel> PrecosComparados { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int ProdutoId { get; set; }

        public async Task OnGetAsync()
        {
            Produtos = await _httpClient.GetFromJsonAsync<List<Produto>>("produto");

            if (ProdutoId != 0)
            {
                PrecosComparados = await _httpClient.GetFromJsonAsync<List<ComparacaoPrecoViewModel>>($"produto/{ProdutoId}/comparar");
            }
        }
    }
}
