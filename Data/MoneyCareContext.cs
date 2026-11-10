
using Microsoft.EntityFrameworkCore;
using MoneyCare.Models;

namespace MoneyCare.Data
{
    public class MoneyCareContext : DbContext
    {
        public MoneyCareContext(
            DbContextOptions<MoneyCareContext> options)
            : base(options)
        {
        }

        // Bảng người dùng
        public DbSet<NguoiDung> NguoiDung { get; set; } = null!;

        // Bảng giao dịch
        public DbSet<GiaoDich> GiaoDich { get; set; } = null!;

        // Bảng danh mục
        public DbSet<DanhMuc> DanhMuc { get; set; } = null!;

        // Bảng ngân sách
        public DbSet<NganSach> NganSach { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình bảng DanhMuc
            modelBuilder.Entity<DanhMuc>()
                .ToTable("DanhMuc", table =>
                    table.UseSqlOutputClause(false));

            // Khóa kết hợp để liên kết danh mục và người dùng
            modelBuilder.Entity<DanhMuc>()
                .HasAlternateKey(d => new
                {
                    d.Id,
                    d.UserId
                });

            // Liên kết danh mục với tài khoản
            modelBuilder.Entity<DanhMuc>()
                .HasOne(d => d.NguoiDung)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Liên kết giao dịch với tài khoản
            modelBuilder.Entity<GiaoDich>()
                .HasOne(g => g.NguoiDung)
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Giao dịch chỉ dùng danh mục của đúng tài khoản
            modelBuilder.Entity<GiaoDich>()
                .HasOne(g => g.DanhMuc)
                .WithMany()
                .HasForeignKey(g => new
                {
                    g.MaDanhMuc,
                    g.UserId
                })
                .HasPrincipalKey(d => new
                {
                    d.Id,
                    d.UserId
                })
                .OnDelete(DeleteBehavior.NoAction);

            // Bỏ liên kết User vì NganSach.UserId là string,
            // trong khi NguoiDung.UserId là int
            modelBuilder.Entity<NganSach>()
                .Ignore(n => n.User);
        }
    }
}
