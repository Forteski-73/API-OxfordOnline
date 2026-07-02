using Microsoft.EntityFrameworkCore;
using OxfordOnline.Models;
using OxfordOnline.Models.Dto;
using OxfordOnline.Models.Enums;
using OxfordOnline.Repositories.Interfaces;
using static System.Net.Mime.MediaTypeNames;

namespace OxfordOnline.Services
{
    public class ProductService
    {
        private readonly IProductRepository         _repo;
        private readonly IOxfordRepository          _oxfordRepo;
        private readonly IInventRepository          _inventRepo;
        private readonly IInventDimRepository       _inventDimRepo;
        private readonly ITaxInformationRepository  _taxRepo;
        private readonly IImageRepository           _imageRepo;

        public ProductService(
            IProductRepository          repo,
            IOxfordRepository           oxfordRepo,
            IInventRepository           inventRepo,
            IInventDimRepository        inventDimRepo,
            ITaxInformationRepository   taxRepo,
            IImageRepository            imageRepo)
        {
            _repo           = repo;
            _oxfordRepo     = oxfordRepo;
            _inventRepo     = inventRepo;
            _inventDimRepo  = inventDimRepo;
            _taxRepo        = taxRepo;
            _imageRepo      = imageRepo;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync() =>
            await _repo.GetAllAsync();
        public async Task<IEnumerable<Product>> GetSearchAsyncAsync(
            string? product, string? barcode, string? family, string? brand, string? line, string? decoration, string? nome) =>
            await _repo.GetSearchAsync( product, barcode, family, brand, line, decoration, nome);

        public async Task<IEnumerable<ProductComplete>> GetAppProductAsync(string? product) =>
            await _repo.GetAppProductAsync(product);
        
        public async Task<ProductSearchResponse> GetAppSearchAsyncAsync(AppProductFilterRequest filterRequest) =>
            await _repo.GetAppSearchAsync(filterRequest);

        public async Task<Product?> GetProductByIdAsync(String productId) =>
            await _repo.GetByProductIdAsync(productId);

        public async Task<bool> CreateOrUpdateProductsAsync(List<Product> products)
        {
            foreach (var p in products)
            {
                var existProduct = await _repo.GetByProductIdAsync(p.ProductId);
                if (existProduct != null)
                {
                    existProduct.ProductName = p.ProductName;
                    existProduct.Barcode = p.Barcode;
                    await _repo.UpdateAsync(existProduct);
                }
                else
                {
                    await _repo.AddAsync(p);
                }
            }

            await _repo.SaveAsync();
            return true;
        }
        public async Task<bool> UpdateProductAsync(Product product)
        {
            await _repo.UpdateAsync(product);
            await _repo.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteProductAsync(String productId)
        {
            var product = await _repo.GetByProductIdAsync(productId);
            if (product == null) return false;

            await _repo.DeleteAsync(product);
            await _repo.SaveAsync();
            return true;
        }

        public async Task<ProductData?> GetProductDataAsync(string productId)
        {
            var product = await _repo.GetByProductIdAsync(productId);
            if (product == null) return null;

            var oxford = await _oxfordRepo.GetByProductIdAsync(productId);
            var invent = await _inventRepo.GetByProductIdAsync(productId);
            var taxInfo = await _taxRepo.GetByProductIdAsync(productId);
            //var images = await _imageRepo.GetByProductIdAsync(productId, Finalidade.TODOS);

            return new ProductData
            {
                Product = product,
                Oxford = oxford,
                Invent = invent,
                TaxInformation = taxInfo,
            };
        }
        public async Task<ProductData?> GetProductDataRangeAsync(string Id)
        {
            var product = await _repo.GetByProductIdAsync(Id);
            if (product == null) return null;

            var oxford      = await _oxfordRepo.GetByProductIdAsync(product.ProductId);
            var invent      = await _inventRepo.GetByProductIdAsync(product.ProductId);
            var taxInfo     = await _taxRepo.GetByProductIdAsync(product.ProductId);
            //var images    = await _imageRepo.GetByProductIdAsync(productId, Finalidade.TODOS);

            return new ProductData
            {
                Product         = product,
                Oxford          = oxford,
                Invent          = invent,
                TaxInformation  = taxInfo,
            };
        }

        public async Task<Product?> GetFirstAsync()
        {
            var prod = await _repo.GetFirstAsync();
            return prod;
        }

        public async Task<List<ProductDetails>> GetProductDetailsAsync(bool status, string locationId)
        {
            var products = await _repo.GetByStatusAndLocationAsync(status, locationId);

            if (products == null || !products.Any())
                return new List<ProductDetails>();

            var detailsList = new List<ProductDetails>();

            foreach (var product in products)
            {
                var productId = product.ProductId;

                var oxford      = await _oxfordRepo.    GetByProductIdAsync(productId);
                var invent      = await _inventRepo.    GetByProductIdAsync(productId);
                var taxInfo     = await _taxRepo.       GetByProductIdAsync(productId);
                var inventDim   = await _inventDimRepo. GetInventDimByProductIdAsync(productId);
                var images      = await _imageRepo.     GetByProductAsync(productId, Finalidade.TODOS);

                var details = new ProductDetails
                {
                    Product         = product,
                    Oxford          = oxford,
                    TaxInformation  = taxInfo,
                    Invent          = invent,
                    Location        = inventDim,
                    Images          = images,
                };

                detailsList.Add(details);
            }

            return detailsList;
        }

        public async Task<List<ProductOxford>> GetProductOxfordAsync(ProductOxfordFilters filters)
        {
            return await _repo.GetProductOxfordAsync(filters);
        }

        public async Task<List<ProductOxfordDetails>> GetFilteredOxfordProductDetailsAsync(List<string> products)
        {
            return await _repo.GetFilteredOxfordProductDetailsAsync(products);
        }
    }
}
