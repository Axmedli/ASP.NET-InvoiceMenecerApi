using InvoiceMenecer.Models;
using Microsoft.EntityFrameworkCore;

public class InvoiceMenecerDBContext : DbContext
{
    public InvoiceMenecerDBContext(DbContextOptions<InvoiceMenecerDBContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceRow> InvoiceRows => Set<InvoiceRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Address)
                .HasMaxLength(500);

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(e => e.CreatedAt)
                .IsRequired();


        });


        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TotalSum)
                .HasPrecision(18, 2);

            entity.Property(e => e.Comment)
                .HasMaxLength(1000);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.StartDate)
                .IsRequired();

            entity.Property(e => e.EndDate)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();


            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Invoices)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

        });


        modelBuilder.Entity<InvoiceRow>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Service)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(e => e.Quantity)
                .HasPrecision(18, 2);

            entity.Property(e => e.Amount)
                .HasPrecision(18, 2);

            entity.Property(e => e.Sum)
                .HasPrecision(18, 2);

            entity.HasOne(e => e.Invoice)
                .WithMany(i => i.Rows)
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

        });
    }
}