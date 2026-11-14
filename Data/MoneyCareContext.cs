using Microsoft.EntityFrameworkCore;
using MoneyCare.Models;

namespace MoneyCare.Data
{
    public class MoneyCareContext : DbContext
    {
        public MoneyCareContext(DbContextOptions<MoneyCareContext> options)
            : base(options)
        {
        }

        public DbSet<NguoiDung> NguoiDung { get; set; }
        public DbSet<DanhMuc> DanhMuc { get; set; }
        public DbSet<GiaoDich> GiaoDich { get; set; }
        public DbSet<MucTieuTietKiem> MucTieuTietKiem { get; set; }
        public DbSet<LichSuTietKiem> LichSuTietKiem { get; set; }
        public DbSet<NganSach> NganSach { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DanhMuc>()
                .ToTable("DanhMuc", table =>
                    table.UseSqlOutputClause(false));

            modelBuilder.Entity<NganSach>()
                .HasIndex(n => new
                {
                    n.UserId,
                    n.MaDanhMuc,
                    n.Thang,
                    n.Nam
                })
                .IsUnique();
        }
    }
}
