using Microsoft.EntityFrameworkCore;
using OxfordOnline.Data;
using OxfordOnline.Models.Dto;
using OxfordOnline.Models.Enums;
using OxfordOnline.Repositories.Interfaces;

namespace OxfordOnline.Repositories
{
    public class KpiRepository : IKpiRepository
    {
        private readonly AppDbContext _context;

        // KPI restrito à família 0003 / tipo P024 (aparelhos) em product_oxford.
        private const string TargetFamilyId = "0003";
        private const string TargetTypeId = "P024";

        // Categorias de completude avaliadas, na ordem exibida ao usuário.
        private static readonly (Finalidade Finalidade, string Label)[] _categorias =
        {
            (Finalidade.PRODUTO, "Produto"),
            (Finalidade.EMBALAGEM, "Embalagem"),
            (Finalidade.PALETIZACAO, "Paletização"),
        };

        public KpiRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<KpiCompletudeResult> GetCompletudeAsync()
        {
            var produtosDaFamilia =
                from p in _context.Product
                join o in _context.Oxford on p.ProductId equals o.ProductId
                where p.Status && o.FamilyId == TargetFamilyId && o.TypeId == TargetTypeId
                select p.ProductId;

            var totalProdutos = await produtosDaFamilia.Distinct().CountAsync();

            var finalidadesAvaliadas = _categorias.Select(c => c.Finalidade.ToString()).ToArray();

            // Um par (ProductId, Finalidade) por produto da família alvo que possui ao menos uma imagem daquela finalidade.
            var produtoFinalidades = await (
                from i in _context.Image
                join p in produtosDaFamilia on i.ProductId equals p
                where finalidadesAvaliadas.Contains(i.Finalidade)
                select new { ProductId = p, i.Finalidade }
            ).Distinct().ToListAsync();

            double PercentComFinalidade(string finalidade)
            {
                if (totalProdutos == 0) return 0;
                var count = produtoFinalidades.Count(x => x.Finalidade == finalidade);
                return Math.Round((double)count / totalProdutos, 4);
            }

            var produtosCompletos = produtoFinalidades
                .GroupBy(x => x.ProductId)
                .Count(g => finalidadesAvaliadas.All(f => g.Any(x => x.Finalidade == f)));

            return new KpiCompletudeResult
            {
                TotalProdutos = totalProdutos,
                ProdutosCompletos = produtosCompletos,
                CompletudeGeral = totalProdutos == 0 ? 0 : Math.Round((double)produtosCompletos / totalProdutos, 4),
                Categorias = _categorias
                    .Select(c => new KpiCompletudeCategoria
                    {
                        Label = c.Label,
                        Percent = PercentComFinalidade(c.Finalidade.ToString())
                    })
                    .ToList()
            };
        }
    }
}
