﻿using System.IO;
using System.Threading.Tasks;

namespace OxfordOnline.Repositories.Interfaces
{
    public interface IFtpService
    {
        Task UploadAsync(string remotePath, Stream content);
        Task DeleteAsync(string remotePath);
        Task<Stream> DownloadAsync(string remotePath);
    }
}
