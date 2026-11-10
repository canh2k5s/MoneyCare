using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyCare.Models
{
    public class NganSach
    {
        [Key]
        public int MaNganSach { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public NguoiDung? User { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int MaDanhMuc { get; set; }

        [ForeignKey("MaDanhMuc")]
        public DanhMuc? DanhMuc { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tháng")]
        [Range(1, 12, ErrorMessage = "Tháng phải từ 1 đến 12")]
        public int Thang { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập năm")]
        public int Nam { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập hạn mức ngân sách")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Hạn mức phải lớn hơn 0")]
        public decimal HanMuc { get; set; }
    }
}