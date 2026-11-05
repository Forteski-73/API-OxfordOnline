using OxfordOnline.Models.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OxfordOnline.Models.Dto
{
    /// <summary>
    /// Dados aceitos no POST de quebra. Os usuários vêm do consumidor da API; as datas de
    /// auditoria (created_at, reviewed_at e written_off_at) são preenchidas pelo servidor.
    /// </summary>
    public class BreakageRecordRequest
    {
        // Nulo ou 0 insere um novo registro; qualquer outro valor atualiza o registro existente.
        public uint? BreakageId { get; set; }

        [Required]
        [StringLength(10)]
        public string ProductId { get; set; } = string.Empty;

        // Se não for enviada: assume 1 na inserção e mantém o valor atual na atualização.
        [DefaultValue(1)]
        [Range(1, int.MaxValue)]
        public int? Quantity { get; set; }

        [Required]
        [StringLength(200)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string PhotoUrl { get; set; } = string.Empty;

        // Nulo mantém o status atual (ou PENDENTE, na inserção).
        public BreakageStatus? Status { get; set; }

        [StringLength(500)]
        public string? RejectionReason { get; set; }

        // Obrigatório na inserção; não é alterado nas atualizações.
        [StringLength(100)]
        public string? CreatedBy { get; set; }

        // Obrigatório ao mudar o status para APROVADO ou REJEITADO.
        [StringLength(100)]
        public string? ReviewedBy { get; set; }

        // Obrigatório ao mudar o status para BAIXADO.
        [StringLength(100)]
        public string? WrittenOffBy { get; set; }
    }
}
