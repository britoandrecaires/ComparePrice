using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaPrecos.Web.ViewModels;
using System.Net.Http.Json;

public class ConfirmarPrecoModel : PageModel
{
    [BindProperty(SupportsGet = true)] public int ProdutoId { get; set; }

    public List<SelectListItem> Produtos { get; set; } = new();
    public List<PrecoLinhaVM>?  ListaPrecos { get; set; }
    public string? Erro { get; set; }

    public async Task OnGetAsync() => await CarregarDadosAsync();

    public async Task<IActionResult> OnPostConfirmarAsync(int id)
    {
        using var client = new HttpClient { BaseAddress = new("http://localhost:5000") };
        var resp = await client.PutAsync($"/api/registopreco/{id}/confirmar", null);

        if (!resp.IsSuccessStatusCode)
            Erro = $"Falha ao confirmar: {resp.StatusCode}";

        // PRG
        return RedirectToPage(new { ProdutoId });
    }

    private async Task CarregarDadosAsync()
    {
        using var client = new HttpClient { BaseAddress = new("http://localhost:5000") };

        // dropdown Produtos
        var prods = await client.GetFromJsonAsync<List<ProdutoViewModel>>("/api/produto");
        Produtos = prods.Select(p => new SelectListItem
        {
            Value = p.ProdutoId.ToString(),
            Text  = $"{p.Marca} {p.Nome}"
        }).ToList();

        // tabela de preços
        if (ProdutoId > 0)
            ListaPrecos = await client.GetFromJsonAsync<List<PrecoLinhaVM>>
                ($"/api/registopreco/produto/{ProdutoId}");
    }

    // id incluído
    public record PrecoLinhaVM(
        int      RegistoPrecoId,
        string   Loja,
        string   Localizacao,
        decimal  Preco,
        DateTime DataRegisto,
        double   Credibilidade);
}
