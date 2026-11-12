using Microsoft.EntityFrameworkCore;

namespace MoneyCare.Models
{
    public class MoneyCareContext : DbContext
    {
        public MoneyCareContext(DbContextOptions<MoneyCareContext> options) : base(options)
        {
        }

        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<GiaoDich> GiaoDichs { get; set; }
        public DbSet<MucTieuTietKiem> MucTieuTietKiems { get; set; }
        public DbSet<LichSuTietKiem> LichSuTietKiems { get; set; }
        public DbSet<NganSach> NganSachs { get; set; }
        
        // DbSet cho View theo dõi ngân sách
        public DbSet<NganSachTheoDoiViewModel> VwTheoDoiNganSachs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình Keyless entity cho View
            modelBuilder.Entity<NganSachTheoDoiViewModel>()
                .ToView("vw_TheoDoiNganSach")
                .HasNoKey();

            // Ràng buộc Unique Index tương ứng CSDL
            modelBuilder.Entity<NganSach>()
                .HasIndex(n => new { n.UserId, n.MaDanhMuc, n.Thang, n.Nam })
                .IsUnique();
        }
    }
}