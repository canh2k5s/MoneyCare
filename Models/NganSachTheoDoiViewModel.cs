namespace MoneyCare.Models
{
    public enum MucCanhBaoNganSach
    {
        BinhThuong,
        SapChamHanMuc,
        DatHanMuc,
        VuotHanMuc
    }

    public class NganSachTheoDoiViewModel
    {
        public int MaNganSach { get; set; }
        public int UserId { get; set; }
        public byte Thang { get; set; }
        public short Nam { get; set; }
        public int MaDanhMuc { get; set; }
        public string TenDanhMuc { get; set; } = string.Empty;
        public decimal HanMuc { get; set; }
        public decimal DaChi { get; set; }

        public decimal ConLai => HanMuc - DaChi;
        public decimal PhanTramDaDung => HanMuc > 0 ? DaChi / HanMuc * 100m : 0m;

        // Xét bằng số tiền thực, không dựa vào phần trăm đã định dạng.
        public MucCanhBaoNganSach CanhBao =>
            HanMuc <= 0 ? MucCanhBaoNganSach.BinhThuong :
            DaChi > HanMuc ? MucCanhBaoNganSach.VuotHanMuc :
            DaChi == HanMuc ? MucCanhBaoNganSach.DatHanMuc :
            DaChi >= HanMuc * 0.8m ? MucCanhBaoNganSach.SapChamHanMuc :
            MucCanhBaoNganSach.BinhThuong;
    }
}
