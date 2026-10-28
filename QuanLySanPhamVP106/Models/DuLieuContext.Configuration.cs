using Microsoft.EntityFrameworkCore;

namespace QuanLySanPhamVP106.Models;

public partial class DuLieuContext
{
    public DuLieuContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                @"Server=.\SQLEXPRESS;Database=DuLieu;Integrated Security=True;TrustServerCertificate=True");
        }
    }
}
