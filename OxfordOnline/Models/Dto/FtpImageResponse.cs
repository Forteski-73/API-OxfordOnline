namespace OxfordOnline.Models.Dto
{
    public class FtpImageResponse
    {
        // A URL original da imagem
        public string Url { get; set; }

        // O conteúdo da imagem codificado em Base64
        public string Base64Content { get; set; }

        // O status do download (ex: "Success", "Error")
        public string Status { get; set; }

        // Mensagem de erro, se houver
        public string Message { get; set; } = string.Empty;
    }
}
