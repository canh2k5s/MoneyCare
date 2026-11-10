
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyCare.Models
{
    [Table("DanhMuc")]
    public class DanhMuc
    {
        [Key, Column("MaDanhMuc")]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required, StringLength(50), Column("TenDanhMuc")]
        public string Ten { get; set; } = "";

        [Required, StringLength(10)]
        public string Loai { get; set; } = "Chi";   // "Chi" hoặc "Thu"

        [StringLength(50)]
        public string? Icon { get; set; }

        [StringLength(10)]
        public string? Mau { get; set; }

        // Liên kết với người dùng
        public NguoiDung? NguoiDung { get; set; }
    }
}
