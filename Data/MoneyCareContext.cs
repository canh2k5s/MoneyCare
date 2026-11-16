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

            // Liên kết giao dịch với danh mục của cùng chủ tài khoản.
            modelBuilder.Entity<GiaoDich>()
                .HasOne(g => g.DanhMuc)
                .WithMany()
                .HasForeignKey(g => new { g.MaDanhMuc, g.UserId })
                .HasPrincipalKey(d => new { d.Id, d.UserId })
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GiaoDich>()
                .HasOne(g => g.NguoiDung)
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Restrict);

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
