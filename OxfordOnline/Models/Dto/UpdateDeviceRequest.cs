namespace OxfordOnline.Models.Dto
{
    public class UpdateDeviceRequest
    {
        public string? CustomDeviceName { get; set; }
        public bool IsActive { get; set; }

        // Opcional: se enviado, atualiza o invent_header_id do InventoryGuid vinculado ao device (via Guid)
        public int? InventHeaderId { get; set; }
    }
}
