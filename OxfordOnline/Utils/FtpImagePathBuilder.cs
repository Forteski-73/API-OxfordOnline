namespace OxfordOnline.Utils
{
    public class FtpImagePathBuilder
    {
        public string Family { get; set; }
        public string Brand { get; set; }
        public string Line { get; set; }
        public string Decoration { get; set; }
        public string ProductId { get; set; }
        public string Finalidade { get; set; }

        public FtpImagePathBuilder(string family, string brand, string line, string decoration, string productId, string finalidade)
        {

            if (string.IsNullOrWhiteSpace(family) ||
                string.IsNullOrWhiteSpace(brand) ||
                string.IsNullOrWhiteSpace(line) ||
                string.IsNullOrWhiteSpace(decoration) ||
                string.IsNullOrWhiteSpace(productId) ||
                string.IsNullOrWhiteSpace(finalidade))
            {
                throw new ArgumentException("Todos os campos devem ser preenchidos para construir o caminho.");
            }

            Family      = family.    Trim().Replace(" ", "_");
            Brand       = brand.     Trim().Replace(" ", "_");
            Line        = line.      Trim().Replace(" ", "_");
            Decoration  = decoration.Trim().Replace(" ", "_");
            ProductId   = productId. Trim();
            Finalidade  = finalidade.Trim();
        }

        public string BuildPath()
        {
            return $"{Family}/{Brand}/{Line}/{Decoration}/{ProductId}/{Finalidade}";
        }

        public override string ToString() => BuildPath();
    }
}
