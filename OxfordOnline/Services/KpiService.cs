using OxfordOnline.Models.Dto;
using OxfordOnline.Repositories.Interfaces;

namespace OxfordOnline.Services
{
    public class KpiService
    {
        private readonly IKpiRepository _kpiRepository;

        public KpiService(IKpiRepository kpiRepository)
        {
            _kpiRepository = kpiRepository;
        }

        public async Task<KpiCompletudeResult> GetCompletudeAsync() =>
            await _kpiRepository.GetCompletudeAsync();
    }
}
