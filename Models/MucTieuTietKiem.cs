using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyCare.Models
{
    [Table("MucTieuTietKiem")]
    public class MucTieuTietKiem
    {
        [Key]
        public int MaMucTieu { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên mục tiêu.")]
        [StringLength(150)]
        public string TenMucTieu { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số tiền mục tiêu.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTienMucTieu { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTienHienTai { get; set; } = 0;

        [DataType(DataType.Date)]
        public DateTime? NgayKetThuc { get; set; }

        [StringLength(20)]
        public string TrangThai { get; set; } = "DangThucHien"; // DangThucHien, HoanThanh, Huy

        // Navigation property
        [ForeignKey("UserId")]
        public virtual NguoiDung? NguoiDung { get; set; }

        public virtual ICollection<LichSuTietKiem>? LichSuTietKiems { get; set; }
    }
}