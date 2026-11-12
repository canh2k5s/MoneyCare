
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyCare.Models
{
    [Table("TietKiem")]
    public class TietKiem
    {
        [Key]
        public int MaTietKiem { get; set; }

        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên mục tiêu")]
        [StringLength(150)]
        public string TenMucTieu { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTienMucTieu { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTienHienTai { get; set; }

        [Column(TypeName = "date")]
        public DateTime HanHoanThanh { get; set; }

        public DateTime NgayTao { get; set; }

        [ForeignKey(nameof(UserId))]
        public NguoiDung? NguoiDung { get; set; }
    }
}
