using OxfordOnline.Utils;
using System.IO;
using System.Threading.Tasks;

namespace OxfordOnline.Repositories.Interfaces
{
    public interface IFtpServiceOxServer
    {
        Task SyncImagesAsync();
        Task RunFtpAsync();
        
    }
}
