using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaPrecos.API.Data;
using SistemaPrecos.API.Models;
using SistemaPrecos.API.ViewModels;

namespace SistemaPrecos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        
        private readonly SistemaPrecosContext _context;

        public AuthController(SistemaPrecosContext context)
        {
            _context = context;
        }

        // ── LOGIN ─────────────────────────────────────────────────────────────────

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var utilizador = _context.Utilizadores
                .Include(u => u.TipoUtilizador)
                .FirstOrDefault(u =>
                    u.Username == request.Username &&
                    u.Password == request.Password);

            // Se não existir OU se o campo Ativo for false, nega o acesso
            if (utilizador == null || !utilizador.Ativo)
            {
                return Unauthorized();
            }

            return Ok(new LoginResponse
            {
                Nome = utilizador.Nome,
                Tipo = utilizador.TipoUtilizador.Tipo, // "Administrador" ou "Utilizador"
                UtilizadorId = utilizador.UtilizadorId
            });
        }

        // ── REGISTAR ───────────────────────────────────────────────────────────────

        [HttpPost("registar")]
        public async Task<IActionResult> Registar([FromBody] UtilizadorCreateViewModel novoUser)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar se já existe utilizador com o mesmo username ou email
            var existente = await _context.Utilizadores
                .AnyAsync(u => u.Username == novoUser.Username || u.Email == novoUser.Email);

            if (existente)
                return Conflict("Já existe um utilizador com esse username ou email.");

            var utilizador = new Utilizador
            {
                Nome = novoUser.Nome,
                Username = novoUser.Username,
                Email = novoUser.Email,
                Password = novoUser.Password,
                TipoUtilizadorId = 2, // Tipo 'Utilizador' por defeito
                Ativo = true          // Ativo por padrão
            };

            _context.Utilizadores.Add(utilizador);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Utilizador registado com sucesso." });
        }

        // ── DESATIVAR CONTA ────────────────────────────────────────────────────────

        [HttpPatch("desativar-conta/{id}")]
        public async Task<IActionResult> DesativarConta(int id)
        {
            // Procura o utilizador pelo ID
            var utilizador = await _context.Utilizadores.FindAsync(id);
            if (utilizador == null)
                return NotFound("Utilizador não encontrado.");

            // Marca-o como inativo
            utilizador.Ativo = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Conta desativada com sucesso." });
        }


        // ── ELIMINAR CONTA (hard-delete + remoção de dependências) ──────────────
        [HttpDelete("eliminar-conta/{id:int}")]
        public async Task<IActionResult> EliminarConta(int id)
        {
            var utilizador = await _context.Utilizadores.FindAsync(id);
            if (utilizador is null)
                return NotFound("Utilizador não encontrado.");

            // 1) remove Registos de Preço que pertençam a este utilizador
            _context.RegistoPrecos.RemoveRange(
                _context.RegistoPrecos.Where(r => r.UtilizadorId == id));

            // 2) remove o próprio utilizador
            _context.Utilizadores.Remove(utilizador);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Conta eliminada com sucesso." });
        }

        // ── ATIVAR CONTA ────────────────────────────────────────────────────────────
        [HttpPatch("ativar-conta/{id:int}")]
        public async Task<IActionResult> AtivarConta(int id)
        {
            var utilizador = await _context.Utilizadores.FindAsync(id);
            if (utilizador is null) return NotFound("Utilizador não encontrado.");

            utilizador.Ativo = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Conta ativada com sucesso." });
        }
        
        // ── LISTAR TODOS ────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IEnumerable<UtilizadorListVM>> GetAll()
        {
            return await _context.Utilizadores
                .Include(u => u.TipoUtilizador)
                .Select(u => new UtilizadorListVM(
                    u.UtilizadorId,
                    u.Nome,
                    u.Username,
                    u.Email,
                    u.TipoUtilizador.Tipo,
                    u.Ativo))
                .ToListAsync();
        }
    }
}
