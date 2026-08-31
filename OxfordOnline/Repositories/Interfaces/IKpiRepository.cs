using OxfordOnline.Models.Dto;

namespace OxfordOnline.Repositories.Interfaces
{
    public interface IKpiRepository
    {
        /// <summary>
        /// Calcula a completude de cadastro de imagens (Produto, Embalagem, Paletização)
        /// considerando apenas produtos com Status ativo.
        /// </summary>
        Task<KpiCompletudeResult> GetCompletudeAsync();
    }
}
