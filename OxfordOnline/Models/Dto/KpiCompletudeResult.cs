namespace OxfordOnline.Models.Dto
{
    public class KpiCompletudeCategoria
    {
        public string Label { get; set; } = string.Empty;
        public double Percent { get; set; }
    }

    public class KpiCompletudeResult
    {
        public double CompletudeGeral { get; set; }
        public int TotalProdutos { get; set; }
        public int ProdutosCompletos { get; set; }
        public List<KpiCompletudeCategoria> Categorias { get; set; } = new();
    }
}
