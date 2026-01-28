using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaPrecos.API.Data;
using SistemaPrecos.API.Repositories;
using SistemaPrecos.API.ViewModels;
using SistemaPrecos.API.Models;


namespace SistemaPrecos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LojaController : ControllerBase
    {
        private readonly SistemaPrecosContext _context;

        public LojaController(SistemaPrecosContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LojaViewModel>>> GetLojas()
        {
            var lojas = await _context.Lojas
                .Include(l => l.Localizacao)
                .Select(l => new LojaViewModel
                {
                    LojaId = l.LojaId,
                    Nome = l.Nome,
                    Localizacao = new LocalizacaoViewModel
                    {
                        LocalizacaoId = l.Localizacao.LocalizacaoId,
                        Cidade = l.Localizacao.Cidade,
                        Pais = l.Localizacao.Pais,
                        CodigoPostal = l.Localizacao.CodigoPostal,
                        Latitude = l.Localizacao.Latitude,
                        Longitude = l.Localizacao.Longitude
                    }
                })
                .ToListAsync();

            return Ok(lojas);
        }

        [HttpPost]
        public async Task<ActionResult> PostLoja(LojaViewModel vm)
        {
            // cria Localizacao
            var loc = new Localizacao
            {
                Cidade        = vm.Localizacao.Cidade,
                Pais          = vm.Localizacao.Pais,
                CodigoPostal  = vm.Localizacao.CodigoPostal,
                Latitude      = vm.Localizacao.Latitude,
                Longitude     = vm.Localizacao.Longitude
            };
            _context.Localizacoes.Add(loc);
            await _context.SaveChangesAsync();

            // cria Loja
            var loja = new Loja
            {
                Nome          = vm.Nome,
                LocalizacaoId = loc.LocalizacaoId
            };
            _context.Lojas.Add(loja);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLojas), new { id = loja.LojaId }, null);
        }

        [HttpGet("todas")]
public async Task<ActionResult<IEnumerable<Loja>>> GetTodasLojas()
{
    return await _context.Lojas.ToListAsync();
}


    }
}
