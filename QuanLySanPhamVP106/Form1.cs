using Microsoft.EntityFrameworkCore;
using QuanLySanPhamVP106.Models;
using System.Globalization;

namespace QuanLySanPhamVP106
{
    public partial class Form1 : Form
    {
        private string? maSanPhamDangChon;
        private string giaBanHopLeTruocDo = string.Empty;

        public Form1()
        {
            InitializeComponent();
            txtGiaBan.KeyPress += txtGiaBan_KeyPress;
            txtGiaBan.TextChanged += txtGiaBan_TextChanged;
            dgvSanPham.ReadOnly = true;
            dgvSanPham.AllowUserToAddRows = false;
            dgvSanPham.MultiSelect = false;
            dgvSanPham.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSanPham.SelectionChanged += dgvSanPham_SelectionChanged;
        }

        private static bool LaGiaBanDangNhap(string text)
        {
            if (text.Length > 12) return false;

            int soDauThapPhan = 0;
            foreach (char kyTu in text)
            {
                if (kyTu >= '0' && kyTu <= '9') continue;

                if (kyTu == '.' || kyTu == ',')
                {
                    soDauThapPhan++;
                    if (soDauThapPhan > 1) return false;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private static bool LaGiaBanHopLe(string text)
        {
            return LaGiaBanDangNhap(text) && decimal.TryParse(
                text.Replace(',', '.'), NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out _);
        }

        private void txtGiaBan_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;

            string textMoi = txtGiaBan.Text
                .Remove(txtGiaBan.SelectionStart, txtGiaBan.SelectionLength)
                .Insert(txtGiaBan.SelectionStart, e.KeyChar.ToString());
            e.Handled = !LaGiaBanDangNhap(textMoi);
        }

        private void txtGiaBan_TextChanged(object? sender, EventArgs e)
        {
            if (LaGiaBanDangNhap(txtGiaBan.Text))
            {
                giaBanHopLeTruocDo = txtGiaBan.Text;
                return;
            }

            int viTri = Math.Min(txtGiaBan.SelectionStart, giaBanHopLeTruocDo.Length);
            txtGiaBan.Text = giaBanHopLeTruocDo;
            txtGiaBan.SelectionStart = viTri;
        }

        private void dgvSanPham_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvSanPham.SelectedRows.Count == 0) return;

            SanPham? sp = dgvSanPham.SelectedRows[0].DataBoundItem as SanPham;
            if (sp == null) return;

            maSanPhamDangChon = sp.MaSanPham;
            txtMaSP.Text = sp.MaSanPham;
            txtTenSP.Text = sp.TenSanPham;
            txtGiaBan.Text = sp.GiaBan;
            txtMaSP.ReadOnly = true;
            btnSua.Enabled = true;
            btnXoa.Enabled = true;
            btnThem.Enabled = false;
        }

        private void DatLaiTrangThai()
        {
            maSanPhamDangChon = null;
            txtMaSP.Clear();
            txtTenSP.Clear();
            txtGiaBan.Clear();
            txtMaSP.ReadOnly = false;
            dgvSanPham.ClearSelection();
            dgvSanPham.CurrentCell = null;
            btnSua.Enabled = false;
            btnXoa.Enabled = false;
            btnThem.Enabled = true;
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có muốn thoát không", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Close();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TaiDanhSach();
        }

        private void TaiDanhSach()
        {
            try
            {
                using var db = new DuLieuContext();
                dgvSanPham.DataSource = db.SanPhams.AsNoTracking().ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải dữ liệu: " + ex.Message);
            }
            DatLaiTrangThai();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(maSanPhamDangChon)) return;

            try
            {
                if (MessageBox.Show("Bạn có muốn xóa sản phẩm không", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                using var db = new DuLieuContext();
                var sp = db.SanPhams.Find(maSanPhamDangChon);
                if (sp != null)
                {
                    db.SanPhams.Remove(sp);
                    db.SaveChanges();
                }

                TaiDanhSach();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
            finally
            {
                DatLaiTrangThai();
            }

        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(maSanPhamDangChon)) return;

            string ten = txtTenSP.Text.Trim();
            string gia = txtGiaBan.Text.Trim();
            if (string.IsNullOrEmpty(ten))
            {
                MessageBox.Show("Tên sản phẩm không được để trống.");
                txtTenSP.Focus();
                return;
            }
            if (!LaGiaBanHopLe(gia))
            {
                MessageBox.Show("Giá bán phải là số không âm và tối đa 12 ký tự.");
                txtGiaBan.Focus();
                return;
            }

            try
            {
                using var db = new DuLieuContext();
                var sp = db.SanPhams.Find(maSanPhamDangChon);
                if (sp == null)
                {
                    MessageBox.Show("Sản phẩm đang chọn không còn tồn tại.");
                    TaiDanhSach();
                    return;
                }

                bool trungTen = db.SanPhams
                    .Where(p => p.MaSanPham != maSanPhamDangChon)
                    .Select(p => p.TenSanPham)
                    .AsEnumerable().Any(t => string.Equals(t, ten, StringComparison.OrdinalIgnoreCase));
                if (trungTen)
                {
                    MessageBox.Show("Tên sản phẩm đã tồn tại.");
                    return;
                }

                sp.TenSanPham = ten;
                sp.GiaBan = gia;
                db.SaveChanges();

                TaiDanhSach();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể sửa sản phẩm: " + ex.Message);
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            string ma = txtMaSP.Text.Trim();
            string ten = txtTenSP.Text.Trim();
            string gia = txtGiaBan.Text.Trim();
            if (string.IsNullOrEmpty(ma) || ma.Length > 10)
            {
                MessageBox.Show("Mã sản phẩm phải có từ 1 đến 10 ký tự.");
                return;
            }
            if (string.IsNullOrEmpty(ten))
            {
                MessageBox.Show("Tên sản phẩm không được để trống.");
                return;
            }
            if (!LaGiaBanHopLe(gia))
            {
                MessageBox.Show("Giá bán phải là số không âm và tối đa 12 ký tự.");
                return;
            }
            try
            {
                using var db = new DuLieuContext();
                if (db.SanPhams.Any(sp => sp.MaSanPham == ma))
                {
                    MessageBox.Show("Mã sản phẩm đã tồn tại.");
                    return;
                }
                bool trungTen = db.SanPhams.Select(sp => sp.TenSanPham)
                    .AsEnumerable().Any(t => string.Equals(t, ten, StringComparison.OrdinalIgnoreCase));
                if (trungTen)
                {
                    MessageBox.Show("Tên sản phẩm đã tồn tại.");
                    return;
                }
                var moi = new SanPham
                {
                    MaSanPham = ma,
                    TenSanPham = ten,
                    GiaBan = gia
                };
                db.SanPhams.Add(moi);
                db.SaveChanges();
                MessageBox.Show("Thêm thành công.");
                TaiDanhSach();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể thêm sản phẩm: " + ex.Message);
            }
        }
    }
}
