using OxfordOnline.Models;
using OxfordOnline.Repositories.Interfaces;

namespace OxfordOnline.Services
{
    public class TaxInformationService
    {
        private readonly ITaxInformationRepository _taxInformation;

        public TaxInformationService(ITaxInformationRepository taxInformation)
        {
            _taxInformation = taxInformation;
        }

        public async Task<TaxInformation?> GetTaxInformationByProductIdAsync(string productId) =>
            await _taxInformation.GetByProductIdAsync(productId);

        public async Task<List<TaxInformation>> GetTaxInformationListByProductIdsAsync(List<string> productIds) =>
            await _taxInformation.GetByProductListIdsAsync(productIds);

        public async Task<bool> CreateOrUpdateTaxInformationsAsync(List<TaxInformation> taxInfos)
        {
            foreach (var taxInfo in taxInfos)
            {
                var existing = await _taxInformation.GetByProductIdAsync(taxInfo.ProductId);
                if (existing != null)
                {
                    existing.TaxationOrigin             = taxInfo.TaxationOrigin;
                    existing.TaxFiscalClassification    = taxInfo.TaxFiscalClassification;
                    existing.ProductType                = taxInfo.ProductType;
                    existing.CestCode                   = taxInfo.CestCode;
                    existing.FiscalGroupId              = taxInfo.FiscalGroupId;
                    existing.ApproxTaxValueFederal      = taxInfo.ApproxTaxValueFederal;
                    existing.ApproxTaxValueState        = taxInfo.ApproxTaxValueState;
                    existing.ApproxTaxValueCity         = taxInfo.ApproxTaxValueCity;

                    await _taxInformation.UpdateAsync(existing);
                }
                else
                {
                    await _taxInformation.AddAsync(taxInfo);
                }
            }

            await _taxInformation.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateTaxInformationAsync(TaxInformation taxInfo)
        {
            await _taxInformation.UpdateAsync(taxInfo);
            await _taxInformation.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteTaxInformationAsync(string productId)
        {
            var taxInfo = await _taxInformation.GetByProductIdAsync(productId);
            if (taxInfo == null) return false;

            await _taxInformation.DeleteAsync(taxInfo);
            await _taxInformation.SaveAsync();
            return true;
        }
    }
}
