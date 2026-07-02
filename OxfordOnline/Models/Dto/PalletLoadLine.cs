namespace OxfordOnline.Models.Dto
{
    public class PalletLoadLine
    {
        public int LoadId { get; init; }
        public int PalletId { get; init; }
        public bool Carregado { get; init; }

        // Detalhes do Pallet
        public string PalletLocation { get; init; } = string.Empty;
        public int PalletTotalQuantity { get; init; }
    }
}
