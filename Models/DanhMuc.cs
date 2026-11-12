
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyCare.Models
{
    public class NganSach
    {
        [Key]
        public int MaNganSach { get; set; }

        public int UserId { get; set; }

        public NguoiDung? User { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int MaDanhMuc { get; set; }

        public DanhMuc? DanhMuc { get; set; }

        [Range(1, 12, ErrorMessage = "Tháng phải từ 1 đến 12")]
        public byte Thang { get; set; }

        [Range(2000, 2100, ErrorMessage = "Năm không hợp lệ")]
        public short Nam { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0.01", "9999999999999999",
            ErrorMessage = "Hạn mức phải lớn hơn 0")]
        public decimal HanMuc { get; set; }
    }
}
