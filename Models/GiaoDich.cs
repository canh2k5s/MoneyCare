using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyCare.Models
{
    // Anh xa den dbo.GiaoDich trong file SQLQuery1.sql
    [Table("GiaoDich")]
    public class GiaoDich
    {
        [Key]
        public int MaGiaoDich { get; set; }

        // Nguoi dung so huu giao dich; duoc lay tu Session khi luu
        public int UserId { get; set; }

        // Khoa ngoai den dbo.DanhMuc
        public int MaDanhMuc { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTien { get; set; }

        [Column(TypeName = "datetime2(0)")]
        public DateTime NgayGiaoDich { get; set; }

        [StringLength(500)]
        public string? GhiChu { get; set; }

        // Thuoc tinh lien ket (navigation properties)
        public NguoiDung? NguoiDung { get; set; }
        public DanhMuc? DanhMuc { get; set; }
    }
}
