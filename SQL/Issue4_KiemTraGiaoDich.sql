-- Chọn database MoneyCare của nhóm trong SSMS trước khi chạy.
-- Chỉ kiểm tra. Không tạo lại database, không INSERT/UPDATE/DELETE dữ liệu.
SET NOCOUNT ON;

IF COL_LENGTH(N'dbo.NguoiDung', N'UserId') IS NULL
    THROW 54001, N'Không thấy dbo.NguoiDung.UserId. Hãy chọn đúng database MoneyCare.', 1;

IF COL_LENGTH(N'dbo.DanhMuc', N'MaDanhMuc') IS NULL OR
   COL_LENGTH(N'dbo.DanhMuc', N'TenDanhMuc') IS NULL OR
   COL_LENGTH(N'dbo.DanhMuc', N'UserId') IS NULL OR
   COL_LENGTH(N'dbo.DanhMuc', N'Loai') IS NULL OR
   COL_LENGTH(N'dbo.DanhMuc', N'Icon') IS NULL OR
   COL_LENGTH(N'dbo.DanhMuc', N'Mau') IS NULL
    THROW 54002, N'Bảng dbo.DanhMuc không khớp với schema của nhóm.', 1;

IF COL_LENGTH(N'dbo.GiaoDich', N'MaGiaoDich') IS NULL OR
   COL_LENGTH(N'dbo.GiaoDich', N'UserId') IS NULL OR
   COL_LENGTH(N'dbo.GiaoDich', N'MaDanhMuc') IS NULL OR
   COL_LENGTH(N'dbo.GiaoDich', N'SoTien') IS NULL OR
   COL_LENGTH(N'dbo.GiaoDich', N'NgayGiaoDich') IS NULL OR
   COL_LENGTH(N'dbo.GiaoDich', N'GhiChu') IS NULL
    THROW 54003, N'Bảng dbo.GiaoDich không khớp với schema của nhóm.', 1;

SELECT DB_NAME() AS DatabaseName, N'Đã thấy đủ cột cho Issue 4.' AS KetQua;

-- Nếu có dòng ở đây, trao đổi với nhóm trước khi chỉnh dữ liệu.
SELECT TOP (20) g.MaGiaoDich, g.UserId, g.MaDanhMuc,
       N'Không tìm thấy chủ tài khoản hoặc danh mục của đúng chủ tài khoản.' AS VanDe
FROM dbo.GiaoDich AS g
LEFT JOIN dbo.NguoiDung AS u ON u.UserId = g.UserId
LEFT JOIN dbo.DanhMuc AS d ON d.MaDanhMuc = g.MaDanhMuc AND d.UserId = g.UserId
WHERE u.UserId IS NULL OR d.MaDanhMuc IS NULL
ORDER BY g.MaGiaoDich;
