using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using SistemaPrecos.Web.ViewModels;

public class PrecosAtuaisModel : PageModel
{
    // ---------- binding ----------
    [BindProperty(SupportsGet = true)] public int ProdutoId { get; set; }

    // ---------- dados p/ view ----------
    public List<SelectListItem> Produtos { get; set; } = new();
    public List<PrecoAtualVM>? PrecosAtuais { get; set; }
    public string? Erro { get; set; }

    // ---------- GET ----------
    public async Task OnGetAsync() => await CarregarAsync();

    // ---------- helpers ----------
    private async Task CarregarAsync()
    {
        using var client = new HttpClient { BaseAddress = new("http://localhost:5000") };

        // dropdown Produtos
        var prods = await client.GetFromJsonAsync<List<ProdutoViewModel>>("/api/produto");
        Produtos = prods.Select(p => new SelectListItem
        {
            Value = p.ProdutoId.ToString(),
            Text = $"{p.Marca} {p.Nome}"
        }).ToList();

        // tabela (se já foi escolhido produto)
        if (ProdutoId > 0)
        {
            var url = $"/api/registopreco/produto/{ProdutoId}/atuais";
            PrecosAtuais = await client.GetFromJsonAsync<List<PrecoAtualVM>>(url);
        }
    }

    // ---------- VM ----------
    public record PrecoAtualVM(
        int LojaId,
        string Loja,
        string Localizacao,
        decimal Preco,
        DateTime DataRegisto);
}
