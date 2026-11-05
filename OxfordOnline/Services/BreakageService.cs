using OxfordOnline.Models;
using OxfordOnline.Models.Dto;
using OxfordOnline.Models.Enums;
using OxfordOnline.Repositories.Interfaces;
using OxfordOnline.Resources;

namespace OxfordOnline.Services
{
    // Violação do fluxo de status da quebra (HTTP 409). Tipo próprio para não ser confundido
    // com as InvalidOperationException lançadas pelo EF em falhas de infraestrutura.
    public class BreakageConflictException : Exception
    {
        public BreakageConflictException(string message) : base(message) { }
    }

    public class BreakageService
    {
        private readonly IBreakageRepository _breakageRepository;

        // Fluxo: PENDENTE -> APROVADO ou REJEITADO; APROVADO -> BAIXADO.
        private static readonly Dictionary<BreakageStatus, BreakageStatus[]> _allowedTransitions = new()
        {
            [BreakageStatus.PENDENTE] = new[] { BreakageStatus.APROVADO, BreakageStatus.REJEITADO },
            [BreakageStatus.APROVADO] = new[] { BreakageStatus.BAIXADO },
            [BreakageStatus.REJEITADO] = Array.Empty<BreakageStatus>(),
            [BreakageStatus.BAIXADO] = Array.Empty<BreakageStatus>(),
        };

        public BreakageService(IBreakageRepository breakageRepository)
        {
            _breakageRepository = breakageRepository;
        }

        public async Task<List<BreakageRecord>> GetRecentBreakagesAsync() =>
            await _breakageRepository.GetCreatedSinceAsync(DateTime.Now.AddMonths(-3));

        public async Task<(BreakageRecord Record, bool Created)> CreateOrUpdateBreakageAsync(BreakageRecordRequest request)
        {
            var created = request.BreakageId is null or 0;

            var record = created
                ? await CreateAsync(request)
                : await UpdateAsync(request);

            await _breakageRepository.SaveAsync();
            return (record, created);
        }

        public async Task<bool> DeleteBreakageAsync(uint breakageId)
        {
            var record = await _breakageRepository.GetByIdAsync(breakageId);
            if (record == null) return false;

            // Um registro BAIXADO já movimentou o estoque: excluí-lo apagaria o histórico da baixa.
            if (record.Status == BreakageStatus.BAIXADO)
                throw new BreakageConflictException(EndPointsMessages.BreakageWrittenOffCannotDelete);

            await _breakageRepository.DeleteAsync(record);
            await _breakageRepository.SaveAsync();
            return true;
        }

        private async Task<BreakageRecord> CreateAsync(BreakageRecordRequest request)
        {
            if (request.Status is not (null or BreakageStatus.PENDENTE))
                throw new ArgumentException(EndPointsMessages.BreakageMustStartPending);

            var createdBy = RequireUser(request.CreatedBy, nameof(request.CreatedBy));

            await EnsureProductExistsAsync(request.ProductId);

            var record = new BreakageRecord
            {
                ProductId = request.ProductId,
                Reason = request.Reason,
                PhotoUrl = request.PhotoUrl,
                Status = BreakageStatus.PENDENTE,
                CreatedBy = createdBy,
                CreatedAt = DateTime.Now,
            };

            if (request.Quantity is int quantity)
                record.Quantity = (uint)quantity;

            await _breakageRepository.AddAsync(record);
            return record;
        }

        private async Task<BreakageRecord> UpdateAsync(BreakageRecordRequest request)
        {
            var record = await _breakageRepository.GetByIdAsync(request.BreakageId!.Value)
                ?? throw new KeyNotFoundException(EndPointsMessages.BreakageNotFound);

            var newStatus = request.Status ?? record.Status;
            if (newStatus != record.Status && !_allowedTransitions[record.Status].Contains(newStatus))
                throw new BreakageConflictException(string.Format(
                    EndPointsMessages.BreakageInvalidTransition, record.Status, newStatus));

            // Os dados da quebra congelam após a análise: o que foi aprovado é o que será baixado.
            if (record.Status == BreakageStatus.PENDENTE)
            {
                await EnsureProductExistsAsync(request.ProductId);

                record.ProductId = request.ProductId;
                record.Reason = request.Reason;
                record.PhotoUrl = request.PhotoUrl;

                if (request.Quantity is int quantity)
                    record.Quantity = (uint)quantity;
            }

            if (newStatus != record.Status)
                ApplyStatusChange(record, newStatus, request);

            return record;
        }

        private static void ApplyStatusChange(
            BreakageRecord record, BreakageStatus newStatus, BreakageRecordRequest request)
        {
            record.Status = newStatus;

            switch (newStatus)
            {
                case BreakageStatus.REJEITADO:
                    record.RejectionReason = request.RejectionReason;
                    record.ReviewedBy = RequireUser(request.ReviewedBy, nameof(request.ReviewedBy));
                    record.ReviewedAt = DateTime.Now;
                    break;
                case BreakageStatus.APROVADO:
                    record.ReviewedBy = RequireUser(request.ReviewedBy, nameof(request.ReviewedBy));
                    record.ReviewedAt = DateTime.Now;
                    break;
                case BreakageStatus.BAIXADO:
                    record.WrittenOffBy = RequireUser(request.WrittenOffBy, nameof(request.WrittenOffBy));
                    record.WrittenOffAt = DateTime.Now;
                    break;
            }
        }

        private static string RequireUser(string? user, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(user))
                throw new ArgumentException(string.Format(EndPointsMessages.BreakageUserRequired, fieldName));

            return user;
        }

        private async Task EnsureProductExistsAsync(string productId)
        {
            if (!await _breakageRepository.ProductExistsAsync(productId))
                throw new KeyNotFoundException(EndPointsMessages.ProductNotFound);
        }
    }
}
