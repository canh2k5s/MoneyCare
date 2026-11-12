using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyCare.Models
{
    [Table("LichSuTietKiem")]
    public class LichSuTietKiem
    {
        [Key]
        public int MaLichSu { get; set; }

        [Required]
        public int MaMucTieu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tiền thêm/rút.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTien { get; set; }

        [Column(TypeName = "datetime2(0)")]
        public DateTime NgayGiaoDich { get; set; } = DateTime.Now;

        [StringLength(300)]
        public string? GhiChu { get; set; }

        // Navigation property
        [ForeignKey("MaMucTieu")]
        public virtual MucTieuTietKiem? MucTieuTietKiem { get; set; }
    }
}