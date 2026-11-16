using System.Globalization;

namespace MoneyCare.Models;

public static class QuyTacTietKiem
{
    public const string DangThucHien = "DangThucHien";
    public const string HoanThanh = "HoanThanh";
    public const string Huy = "Huy";
    public const decimal SoTienToiDa = 9999999999999999.99m;

    public static bool DaHuy(string? trangThai) => trangThai is Huy or "DaHuy";

    public static string TrangThaiTheoSoDu(decimal hienTai, decimal canDat) =>
        canDat > 0 && hienTai >= canDat ? HoanThanh : DangThucHien;

    // Form HTML dùng dấu chấm cho phần thập phân; đọc độc lập với locale máy chủ.
    public static bool ThuDocSoTien(string? text, out decimal soTien)
    {
        if (!decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out soTien))
            return false;
        return soTien > 0 && soTien <= SoTienToiDa && decimal.Round(soTien, 2) == soTien;
    }

    public static decimal TinhPhanTram(decimal hienTai, decimal canDat)
    {
        if (canDat <= 0 || hienTai <= 0) return 0;
        if (hienTai >= canDat) return 100;
        // Không hiển thị 100% khi người dùng còn thiếu tiền.
        return Math.Floor(hienTai / canDat * 10000m) / 100m;
    }
}

public sealed class TietKiemTienDoViewModel
{
    public decimal SoDuKhaDung { get; init; }
    public IReadOnlyList<MucTieuTietKiem> MucTieu { get; init; } = [];
    public MucTieuTietKiem? MucTieuLichSu { get; init; }
    public IReadOnlyList<LichSuTietKiem> LichSu { get; init; } = [];
    public int Trang { get; init; } = 1;
    public int TongTrang { get; init; } = 1;
}

public sealed class TaoMucTieuInput
{
    public string? TenMucTieu { get; set; }
    public string? SoTienMucTieu { get; set; }
}

public sealed class NapRutTietKiemInput
{
    public int MaMucTieu { get; set; }
    public string? Loai { get; set; }
    public string? SoTien { get; set; }
    public string? GhiChu { get; set; }
}
