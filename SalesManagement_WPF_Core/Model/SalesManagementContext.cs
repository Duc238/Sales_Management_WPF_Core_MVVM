using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace SalesManagement_WPF_Core.Model;

public partial class SalesManagementContext : DbContext
{
    public SalesManagementContext()
    {
        // Tự động Migrate/Tạo database + các Bảng nếu máy khách chưa có
        this.Database.Migrate();
    }

    public SalesManagementContext(DbContextOptions<SalesManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CaculateOutputPrice> CaculateOutputPrices { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Input> Inputs { get; set; }

    public virtual DbSet<InputInfo> InputInfos { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }

    public virtual DbSet<Object> Objects { get; set; }

    public virtual DbSet<Output> Outputs { get; set; }

    public virtual DbSet<OutputInfo> OutputInfos { get; set; }

    public virtual DbSet<Suplier> Supliers { get; set; }

    public virtual DbSet<Unit> Units { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // 1. Đọc từ file appsettings.json nếu có
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // 2. Nếu không thấy trong json, dùng chuỗi mặc định trỏ vào .\SQLEXPRESS01
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Data Source=.\\SQLEXPRESS01;Initial Catalog=SalesManagement;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=30;Encrypt=False;";
            }

            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CaculateOutputPrice>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("CaculateOutputPrice");

            entity.Property(e => e.Id).HasMaxLength(128);
            entity.Property(e => e.IdObject).HasMaxLength(128);
            entity.Property(e => e.InputPrice).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.OutputPrice).HasColumnType("numeric(37, 6)");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC0775AB5423");

            entity.ToTable("Customer");

            entity.Property(e => e.ContractDate).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<Input>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Input__3214EC0707EBC127");

            entity.ToTable("Input");

            entity.Property(e => e.Id).HasMaxLength(128);
            entity.Property(e => e.DateInput).HasColumnType("datetime");
        });

        modelBuilder.Entity<InputInfo>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("InputInfo");

            entity.Property(e => e.Id).HasMaxLength(128);
            entity.Property(e => e.IdInput).HasMaxLength(128);
            entity.Property(e => e.IdObject).HasMaxLength(128);
            entity.Property(e => e.InputPrice).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.OutputPrice).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.IdInputNavigation).WithMany()
                .HasForeignKey(d => d.IdInput)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InputInfo__IdInp__31EC6D26");

            entity.HasOne(d => d.IdObjectNavigation).WithMany()
                .HasForeignKey(d => d.IdObject)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InputInfo__IdObj__32E0915F");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Invoice__3214EC072A61279A");

            entity.ToTable("Invoice");

            entity.Property(e => e.ChangeMoney).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.CustomerPayment).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.DateCreated).HasColumnType("datetime");
            entity.Property(e => e.GrandTotal).HasColumnType("decimal(18, 0)");
        });

        modelBuilder.Entity<InvoiceDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__InvoiceD__3214EC07DD58D934");

            entity.ToTable("InvoiceDetail");

            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.OutputPrice).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InvoiceDe__Total__59FA5E80");
        });

        modelBuilder.Entity<Object>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Object__3214EC0783820A16");

            entity.ToTable("Object");

            entity.Property(e => e.Id).HasMaxLength(128);
            entity.Property(e => e.InputPrice).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.OutputPrice).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.IdSuplierNavigation).WithMany(p => p.Objects)
                .HasForeignKey(d => d.IdSuplier)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Object__IdSuplie__33D4B598");

            entity.HasOne(d => d.IdUnitNavigation).WithMany(p => p.Objects)
                .HasForeignKey(d => d.IdUnit)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Object__IdUnit__34C8D9D1");
        });

        modelBuilder.Entity<Output>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Output__3214EC07013C6F17");

            entity.ToTable("Output");

            entity.Property(e => e.DateOutput).HasColumnType("datetime");
        });

        modelBuilder.Entity<OutputInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OutputIn__3214EC07BFF6C5B5");

            entity.ToTable("OutputInfo");

            entity.Property(e => e.IdObject).HasMaxLength(128);
            entity.Property(e => e.Status).HasMaxLength(200);

            entity.HasOne(d => d.IdCustomerNavigation).WithMany(p => p.OutputInfos)
                .HasForeignKey(d => d.IdCustomer)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OutputInf__Statu__2180FB33");

            entity.HasOne(d => d.IdObjectNavigation).WithMany(p => p.OutputInfos)
                .HasForeignKey(d => d.IdObject)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OutputInf__IdObj__22751F6C");

            entity.HasOne(d => d.IdOutputNavigation).WithMany(p => p.OutputInfos)
                .HasForeignKey(d => d.IdOutput)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OutputInf__IdOut__236943A5");
        });

        modelBuilder.Entity<Suplier>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Suplier__3214EC07C4D01120");

            entity.ToTable("Suplier");

            entity.Property(e => e.ContractDate).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Unit__3214EC0724DDFF87");

            entity.ToTable("Unit");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
