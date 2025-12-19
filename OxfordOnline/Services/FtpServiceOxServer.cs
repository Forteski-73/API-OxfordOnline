using Microsoft.Extensions.Configuration;
using OxfordOnline.Models;
using OxfordOnline.Models.Enums;
using OxfordOnline.Repositories;
using OxfordOnline.Repositories.Interfaces;
using OxfordOnline.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace OxfordOnline.Services
 {
    public class FtpServiceOxServer : IFtpServiceOxServer
    {
        private readonly string _ftpHost;
        private readonly string _ftpUser;
        private readonly string _ftpPassword;
        private readonly ProductService _productService;
        private readonly ImageService _imageService;
        private readonly OxfordService _oxfordService;
        private readonly ILogger<FtpServiceOxServer> _logger;

        private readonly IImageRepository _imageRepository;
        private readonly IFtpService _ftpService; // FTP destino

        public FtpServiceOxServer(
            IConfiguration configuration,
            ProductService productService,
            ImageService imageService,
            OxfordService oxfordService,
            ILogger<FtpServiceOxServer> logger,
            IImageRepository imageRepository,
            IFtpService ftpService)
        {
            _ftpHost = configuration["FtpOxServer:Host"] ?? throw new ArgumentNullException("FtpOxServer:Host");
            _ftpUser = configuration["FtpOxServer:User"] ?? throw new ArgumentNullException("FtpOxServer:User");
            _ftpPassword = configuration["FtpOxServer:Password"] ?? throw new ArgumentNullException("FtpOxServer:Password");
            _productService = productService;
            _imageService = imageService;
            _oxfordService = oxfordService;
            _logger = logger;
            _imageRepository = imageRepository;
            _ftpService = ftpService;
        }

        public async Task SyncImagesAsync()
        {
            var uriPath         = new Uri($"ftp://{_ftpHost}/imagens/");
            var request         = (FtpWebRequest)WebRequest.Create(uriPath);
            request.Method      = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential(_ftpUser, _ftpPassword);
            request.UsePassive  = true;
            request.UseBinary   = true;
            request.KeepAlive   = false;

            try
            {
                using var response = (FtpWebResponse)await request.GetResponseAsync();
                using var responseStream = response.GetResponseStream();
                using var reader = new StreamReader(responseStream);

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (!string.IsNullOrWhiteSpace(line) && line.StartsWith("P", StringComparison.OrdinalIgnoreCase))
                    {
                        var fullPath = $"{uriPath}{line}";
                        var nomeProduto = Path.GetFileNameWithoutExtension(line);
                        var extensao = Path.GetExtension(line);
                        _logger.LogInformation("*{nomeProduto}******** Arquivo lido: {Line} | Caminho completo: {FullPath}", nomeProduto, line, fullPath);

                        nomeProduto = nomeProduto.TrimStart('P', 'p');


                        // verifica se existe a imagem no banco
                        var imagensExists = await _imageRepository.GetByProductAsync(nomeProduto, Finalidade.PRODUTO);

                        if (imagensExists != null && imagensExists.Any())
                        {
                            // Já existem imagens para o produto e finalidade informados
                            //_logger.LogInformation("EXISTE****{nomeProduto}********", nomeProduto);
                        }
                        else
                        {
                            // Não existem imagens 
                            //_logger.LogInformation("NÃO EXISTE****{nomeProduto}********", nomeProduto);
                            var oxford = await _oxfordService.GetOxfordByProductIdAsync(nomeProduto);

                            if (oxford != null)
                            {
                                if (!string.IsNullOrWhiteSpace(oxford.FamilyDescription) &&
                                    !string.IsNullOrWhiteSpace(oxford.BrandDescription) &&
                                    !string.IsNullOrWhiteSpace(oxford.LineDescription) &&
                                    !string.IsNullOrWhiteSpace(oxford.DecorationDescription) &&
                                    !string.IsNullOrWhiteSpace(oxford.ProductId))
                                {
                                    var pathBuilder = new FtpImagePathBuilder(
                                        oxford.FamilyDescription.Replace(" ", "_"),
                                        oxford.BrandDescription.Replace(" ", "_"),
                                        oxford.LineDescription.Replace(" ", "_"),
                                        oxford.DecorationDescription.Replace(" ", "_"),
                                        oxford.ProductId,
                                        Finalidade.PRODUTO.ToString());

                                    var sourceUri = new Uri($"ftp://{_ftpHost}/imagens/{line}");

                                    // Baixa imagem do FTP antigo
                                    using var sourceStream = await DownloadFromFtpAsync(sourceUri);

                                    // Monta path no FTP novo
                                    var targetPathBuilder = new FtpImagePathBuilder(
                                        oxford.FamilyDescription.Replace(" ", "_"),
                                        oxford.BrandDescription.Replace(" ", "_"),
                                        oxford.LineDescription.Replace(" ", "_"),
                                        oxford.DecorationDescription.Replace(" ", "_"),
                                        oxford.ProductId,
                                        Finalidade.PRODUTO.ToString()
                                    );
                                    var targetPath = $"{targetPathBuilder.ToString()}/{nomeProduto}{extensao}";
                                    //var targetPath = Path.Combine(targetPathBuilder.ToString(), nomeProduto + extensao);

                                    // Garante diretório no FTP novo
                                    await _ftpService.EnsureDirectoryExistsAsync(targetPathBuilder);

                                    // Envia para o FTP novo
                                    await _ftpService.UploadAsync(targetPath, sourceStream);

                                    // Salva no banco
                                    var image = new Image
                                    {
                                        ProductId = nomeProduto,
                                        ImagePath = targetPath,
                                        Finalidade = Finalidade.PRODUTO.ToString(),
                                    };
                                    await _imageRepository.AddOrUpdateAsync(image);
                                    await _imageRepository.SaveAsync();
                                    //await _imageRepository.AddRangeAsync(new List<Image> { image });
                                    //await _imageRepository.SaveAsync();

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao listar arquivos em '{uriPath}': {ex.Message}", ex);
            }
        }

        public async Task RunFtpAsync()
        {
            var products = await _productService.GetAllProductsAsync();

            foreach (var product in products)
            {
                //var images = await _imageService.GetImagesByProductIdAsync(product.ProductId, Finalidade.TODOS);

                var oxford = await _oxfordService.GetOxfordByProductIdAsync(product.ProductId);

                if (oxford != null)
                {
                    if (!string.IsNullOrWhiteSpace(oxford.FamilyDescription) &&
                        !string.IsNullOrWhiteSpace(oxford.BrandDescription) &&
                        !string.IsNullOrWhiteSpace(oxford.LineDescription) &&
                        !string.IsNullOrWhiteSpace(oxford.DecorationDescription) &&
                        !string.IsNullOrWhiteSpace(oxford.ProductId))
                    {
                        var pathBuilder = new FtpImagePathBuilder(
                        oxford.FamilyDescription.Replace(" ", "_"),
                        oxford.BrandDescription.Replace(" ", "_"),
                        oxford.LineDescription.Replace(" ", "_"),
                        oxford.DecorationDescription.Replace(" ", "_"),
                        oxford.ProductId,
                        Finalidade.PRODUTO.ToString());

                        _logger.LogInformation("----> Caminho da imagem: {Path}", pathBuilder.BuildPath());

                        var imageName = $"P{product.ProductId}.jpg";
                        bool exists = await FtpFileExistsAsync(imageName);

                        if (exists)
                        {
                            _logger.LogInformation(">>>>>> Imagem encontrada no FTP: {ImageName}", imageName);

                            var sourceUri = new Uri($"ftp://{_ftpHost}/imagens/{imageName}");

                            // Baixa imagem do FTP antigo
                            using var sourceStream = await DownloadFromFtpAsync(sourceUri);

                            // Monta path no FTP novo
                            var targetPathBuilder = new FtpImagePathBuilder(
                                oxford.FamilyDescription.Replace(" ", "_"),
                                oxford.BrandDescription.Replace(" ", "_"),
                                oxford.LineDescription.Replace(" ", "_"),
                                oxford.DecorationDescription.Replace(" ", "_"),
                                oxford.ProductId,
                                Finalidade.PRODUTO.ToString()
                            );
                            var targetPath = $"{targetPathBuilder.ToString()}/{imageName}";

                            // Garante diretório no FTP novo
                            await _ftpService.EnsureDirectoryExistsAsync(targetPathBuilder);

                            // Envia para o FTP novo
                            await _ftpService.UploadAsync(targetPath, sourceStream);

                            // Salva no banco
                            var image = new Image
                            {
                                ProductId = product.ProductId,
                                ImagePath = targetPath,
                                Sequence = 1,
                                ImageMain = true,
                                Finalidade = Finalidade.PRODUTO.ToString(),
                            };

                            await _imageRepository.AddRangeAsync(new List<Image> { image });
                            await _imageRepository.SaveAsync();
                        }
                    }
                }
            }

            return;
        }

        private async Task<Stream> DownloadFromFtpAsync(Uri uri)
        {
            var request = (FtpWebRequest)WebRequest.Create(uri);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(_ftpUser, _ftpPassword);
            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = false;

            try
            {
                using var response = (FtpWebResponse)await request.GetResponseAsync();
                var responseStream = response.GetResponseStream();

                var memoryStream = new MemoryStream();
                await responseStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0; // Reset para leitura posterior
                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao baixar imagem: {uri}");
                throw;
            }
        }



        public async Task<bool> FtpFileExistsAsync(string imageName)
        {
            var uri = new Uri($"ftp://{_ftpHost}/imagens/{imageName}");
            var request = (FtpWebRequest)WebRequest.Create(uri);
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            request.Credentials = new NetworkCredential(_ftpUser, _ftpPassword);
            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = false;

            try
            {
                using var response = (FtpWebResponse)await request.GetResponseAsync();
                return true; // Arquivo existe
            }
            catch (WebException ex)
            {
                if (ex.Response is FtpWebResponse ftpResponse &&
                    ftpResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    return false; // Arquivo não existe
                }
                throw new Exception($"Erro ao verificar existência de '{imageName}': {ex.Message}", ex);
            }
        }
    }
}
