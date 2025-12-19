using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OxfordOnline.Models;
using OxfordOnline.Models.Dto;

namespace OxfordOnline.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IConfiguration _configuration;
        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        public DbSet<ApiUser> ApiUser { get; set; }

        public DbSet<UserAccount> UserAccount { get; set; }

        public DbSet<Product> Product { get; set; }

        public DbSet<Image> Image { get; set; }

        public DbSet<Invent> Invent { get; set; }

        public DbSet<InventDim> InventDim { get; set; }

        public DbSet<Oxford> Oxford { get; set; }

        public DbSet<TaxInformation> TaxInformation { get; set; }

        public DbSet<Tag> Tag { get; set; }

        public DbSet<ProductBrand> ProductBrand { get; set; }

        public DbSet<ProductLine> ProductLine { get; set; }

        public DbSet<ProductFamily> ProductFamily { get; set; }

        public DbSet<ProductDecoration> ProductDecoration { get; set; }

        public DbSet<ProductAttributeMap> ProductAttributeMap { get; set; }

        public DbSet<Pallet> Pallet { get; set; }

        public DbSet<PalletImage> PalletImage { get; set; }

        public DbSet<PalletItem> PalletItem { get; set; }

        public DbSet<PalletLoadHead> PalletLoadHead { get; set; }

        public DbSet<Models.PalletLoadLine> PalletLoadLine { get; set; }

        public DbSet<PalletInvoice> PalletInvoice { get; set; } = null!;

        public DbSet<Models.InventoryGuid> InventoryGuid { get; set; }

        public DbSet<Models.Inventory> Inventory { get; set; }

        public DbSet<Models.InventoryRecord> InventoryRecord { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                optionsBuilder.UseMySql(connectionString,
                    ServerVersion.AutoDetect(connectionString)); // Detecta a versão do banco automaticamente
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Define a Chave Primária Composta (PalletId + ProductId)
            modelBuilder.Entity<PalletItem>()
                .HasKey(pi => new { pi.PalletId, pi.ProductId });

            // 2. Define Explicitamente a Relação de Chave Estrangeira com Product
            // Isso garante que o EF Core saiba como buscar o Product.
            modelBuilder.Entity<PalletItem>()
                .HasOne(pi => pi.Product) // A propriedade de navegação PalletItem.Product
                .WithMany()
                .HasForeignKey(pi => pi.ProductId); // Usando PalletItem.ProductId como FK

            // Chave primária composta para PalletLoadLine
            modelBuilder.Entity<Models.PalletLoadLine>()
                .HasKey(pll => new { pll.LoadId, pll.PalletId });

            modelBuilder.Entity<Models.PalletLoadLine>()
                .HasOne(pll => pll.Pallet)
                .WithMany()
                .HasForeignKey(pll => pll.PalletId);
        }
    }
}
