namespace OxfordOnline.Models.Dto
{
    public class FtpImageRequest
    {
        // Lista de URLs das imagens a serem baixadas (pode ser FTP, HTTP, etc.)
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
