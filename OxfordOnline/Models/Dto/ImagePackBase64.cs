namespace OxfordOnline.Models.Dto
{
    public class ImagePackBase64
    {
        public string CodeId { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public int Sequence { get; set; } = 1;
        public string? ImagesBase64 { get; set; } // Campo para a string Base64 do ZIP das imagens
    }
}
