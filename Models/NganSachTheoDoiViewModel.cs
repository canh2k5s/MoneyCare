using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyCare.Models
{
    [Keyless]
    public class NganSachTheoDoiViewModel
    {
        public int MaNganSach { get; set; }
        public int UserId { get; set; }
        public byte Thang { get; set; }
        public short Nam { get; set; }
        public int MaDanhMuc { get; set; }
        public string TenDanhMuc { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal HanMuc { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DaChi { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ConLai { get; set; }

        public decimal PhanTramDaDung { get; set; }
        public string CanhBao { get; set; } = string.Empty; // 'BinhThuong', 'SapVuot', 'VuotHanMuc'
    }
}