using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaPrecos.Web.ViewModels;
using System.Net.Http.Json;

public class RegistarPrecoModel : PageModel
{
    // ---------- propriedades de binding ----------
    [BindProperty(SupportsGet = true)] public int ProdutoId { get; set; }

    [BindProperty] public int LojaId { get; set; }
    [BindProperty] public decimal Preco { get; set; }

    public List<SelectListItem> Produtos { get; set; } = new();
    public List<SelectListItem> Lojas    { get; set; } = new();
    public List<PrecoLinhaVM>?  ListaPrecos { get; set; }

    public string? Erro { get; set; }

    public async Task OnGetAsync() => await CarregarDadosAsync();

    // ---------- POST : registar novo preço ----------
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await CarregarDadosAsync();
            return Page();
        }

        // valida cookie de utilizador
        if (!Request.Cookies.TryGetValue("userid", out var idStr) || !int.TryParse(idStr, out var utilizadorId))
        {
            Erro = "Utilizador não autenticado.";
            await CarregarDadosAsync();
            return Page();
        }

        using var client = new HttpClient { BaseAddress = new("http://localhost:5000") };

        var payload = new RegistoPrecoViewModel
        {
            ProdutoId    = ProdutoId,
            LojaId       = LojaId,
            Preco        = Preco,
            UtilizadorId = utilizadorId,
            TipoAcaoId   = 1
        };

        var resp = await client.PostAsJsonAsync("/api/registopreco", payload);
        if (!resp.IsSuccessStatusCode)
            Erro = $"Falha ao registar: {resp.StatusCode}";

        //  PRG: redirecciona para GET para limpar repost
        return RedirectToPage(new { ProdutoId });
    }

    // ---------- helpers ----------
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

        // carrega preços se já foi seleccionado
        if (ProdutoId > 0)
            ListaPrecos = await client.GetFromJsonAsync<List<PrecoLinhaVM>>($"/api/registopreco/produto/{ProdutoId}");

        // dropdown Lojas (só precisas se Produto escolhido)
        if (ProdutoId > 0)
        {
            var lojas = await client.GetFromJsonAsync<List<LojaViewModel>>("/api/loja");
            Lojas = lojas.Select(l => new SelectListItem
            {
                Value = l.LojaId.ToString(),
                Text  = l.Nome
            }).ToList();
        }
    }

    // ---------- view-model para tabela ----------
    public record PrecoLinhaVM(string Loja, string Localizacao, decimal Preco, DateTime DataRegisto, double Credibilidade);
}
