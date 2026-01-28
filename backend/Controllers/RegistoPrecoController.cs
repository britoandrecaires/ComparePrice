using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaPrecos.API.Data;
using SistemaPrecos.API.Models;
using SistemaPrecos.API.ViewModels;

[ApiController]
[Route("api/registopreco")]
public class RegistoController : ControllerBase
{
    private readonly SistemaPrecosContext _ctx;
    public RegistoController(SistemaPrecosContext ctx) => _ctx = ctx;

    [HttpPost]
    public async Task<IActionResult> Post(RegistoPrecoViewModel vm)
    {
        if (!await _ctx.Produtos.AnyAsync(p => p.ProdutoId == vm.ProdutoId))
            return BadRequest(new { Erro = "Produto inexistente" });

        if (!await _ctx.Lojas.AnyAsync(l => l.LojaId == vm.LojaId))
            return BadRequest(new { Erro = "Loja inexistente" });

        var reg = new RegistoPreco
        {
            ProdutoId = vm.ProdutoId,
            LojaId = vm.LojaId,
            Preco = vm.Preco,
            UtilizadorId = vm.UtilizadorId,
            TipoAcaoId = vm.TipoAcaoId,
            DataRegisto = DateTime.UtcNow,
            Credibilidade = 5
        };

        _ctx.RegistoPrecos.Add(reg);
        await _ctx.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("produto/{produtoId:int}")]
    public async Task<ActionResult<IEnumerable<PrecoLinhaVM>>> PorProduto(int produtoId)
    {
        var dados = await
            (from r in _ctx.RegistoPrecos
             join l in _ctx.Lojas on r.LojaId equals l.LojaId
             join loc in _ctx.Localizacoes on l.LocalizacaoId equals loc.LocalizacaoId
             where r.ProdutoId == produtoId
             orderby r.DataRegisto descending
             select new
             {
                 r.RegistoPrecoId,                       //  ←  novo campo
                 l.Nome,
                 Localizacao = loc.Cidade + " (" + loc.CodigoPostal + ")",
                 r.Preco,
                 r.DataRegisto
             })
            .ToListAsync();

        var agora = DateTime.UtcNow;
        const double decaimentoDia = 0.02;

        var result = dados.Select(x =>
        {
            var dias = (agora - x.DataRegisto).TotalDays;
            var cred = Math.Max(0, 1 - dias * decaimentoDia);

            return new PrecoLinhaVM(
                x.RegistoPrecoId,
                x.Nome,
                x.Localizacao,
                x.Preco,
                x.DataRegisto,
                cred
            );
        });

        return Ok(result);
    }

    [HttpPut("{registoPrecoId:int}/confirmar")]
    public async Task<IActionResult> Confirmar(int registoPrecoId)
    {
        var reg = await _ctx.RegistoPrecos.FindAsync(registoPrecoId);
        if (reg is null) return NotFound();

        reg.DataRegisto = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    //PrecosAtuais
    [HttpGet("produto/{produtoId:int}/atuais")]
    public async Task<ActionResult<IEnumerable<PrecoAtualVM>>> PrecosAtuais(int produtoId)
    {
        // A) 1º passo: para cada loja, descobrir a data MAIS RECENTE
        var ultimasDatas =
            from r in _ctx.RegistoPrecos
            where r.ProdutoId == produtoId
            group r by r.LojaId into g
            select new
            {
                LojaId     = g.Key,
                UltimaData = g.Max(x => x.DataRegisto)
            };

        // B) 2º passo: voltar a ligar ao registo que tem exactamente essa data
        var query =
            from r   in _ctx.RegistoPrecos
            join u   in ultimasDatas
                    on new { r.LojaId, r.DataRegisto }
                    equals new { u.LojaId, DataRegisto = u.UltimaData }
            join l   in _ctx.Lojas        on r.LojaId        equals l.LojaId
            join loc in _ctx.Localizacoes on l.LocalizacaoId equals loc.LocalizacaoId
            where r.ProdutoId == produtoId
            select new
            {
                l.LojaId,
                Loja        = l.Nome,
                Localizacao = loc.Cidade + " (" + loc.CodigoPostal + ")",
                r.Preco,
                r.DataRegisto
            };

        // C) executa em SQL, depois ordena/projeta em memória
        var dados = await query.ToListAsync();

        var resultado = dados
            .OrderByDescending(x => x.DataRegisto)   // mais recentes 1º
            .ThenBy(x => x.Preco)                    // depois mais baratos
            .Select(x => new PrecoAtualVM(
                x.LojaId,
                x.Loja,
                x.Localizacao,
                x.Preco,
                x.DataRegisto))
            .ToList();

        return Ok(resultado);
    }

    public record PrecoAtualVM(
        int      LojaId,
        string   Loja,
        string   Localizacao,
        decimal  Preco,
        DateTime DataRegisto);

    public record PrecoLinhaVM(
        int RegistoPrecoId,
        string Loja,
        string Localizacao,
        decimal Preco,
        DateTime DataRegisto,
        double Credibilidade);
}