using OxfordOnline.Models.Dto;
using System.Threading.Tasks;

namespace OxfordOnline.Repositories.Interfaces
{
    public interface ISeniorAuthService
    {
        Task<string> GetSeniorTokenAsync();
        Task<SeniorEmployeeData?> ValidateBadgeAsync(string badgeCode);
    }
}