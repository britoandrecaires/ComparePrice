using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using SistemaPrecos.Web.ViewModels;

namespace SistemaPrecos.Web.Pages.Produtos
{
    public class ListaAdminModel : PageModel
    {
        private readonly HttpClient _http;

        public ListaAdminModel(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("Api");
        }

        public List<Produto> Produtos { get; set; } = new();
        public List<Categoria> Categorias { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? TermoPesquisa { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoriaId { get; set; }

        public async Task OnGetAsync()
        {
            // Carregar categorias
            Categorias = await _http.GetFromJsonAsync<List<Categoria>>("categoria") ?? new();

            // Carregar todos os produtos
            var todosProdutos = await _http.GetFromJsonAsync<List<Produto>>("produto") ?? new();

            // Filtro por nome
            if (!string.IsNullOrWhiteSpace(TermoPesquisa))
            {
                todosProdutos = todosProdutos
                    .Where(p => p.Nome.Contains(TermoPesquisa, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Filtro por categoria
            if (CategoriaId.HasValue)
            {
                todosProdutos = todosProdutos
                    .Where(p => p.CategoriaId == CategoriaId.Value)
                    .ToList();
            }

            Produtos = todosProdutos;
        }
    }
}