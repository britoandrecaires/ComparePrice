using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaPrecos.API.Data;
using SistemaPrecos.API.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.IO;
using System.Linq;


namespace SistemaPrecos.API.Controllers
{
    [Route("admin/relatorios")]
    public class RelatorioController : Controller
    {
        private readonly SistemaPrecosContext _context;

        public RelatorioController(SistemaPrecosContext context)
        {
            _context = context;
        }

        // Relatório Geral de Lojas: localização e nº de produtos por categoria (versão PDF)
        [HttpGet("gerar-pdf/geral-lojas")]
public IActionResult GerarRelatorioGeralLojasPdf()
{
    var lojas = _context.Lojas
        .Include(l => l.Localizacao)
        .ToList();

    var categorias = _context.Categorias.ToList();
    var produtos = _context.Produtos.ToList();
    var precos = _context.RegistoPrecos.ToList();

    using var stream = new MemoryStream();
    var doc = new PdfDocument();
    var page = doc.AddPage();
    var gfx = XGraphics.FromPdfPage(page);
    var font = new XFont("Verdana", 12, XFontStyle.Regular);
    double y = 40;

    gfx.DrawString("Relatório Geral de Lojas", new XFont("Verdana", 16, XFontStyle.Bold), XBrushes.Black, new XRect(0, y, page.Width, 30), XStringFormats.TopCenter);
    y += 40;

    foreach (var loja in lojas)
    {
        gfx.DrawString($"Loja: {loja.Nome}", font, XBrushes.Black, 40, y);
        y += 20;
        gfx.DrawString($"Localização: {loja.Localizacao.Cidade} ({loja.Localizacao.CodigoPostal})", font, XBrushes.Black, 60, y);
        y += 20;

        // Produtos associados à loja (via RegistoPreco)
        var produtosDaLoja = precos
            .Where(p => p.LojaId == loja.LojaId)
            .Select(p => produtos.FirstOrDefault(prod => prod.ProdutoId == p.ProdutoId))
            .Where(p => p != null)
            .GroupBy(p => p.CategoriaId)
            .Select(g => new {
                Categoria = categorias.FirstOrDefault(c => c.CategoriaId == g.Key)?.Nome ?? "Desconhecida",
                Quantidade = g.Count()
            });

        foreach (var cat in produtosDaLoja)
        {
            gfx.DrawString($"- {cat.Categoria}: {cat.Quantidade} produto(s)", font, XBrushes.Gray, 80, y);
            y += 18;
        }

        y += 25;

        if (y > page.Height - 100)
        {
            page = doc.AddPage();
            gfx = XGraphics.FromPdfPage(page);
            y = 40;
        }
    }

    doc.Save(stream, false);
    stream.Position = 0;

    return File(stream.ToArray(), "application/pdf", "RelatorioGeralLojas.pdf");
}

[HttpGet("gerar-pdf/loja")]
public IActionResult GerarRelatorioPorLojaPdf(int id)
{
    var loja = _context.Lojas
        .Include(l => l.Localizacao)
        .FirstOrDefault(l => l.LojaId == id);

    if (loja == null) return NotFound();

    var produtos = _context.Produtos.ToList();

    var registos = _context.RegistoPrecos
        .Where(r => r.LojaId == id)
        .GroupBy(r => r.ProdutoId)
        .Select(g => g.OrderByDescending(r => r.DataRegisto).First())
        .ToList();

    using var stream = new MemoryStream();
    var doc = new PdfDocument();
    var page = doc.AddPage();
    var gfx = XGraphics.FromPdfPage(page);
    var font = new XFont("Verdana", 12, XFontStyle.Regular);
    double y = 40;

    gfx.DrawString("Relatório da Loja", new XFont("Verdana", 16, XFontStyle.Bold), XBrushes.Black,
        new XRect(0, y, page.Width, 30), XStringFormats.TopCenter);
    y += 40;

    gfx.DrawString($"Loja: {loja.Nome}", font, XBrushes.Black, 40, y); y += 20;
    gfx.DrawString($"Localização: {loja.Localizacao.Cidade} ({loja.Localizacao.CodigoPostal})", font, XBrushes.Black, 40, y); y += 30;

    foreach (var reg in registos)
    {
        var produto = produtos.FirstOrDefault(p => p.ProdutoId == reg.ProdutoId);
        if (produto == null) continue;

        gfx.DrawString($"Produto: {produto.Nome}", font, XBrushes.Black, 60, y); y += 20;
        gfx.DrawString($"Preço Atual: {reg.Preco:C}", font, XBrushes.Gray, 80, y); y += 25;

        if (y > page.Height - 100)
        {
            page = doc.AddPage();
            gfx = XGraphics.FromPdfPage(page);
            y = 40;
        }
    }

    doc.Save(stream, false);
    stream.Position = 0;

    return File(stream.ToArray(), "application/pdf", $"RelatorioLoja_{id}.pdf");
}

[HttpGet("gerar-pdf/produto")]
public IActionResult GerarRelatorioPorProdutoPdf(int id)
{
    var produto = _context.Produtos.FirstOrDefault(p => p.ProdutoId == id);
    if (produto == null)
        return NotFound();

    var lojas = _context.Lojas.ToList();
    var registos = _context.RegistoPrecos
        .Where(r => r.ProdutoId == id)
        .GroupBy(r => r.LojaId)
        .Select(g => g.OrderByDescending(r => r.DataRegisto).First())
        .ToList();

    using var stream = new MemoryStream();
    var doc = new PdfDocument();
    var page = doc.AddPage();
    var gfx = XGraphics.FromPdfPage(page);
    var font = new XFont("Verdana", 12, XFontStyle.Regular);
    double y = 40;

    gfx.DrawString("Relatório por Produto", new XFont("Verdana", 16, XFontStyle.Bold), XBrushes.Black,
        new XRect(0, y, page.Width, 30), XStringFormats.TopCenter);
    y += 40;

    gfx.DrawString($"Produto: {produto.Nome}", font, XBrushes.Black, 40, y); y += 30;

    foreach (var reg in registos)
    {
        var loja = lojas.FirstOrDefault(l => l.LojaId == reg.LojaId);
        if (loja == null) continue;

        gfx.DrawString($"Loja: {loja.Nome}", font, XBrushes.Black, 60, y); y += 20;
        gfx.DrawString($"Preço Atual: {reg.Preco:C}", font, XBrushes.Gray, 80, y); y += 25;

        if (y > page.Height - 100)
        {
            page = doc.AddPage();
            gfx = XGraphics.FromPdfPage(page);
            y = 40;
        }
    }

    doc.Save(stream, false);
    stream.Position = 0;

    return File(stream.ToArray(), "application/pdf", $"RelatorioProduto_{id}.pdf");
}


    }
}
