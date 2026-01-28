using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SistemaPrecos.Web.Pages.Relatorios
{
    public class EscolherRelatorioModel : PageModel
    {
        public List<SelectListItem> Lojas { get; set; } = new();
        public string NomeUtilizador { get; set; }

        public async Task OnGetAsync()
        {
            NomeUtilizador = Request.Cookies["username"];

            using var http = new HttpClient();
            var response = await http.GetAsync("http://localhost:5000/api/loja/todas");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var lojas = JsonSerializer.Deserialize<List<LojaDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                foreach (var loja in lojas)
                {
                    Lojas.Add(new SelectListItem
                    {
                        Value = loja.LojaId.ToString(),
                        Text = loja.Nome
                    });
                }
            }

            // Produtos
var responseProd = await http.GetAsync("http://localhost:5000/api/produto/todos");
if (responseProd.IsSuccessStatusCode)
{
    var json = await responseProd.Content.ReadAsStringAsync();
    var produtos = JsonSerializer.Deserialize<List<ProdutoDto>>(json, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    foreach (var p in produtos)
    {
        Produtos.Add(new SelectListItem
        {
            Value = p.ProdutoId.ToString(),
            Text = p.Nome
        });
    }
}

        }

        public class LojaDto
        {
            public int LojaId { get; set; }
            public string Nome { get; set; }
        }

        public List<SelectListItem> Produtos { get; set; } = new();

public class ProdutoDto
{
    public int ProdutoId { get; set; }
    public string Nome { get; set; }
}

    }
}
