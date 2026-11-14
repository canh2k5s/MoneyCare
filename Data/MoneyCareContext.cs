
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

        // Các bảng dữ liệu
        public DbSet<NguoiDung> NguoiDung { get; set; }

        public DbSet<DanhMuc> DanhMuc { get; set; }

        public DbSet<GiaoDich> GiaoDich { get; set; }

        public DbSet<MucTieuTietKiem> MucTieuTietKiem { get; set; }

        public DbSet<LichSuTietKiem> LichSuTietKiem { get; set; }

        public DbSet<NganSach> NganSach { get; set; }

        // View theo dõi ngân sách
        public DbSet<NganSachTheoDoiViewModel> VwTheoDoiNganSachs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình bảng DanhMuc
            modelBuilder.Entity<DanhMuc>()
                .ToTable("DanhMuc", table =>
                    table.UseSqlOutputClause(false));

            // Cấu hình View theo dõi ngân sách
            modelBuilder.Entity<NganSachTheoDoiViewModel>()
                .ToView("vw_TheoDoiNganSachs")
                .HasNoKey();

            // Mỗi người dùng chỉ có một ngân sách
            // cho một danh mục trong một tháng và một năm
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
