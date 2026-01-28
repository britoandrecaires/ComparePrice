namespace SistemaPrecos.API.ViewModels;

public record UtilizadorListVM(
    int    UtilizadorId,
    string Nome,
    string Username,
    string Email,
    string TipoUtilizador,
    bool   Ativo);
