using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyCare.Data;
using MoneyCare.Models;

namespace MoneyCare.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class TietKiemController(MoneyCareContext db) : Controller
{
    private const int KichThuocTrang = 15;
    private int? UserId => HttpContext.Session.GetInt32("UserId");

    [HttpGet]
    public async Task<IActionResult> Index(int? id, int trang = 1, CancellationToken ct = default)
    {
        if (UserId is not int userId)
            return RedirectToAction("Login", "Account");

        var mucTieu = await db.MucTieuTietKiem.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.MaMucTieu)
            .ToListAsync(ct);

        var dangXem = id.HasValue ? mucTieu.Find(x => x.MaMucTieu == id.Value) : null;
        if (id.HasValue && dangXem is null) return NotFound();

        List<LichSuTietKiem> lichSu = [];
        int tongTrang = 1;
        if (dangXem is not null)
        {
            var query = db.LichSuTietKiem.AsNoTracking().Where(x => x.MaMucTieu == dangXem.MaMucTieu);
            int soDong = await query.CountAsync(ct);
            tongTrang = Math.Max(1, (int)(((long)soDong + KichThuocTrang - 1) / KichThuocTrang));
            trang = Math.Clamp(trang, 1, tongTrang);
            lichSu = await query.OrderByDescending(x => x.NgayGiaoDich).ThenByDescending(x => x.MaLichSu)
                .Skip((trang - 1) * KichThuocTrang).Take(KichThuocTrang).ToListAsync(ct);
        }
        else
        {
            trang = 1;
        }

