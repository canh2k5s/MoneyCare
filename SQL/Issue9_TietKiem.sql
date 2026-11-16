-- Chon dung co so du lieu MoneyCare cua nhom trong SSMS truoc khi chay.
-- Chi tao hai bang neu chua co. Khong xoa, ghi de hay tu sua du lieu cu.
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.NguoiDung', N'U') IS NULL OR COL_LENGTH(N'dbo.NguoiDung', N'UserId') IS NULL
    THROW 52090, N'Hãy chọn database MoneyCare đã có bảng dbo.NguoiDung (UserId).', 1;

BEGIN TRY
    BEGIN TRANSACTION;
    IF OBJECT_ID(N'dbo.MucTieuTietKiem', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.MucTieuTietKiem
        (
            MaMucTieu int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MucTieuTietKiem PRIMARY KEY,
            UserId int NOT NULL,
            TenMucTieu nvarchar(150) NOT NULL,
            SoTienMucTieu decimal(18,2) NOT NULL,
            SoTienHienTai decimal(18,2) NOT NULL CONSTRAINT DF_Issue9_SoTienHienTai DEFAULT (0),
            NgayKetThuc date NULL,
            TrangThai nvarchar(20) NOT NULL CONSTRAINT DF_Issue9_TrangThai DEFAULT (N'DangThucHien'),
            CONSTRAINT FK_Issue9_MucTieu_NguoiDung FOREIGN KEY (UserId) REFERENCES dbo.NguoiDung(UserId),
            CONSTRAINT CK_Issue9_MucTieu_SoTien CHECK (SoTienMucTieu > 0 AND SoTienHienTai >= 0),
            CONSTRAINT CK_Issue9_MucTieu_TrangThai CHECK (TrangThai IN (N'DangThucHien', N'HoanThanh', N'Huy', N'DaHuy'))
        );
        CREATE INDEX IX_Issue9_MucTieu_User ON dbo.MucTieuTietKiem(UserId, MaMucTieu DESC);
    END;

    IF COL_LENGTH(N'dbo.MucTieuTietKiem', N'MaMucTieu') IS NULL OR
       COL_LENGTH(N'dbo.MucTieuTietKiem', N'UserId') IS NULL OR
       COL_LENGTH(N'dbo.MucTieuTietKiem', N'TenMucTieu') IS NULL OR
       COL_LENGTH(N'dbo.MucTieuTietKiem', N'SoTienMucTieu') IS NULL OR
       COL_LENGTH(N'dbo.MucTieuTietKiem', N'SoTienHienTai') IS NULL OR
       COL_LENGTH(N'dbo.MucTieuTietKiem', N'NgayKetThuc') IS NULL OR
       COL_LENGTH(N'dbo.MucTieuTietKiem', N'TrangThai') IS NULL
        THROW 52091, N'Bảng MucTieuTietKiem đang có không khớp với model của nhánh nhóm.', 1;

    IF OBJECT_ID(N'dbo.LichSuTietKiem', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.LichSuTietKiem
        (
            MaLichSu int IDENTITY(1,1) NOT NULL CONSTRAINT PK_LichSuTietKiem PRIMARY KEY,
            MaMucTieu int NOT NULL,
            SoTien decimal(18,2) NOT NULL,
            NgayGiaoDich datetime2(0) NOT NULL CONSTRAINT DF_Issue9_NgayGiaoDich DEFAULT (SYSDATETIME()),
            GhiChu nvarchar(300) NULL,
            CONSTRAINT FK_Issue9_LichSu_MucTieu FOREIGN KEY (MaMucTieu) REFERENCES dbo.MucTieuTietKiem(MaMucTieu),
            -- Nạp có dấu dương và rút có dấu âm.
            CONSTRAINT CK_Issue9_LichSu_SoTien CHECK (SoTien <> 0)
        );
        CREATE INDEX IX_Issue9_LichSu_MucTieu
            ON dbo.LichSuTietKiem(MaMucTieu, NgayGiaoDich DESC, MaLichSu DESC);
    END;

    IF COL_LENGTH(N'dbo.LichSuTietKiem', N'MaLichSu') IS NULL OR
       COL_LENGTH(N'dbo.LichSuTietKiem', N'MaMucTieu') IS NULL OR
       COL_LENGTH(N'dbo.LichSuTietKiem', N'SoTien') IS NULL OR
       COL_LENGTH(N'dbo.LichSuTietKiem', N'NgayGiaoDich') IS NULL OR
       COL_LENGTH(N'dbo.LichSuTietKiem', N'GhiChu') IS NULL
        THROW 52092, N'Bảng LichSuTietKiem đang có không khớp với model của nhánh nhóm.', 1;

    COMMIT TRANSACTION;
    SELECT DB_NAME() AS DatabaseName, N'Đã kiểm tra đủ cột cho Issue 9.' AS KetQua;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
