namespace OxfordOnline.Models.Dto
{
    public class ImageBase64
    {
        public string ProductId { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public int Sequence { get; set; } = 1;
        public bool ImageMain { get; set; } = false;
        public string Finalidade { get; set; } = "PRODUTO";
        public string? ImagesBase64 { get; set; } // Campo para a string Base64 do ZIP das imagens
    }
}
