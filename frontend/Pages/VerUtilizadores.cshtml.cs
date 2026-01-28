using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaPrecos.Web.ViewModels;
using System.Net.Http.Json;

public class VerUtilizadoresModel : PageModel
{
    public List<UtilizadorListVM> Utilizadores { get; set; } = new();
    public string? MensagemOk { get; set; }
    public string? Erro       { get; set; }

    // ---------- GET ----------
    public async Task OnGetAsync()
    {
        using var http = new HttpClient { BaseAddress = new("http://localhost:5000") };
        Utilizadores = await http.GetFromJsonAsync<List<UtilizadorListVM>>("/api/auth") ?? new();
    }

    // ---------- POST : ativar ----------
    public async Task<IActionResult> OnPostAtivarAsync(int id)
    {
        using var http = new HttpClient { BaseAddress = new("http://localhost:5000") };
        var resp = await http.PatchAsync($"/api/auth/ativar-conta/{id}", null);

        MensagemOk = resp.IsSuccessStatusCode ? "Conta ativada." : null;
        Erro       = resp.IsSuccessStatusCode ? null : $"Falha ao ativar (HTTP {(int)resp.StatusCode})";

        return RedirectToPage();
    }

    // ---------- POST : eliminar ----------
    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        using var http = new HttpClient { BaseAddress = new("http://localhost:5000") };
        var resp = await http.DeleteAsync($"/api/auth/eliminar-conta/{id}");

        MensagemOk = resp.IsSuccessStatusCode ? "Conta eliminada." : null;
        Erro       = resp.IsSuccessStatusCode ? null : $"Falha ao eliminar (HTTP {(int)resp.StatusCode})";

        return RedirectToPage();
    }

    // ---------- VM ----------
    public record UtilizadorListVM(int UtilizadorId, string Nome, string Username, string Email, string TipoUtilizador, bool Ativo);
}
