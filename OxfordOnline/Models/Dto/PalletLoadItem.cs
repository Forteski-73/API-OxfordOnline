namespace OxfordOnline.Models.Dto
{
    public class PalletLoadItem
    {
        // Chaves compostas para identificar o item
        public int PalletId { get; set; }
        public string ProductId { get; set; }
        // Nova quantidade recebida (o que o usuário digitou no Flutter)
        public int QuantityReceived { get; set; }
        // Opcional: Para auditoria
        public string? UserId { get; set; }
    }
}
