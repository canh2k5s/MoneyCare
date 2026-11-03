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
    }
}