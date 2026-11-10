using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MoneyCare.Models
{
    // Du lieu form, khong chua UserId de tranh gan giao dich cho tai khoan khac
    public class ThemGiaoDichInput
    {
        [Range(typeof(decimal), "0.01", "9999999999999999.99",
            ParseLimitsInInvariantCulture = true,
            ConvertValueInInvariantCulture = true,
            ErrorMessage = "Số tiền phải lớn hơn 0.")]
        public decimal SoTien { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại giao dịch.")]
        [RegularExpression("^(Thu|Chi)$", ErrorMessage = "Chỉ chấp nhận Thu hoặc Chi.")]
        public string Loai { get; set; } = "Chi";

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục.")]
        public int MaDanhMuc { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày giao dịch.")]
        [DataType(DataType.Date)]
        public DateTime? NgayGiaoDich { get; set; } = DateTime.Today;

        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự.")]
        public string? GhiChu { get; set; }
    }

    public class GiaoDichPageViewModel
    {
        public ThemGiaoDichInput Form { get; set; } = new();
        public List<DanhMuc> DanhMuc { get; set; } = new();
        public List<GiaoDich> DanhSach { get; set; } = new();
    }
}
