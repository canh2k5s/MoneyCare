1. Tổng quan và lý do chọn đề tài
Trong sinh hoạt hằng ngày, sinh viên và người đi làm thường nhận tiền lương, tiền hỗ trợ hoặc thu nhập khác nhưng khó theo dõi đã chi bao nhiêu, còn bao nhiêu và cần để dành bao nhiêu cho các mục tiêu. MoneyCare hỗ trợ ghi chép thủ công, đặt hạn mức chi tiêu theo từng danh mục mỗi tháng và trích tiền từ số dư khả dụng vào các mục tiêu tiết kiệm.
Phạm vi: website cá nhân, không kết nối tài khoản ngân hàng; dữ liệu do người dùng tự nhập và được lưu trong SQL Server. Mỗi tài khoản chỉ được truy cập dữ liệu của chính mình.
2. Mục tiêu và sản phẩm đầu ra
•	Website chạy được trên trình duyệt, có đăng ký/đăng nhập và giao diện tiếng Việt.
•	Ghi nhận thu nhập, chi tiêu; tự tính tổng thu, tổng chi, số dư khả dụng.
•	Tự phân bổ hạn mức chi tiêu theo tháng cho ăn uống, phòng trọ, điện nước, mạng, đi lại, y tế…
•	Tạo mục tiêu tiết kiệm; trích/rút tiền và hiển thị tiến độ hoàn thành.
•	Thống kê, lọc lịch sử giao dịch và cảnh báo khi chi tiêu gần/vượt hạn mức.
3. Chức năng hệ thống
Phân hệ	Chức năng chính
Tài khoản	Đăng ký, đăng nhập, đăng xuất; bảo vệ dữ liệu riêng từng người.
Danh mục	Thêm/sửa/xóa danh mục Thu hoặc Chi; danh mục mẫu: lương, ăn uống, phòng trọ, y tế…
Giao dịch	Thêm/sửa/xóa khoản thu, chi; nhập số tiền, ngày, danh mục, ghi chú; tìm kiếm và lọc.
Ngân sách tháng	Nhập hạn mức thủ công theo danh mục Chi, tháng và năm; hiển thị đã chi/còn lại, cảnh báo 80% và vượt mức.
Mục tiêu tiết kiệm	Tạo mục tiêu, đặt số tiền cần đạt, nạp/rút từ số dư khả dụng, xem lịch sử và tiến độ.
Dashboard	Tổng thu, tổng chi, số dư khả dụng, tiền đang tiết kiệm; biểu đồ cột và tròn.

4. Ví dụ minh họa luồng hoạt động
Tháng 10, người dùng nhập khoản lương 8.000.000đ. Người dùng đặt hạn mức: ăn uống 2.000.000đ, phòng trọ 1.800.000đ, điện/nước/mạng 500.000đ, đi lại 500.000đ, y tế 300.000đ. Đây chỉ là kế hoạch, không trừ tiền khỏi số dư.
Sau đó người dùng ghi nhận chi tiêu thực tế 2.100.000đ và trích 1.000.000đ vào mục tiêu “Mua điện thoại” trị giá 10.000.000đ. Khi đó: tổng thu 8.000.000đ; tổng chi 2.100.000đ; tiền đang tiết kiệm 1.000.000đ; số dư khả dụng 4.900.000đ; tiến độ mục tiêu 10%.
Nếu chi 800.000đ cho ăn uống thì ngân sách ăn uống còn 1.200.000đ. Nếu chi tới 1.600.000đ, hệ thống cảnh báo đạt 80% hạn mức. Việc phân bổ ngân sách không được tính thành chi tiêu; việc chuyển tiền vào mục tiêu không được cộng vào tổng chi.
5. Quy tắc nghiệp vụ và công thức
Chỉ số	Cách tính / quy tắc
Tổng thu	Tổng các giao dịch thuộc danh mục Thu.
Tổng chi	Tổng các giao dịch thuộc danh mục Chi.
Tiền tiết kiệm	Tổng tiền nạp mục tiêu trừ tổng tiền rút.
Số dư khả dụng	Số dư ban đầu + Tổng thu − Tổng chi − Tiền tiết kiệm.
Ngân sách còn lại	Hạn mức tháng của danh mục − tổng chi thực tế cùng tháng, cùng danh mục.
Tiến độ mục tiêu	Tiền đang có trong mục tiêu / số tiền cần đạt × 100%.
Kiểm soát	Không cho nạp tiết kiệm vượt số dư; không cho rút vượt số tiền mục tiêu đang có; số tiền giao dịch phải > 0.

Đơn giản hóa giai đoạn đầu: số dư ban đầu mặc định bằng 0; nếu cần, nhập một khoản Thu “Số dư ban đầu”. Khi sửa/xóa giao dịch phải kiểm tra lại số dư để tránh âm; thao tác trích/rút tiền cần lưu nhất quán trong cơ sở dữ liệu.
6. Thiết kế cơ sở dữ liệu dự kiến
Sử dụng ASP.NET Core Identity để xác thực: bảng tài khoản thực tế là AspNetUsers cùng các bảng hệ thống Identity. Sáu thực thể nghiệp vụ chính dưới đây gồm người dùng và năm bảng chức năng; số bảng SQL thực tế sẽ nhiều hơn do Identity.
Bảng / thực thể	Trường dữ liệu chính	Liên kết
NguoiDung (Identity)	Id, HoTen, Email, PasswordHash	Một người có nhiều danh mục, giao dịch, ngân sách, mục tiêu.
DanhMuc	MaDanhMuc (PK), UserId (FK), TenDanhMuc, Loai	Thu/Chi; thuộc người dùng.
GiaoDich	MaGiaoDich (PK), UserId (FK), MaDanhMuc (FK), SoTien, NgayGiaoDich, GhiChu	Thuộc người dùng và danh mục.
NganSach	MaNganSach (PK), UserId (FK), MaDanhMuc (FK), Thang, Nam, HanMuc	Mỗi danh mục Chi tối đa một hạn mức/tháng/năm.
MucTieuTietKiem	MaMucTieu (PK), UserId (FK), TenMucTieu, SoTienMucTieu, NgayTao, TrangThai	Một người có nhiều mục tiêu.
LichSuTietKiem	MaLichSu (PK), MaMucTieu (FK), SoTien, Loai (Nap/Rut), NgayThucHien	Một mục tiêu có nhiều lần nạp/rút.

Ràng buộc: số tiền dương; tháng 1–12; danh mục ngân sách phải là loại Chi; khóa ngoại phải thuộc đúng tài khoản; đặt unique(UserId, MaDanhMuc, Thang, Nam) cho NganSach. Tiền trong mục tiêu được tính từ lịch sử nạp/rút, không cần lưu lặp số dư.
7. Công nghệ và kiến trúc
Thành phần	Lựa chọn
Backend	C# – ASP.NET Core MVC (.NET 9)
CSDL	SQL Server, Entity Framework Core, EF Core Migrations
Xác thực	ASP.NET Core Identity
Frontend	Razor Views, HTML, CSS, Bootstrap 5
Biểu đồ	Chart.js
Làm việc nhóm	Git, GitHub, Pull Request
Kiến trúc	MVC; các module dùng chung ApplicationDbContext
