
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<DanhMuc>()
        .ToTable("DanhMuc", table =>
            table.UseSqlOutputClause(false));
}
    }
}

