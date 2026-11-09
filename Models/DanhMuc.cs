using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyCare.Models
{
    // Danh muc thu/chi thuoc mot tai khoan cu the
    [Table("DanhMuc")]
    public class DanhMuc
    {
        [Key]
        public int MaDanhMuc { get; set; }

        public int UserId { get; set; }

        [Required, StringLength(100)]
        public string TenDanhMuc { get; set; } = string.Empty;

        [Required, StringLength(3)]
        public string Loai { get; set; } = string.Empty; // "Thu" hoac "Chi"

        public NguoiDung? NguoiDung { get; set; }
    }
}
