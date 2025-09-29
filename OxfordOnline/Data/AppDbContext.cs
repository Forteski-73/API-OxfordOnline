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

        public DbSet<PalletItem> PalletItem { get; set; }

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

            // Configura PK composta para PalletItem
            modelBuilder.Entity<PalletItem>()
                .HasKey(pi => new { pi.PalletId, pi.ProductId });
        }
    }
}
