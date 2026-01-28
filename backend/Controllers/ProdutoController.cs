using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaPrecos.API.Data;
using SistemaPrecos.API.Models;
using SistemaPrecos.API.ViewModels;

namespace SistemaPrecos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutoController : ControllerBase
    {
        private readonly SistemaPrecosContext _context;

        public ProdutoController(SistemaPrecosContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
        {
            return await _context.Produtos.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetProduto(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
            {
                return NotFound();
            }
            return produto;
        }

        // POST: api/produto
        [HttpPost]
        public async Task<ActionResult<Produto>> PostProduto(ProdutoViewModel vm)
        {
            var produto = new Produto
            {
                Nome        = vm.Nome,
                Marca       = vm.Marca,
                Descricao   = vm.Descricao,
                CategoriaId = vm.CategoriaId
            };

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduto), new { id = produto.ProdutoId }, produto);
        }


        // ✅ NOVO ENDPOINT: Comparar preços mais recentes por loja
        [HttpGet("{id}/comparar")]
        public async Task<ActionResult<IEnumerable<ComparacaoPrecoViewModel>>> CompararPrecos(int id)
        {
            var precos = await _context.RegistoPrecos
                .Where(r => r.ProdutoId == id)
                .GroupBy(r => r.LojaId)
                .Select(g => g.OrderByDescending(r => r.DataRegisto).First())
                .ToListAsync();

            var lojas = await _context.Lojas
                .Include(l => l.Localizacao)
                .ToListAsync();

            var resultado = precos
                .Join(lojas,
                    preco => preco.LojaId,
                    loja => loja.LojaId,
                    (preco, loja) => new ComparacaoPrecoViewModel
                    {
                        NomeLoja = loja.Nome,
                        Localizacao = $"{loja.Localizacao.Cidade} ({loja.Localizacao.CodigoPostal})",
                        Preco = preco.Preco,
                        Data = preco.DataRegisto
                    })
                .ToList();

            return Ok(resultado);
        }
        [HttpGet("todos")]
public async Task<ActionResult<IEnumerable<Produto>>> GetTodosProdutos()
{
    return await _context.Produtos.ToListAsync();
}


    }
}
