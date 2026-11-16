using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MoneyCare.Models
{
    // Chỉ nhận thông tin giao dịch từ form, không nhận UserId từ trình duyệt.
    public class ThemGiaoDichInput
    {
        [Required(ErrorMessage = "Vui lòng nhập số tiền.")]
        public string? SoTien { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại giao dịch.")]
        [RegularExpression("^(Thu|Chi)$", ErrorMessage = "Loại giao dịch chỉ được là Thu hoặc Chi.")]
        public string? Loai { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục.")]
        public int MaDanhMuc { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày giao dịch.")]
        [DataType(DataType.Date)]
        public DateTime? NgayGiaoDich { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string? GhiChu { get; set; }
    }

    // Bộ lọc lịch sử; mỗi trường là tùy chọn, có thể phối hợp nhiều điều kiện.
    public class GiaoDichBoLoc
    {
        public string? TuKhoa { get; set; }
        public string? Loai { get; set; }
        public int? MaDanhMuc { get; set; }
        [DataType(DataType.Date)] public DateTime? TuNgay { get; set; }
        [DataType(DataType.Date)] public DateTime? DenNgay { get; set; }
        [Range(1, 12, ErrorMessage = "Tháng phải từ 1 đến 12.")]
        public int? Thang { get; set; }
        [Range(1, 9999, ErrorMessage = "Năm phải từ 1 đến 9999.")]
        public int? Nam { get; set; }
    }

    public class GiaoDichPageViewModel
    {
        public ThemGiaoDichInput Form { get; set; } = new();
        public GiaoDichBoLoc BoLoc { get; set; } = new();
        public List<DanhMuc> DanhMuc { get; set; } = new();
        public List<GiaoDich> DanhSach { get; set; } = new();

        // Phân trang: không tải toàn bộ lịch sử vào một trang.
        public int Trang { get; set; } = 1;
        public int SoTrang { get; set; } = 1;
        public int TongBanGhi { get; set; }
        public decimal TongThu { get; set; }
        public decimal TongChi { get; set; }
    }

    public class SuaGiaoDichViewModel
    {
        public int MaGiaoDich { get; set; }
        public ThemGiaoDichInput Form { get; set; } = new();
        public List<DanhMuc> DanhMuc { get; set; } = new();
    }
}
