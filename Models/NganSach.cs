using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyCare.Models
{
    [Table("NganSach")]
    public class NganSach
    {
        [Key]
        public int MaNganSach { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục chi tiêu.")]
        public int MaDanhMuc { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tháng.")]
        [Range(1, 12, ErrorMessage = "Tháng phải từ 1 đến 12.")]
        public byte Thang { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập năm.")]
        [Range(2000, 2100, ErrorMessage = "Năm không hợp lệ.")]
        public short Nam { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập hạn mức ngân sách.")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Hạn mức ngân sách phải lớn hơn 0.")]
        public decimal HanMuc { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual NguoiDung? NguoiDung { get; set; }

        [ForeignKey("MaDanhMuc")]
        public virtual DanhMuc? DanhMuc { get; set; }
    }
}