        return View("~/Views/Home/TietKiem.cshtml", new TietKiemTienDoViewModel
        {
            SoDuKhaDung = await TinhSoDuKhaDung(userId, mucTieu.Sum(x => x.SoTienHienTai), ct),
            MucTieu = mucTieu,
            MucTieuLichSu = dangXem,
            LichSu = lichSu,
            Trang = trang,
            TongTrang = tongTrang
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TaoMucTieu([FromForm] TaoMucTieuInput form, CancellationToken ct)
    {
        if (UserId is not int userId)
            return RedirectToAction("Login", "Account");

        string? ten = form.TenMucTieu?.Trim();
        if (string.IsNullOrWhiteSpace(ten) || ten.Length > 150)
            return Loi("Tên mục tiêu phải từ 1 đến 150 ký tự.");

        if (!QuyTacTietKiem.ThuDocSoTien(form.SoTienMucTieu, out decimal canDat))
            return Loi("Số tiền mục tiêu phải lớn hơn 0, tối đa hai chữ số thập phân và trong giới hạn cho phép.");

        var mucTieu = new MucTieuTietKiem
        {
            UserId = userId,
            TenMucTieu = ten,
            SoTienMucTieu = canDat,
            SoTienHienTai = 0,
            TrangThai = QuyTacTietKiem.DangThucHien
        };
        db.MucTieuTietKiem.Add(mucTieu);
        await db.SaveChangesAsync(ct);
        TempData["TietKiemSuccess"] = "Đã tạo mục tiêu tiết kiệm.";
        return RedirectToAction(nameof(Index), null, new { id = mucTieu.MaMucTieu }, "muc-tieu-" + mucTieu.MaMucTieu);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> NapRut([FromForm] NapRutTietKiemInput form, CancellationToken ct)
    {
        if (UserId is not int userId)
            return RedirectToAction("Login", "Account");

        if (form.Loai is not ("Nap" or "Rut"))
            return Loi("Vui lòng chọn Nạp hoặc Rút.", form.MaMucTieu);
        if (!QuyTacTietKiem.ThuDocSoTien(form.SoTien, out decimal soTien))
            return Loi("Số tiền phải lớn hơn 0, tối đa hai chữ số thập phân và trong giới hạn cho phép.", form.MaMucTieu);

        string? ghiChu = form.GhiChu?.Trim();
        if (ghiChu?.Length > 300)
            return Loi("Ghi chú không được dài quá 300 ký tự.", form.MaMucTieu);

        try
        {
            // Kiểm tra ví và ghi trong cùng giao dịch: không dùng cùng số dư để nạp vào hai mục tiêu.
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var mucTieu = await db.MucTieuTietKiem
                .SingleOrDefaultAsync(x => x.MaMucTieu == form.MaMucTieu && x.UserId == userId, ct);
            if (mucTieu is null) return NotFound();

            if (QuyTacTietKiem.DaHuy(mucTieu.TrangThai))
                return Loi("Mục tiêu đã hủy, không thể nạp hoặc rút tiền.", mucTieu.MaMucTieu);
            if (form.Loai == "Rut" && soTien > mucTieu.SoTienHienTai)
                return Loi("Không thể rút nhiều hơn số tiền hiện có trong mục tiêu.", mucTieu.MaMucTieu);

            if (form.Loai == "Nap")
            {
                decimal tongDaGiu = await db.MucTieuTietKiem.Where(x => x.UserId == userId)
                    .SumAsync(x => x.SoTienHienTai, ct);
                decimal soDuKhaDung = await TinhSoDuKhaDung(userId, tongDaGiu, ct);
                if (soTien > soDuKhaDung)
                    return Loi("Số dư Thu/Chi khả dụng không đủ để nạp vào mục tiêu.", mucTieu.MaMucTieu);
            }

            // Đây là chuyển tiền nội bộ, không ghi vào GiaoDich và không làm tăng số Chi của ngân sách.
            decimal bienDong = form.Loai == "Nap" ? soTien : -soTien;
            decimal soDuMoi = mucTieu.SoTienHienTai + bienDong;
            if (soDuMoi > QuyTacTietKiem.SoTienToiDa)
                return Loi("Số tiền trong mục tiêu đã vượt giới hạn lưu trữ.", mucTieu.MaMucTieu);

            mucTieu.SoTienHienTai = soDuMoi;
            mucTieu.TrangThai = QuyTacTietKiem.TrangThaiTheoSoDu(soDuMoi, mucTieu.SoTienMucTieu);
            db.LichSuTietKiem.Add(new LichSuTietKiem
            {
                MaMucTieu = mucTieu.MaMucTieu,
                // Schema nhóm không có cột Loai: nạp lưu dương, rút lưu âm.
                SoTien = bienDong,
                NgayGiaoDich = DateTime.Now,
                GhiChu = string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu
            });

            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Loi("Số dư vừa thay đổi. Vui lòng kiểm tra lại trước khi nạp hoặc rút.", form.MaMucTieu);
        }
        catch (SqlException ex) when (ex.Number == 1205)
        {
            return Loi("Có thao tác khác đang cập nhật dữ liệu. Vui lòng kiểm tra số dư và thử lại.", form.MaMucTieu);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 1205 })
        {
            return Loi("Có thao tác khác đang cập nhật dữ liệu. Vui lòng kiểm tra số dư và thử lại.", form.MaMucTieu);
        }

        TempData["TietKiemSuccess"] = form.Loai == "Nap" ? "Đã nạp tiền vào mục tiêu." : "Đã rút tiền khỏi mục tiêu.";
        return RedirectToAction(nameof(Index), null, new { id = form.MaMucTieu }, "lich-su");
    }

    private async Task<decimal> TinhSoDuKhaDung(int userId, decimal tongDaGiu, CancellationToken ct)
    {
        var bienDongVi =
            from g in db.GiaoDich.AsNoTracking()
            join d in db.DanhMuc.AsNoTracking() on g.MaDanhMuc equals d.Id
            where g.UserId == userId && d.UserId == userId && (d.Loai == "Thu" || d.Loai == "Chi")
            select d.Loai == "Thu" ? g.SoTien : -g.SoTien;

        // Dùng giao dịch hiện có làm nguồn số liệu, không tạo bảng ví hay sao chép giao dịch.
        return await bienDongVi.SumAsync(ct) - tongDaGiu;
    }

    private IActionResult Loi(string message, int? id = null)
    {
        TempData["TietKiemError"] = message;
        return RedirectToAction(nameof(Index), new { id });
    }
}
