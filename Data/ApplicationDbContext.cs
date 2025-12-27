using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SaccoShareManagementSys.Models;


namespace SaccoShareManagementSys.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<Shareholder> Shareholders { get; set; }
        public DbSet<Share> Shares { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<ShareTransfer> ShareTransfers { get; set; }
        public DbSet<Dividend> Dividends { get; set; }
        public DbSet<ProfileImg> ProfileImg { get; set; }

        public DbSet<ShareTransaction> ShareTransactions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Shareholder entity
            modelBuilder.Entity<Shareholder>(entity =>
            {
                entity.ToTable("Shareholders");
                entity.HasKey(e => e.ShareholderId);

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasIndex(e => e.Phone)
                    .IsUnique();

                entity.Property(e => e.TotalShares)
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValue(0);

                entity.Property(e => e.CurrentBalance)
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValue(0);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("Active");

                entity.Property(e => e.MemberType)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("New");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");
            });

            // Configure Share entity
            modelBuilder.Entity<Share>(entity =>
            {
                entity.ToTable("Shares");
                entity.HasKey(e => e.ShareId);

                entity.Property(e => e.CertificateNumber)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(e => e.CertificateNumber)
                    .IsUnique();

                entity.Property(e => e.ShareType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.NumberOfShares)
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ShareValue)
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("Active");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                // Relationship with Shareholder
                entity.HasOne(e => e.Shareholder)
                    .WithMany(s => s.Shares)
                    .HasForeignKey(e => e.ShareholderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Transaction entity
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transactions");
                entity.HasKey(e => e.TransactionId);

                entity.Property(e => e.TransactionType)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.PaymentMethod)
                    .HasMaxLength(50);

                entity.Property(e => e.ReferenceNumber)
                    .HasMaxLength(100);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("Completed");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.TransactionDate);
                entity.HasIndex(e => e.ReferenceNumber);

                // Relationship with Share
                //    entity.HasOne(e => e.Share)
                //        .WithMany(s => s.Transactions)
                //        .HasForeignKey(e => e.ShareId)
                //        .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<ShareTransaction>(entity =>
            {
                entity.Property(e => e.Amount)
                      .HasPrecision(18, 2);
                entity.HasKey(e => e.TransactionId);

                entity.HasOne(e => e.Share)
                      .WithMany(s => s.Transactions)
                      .HasForeignKey(e => e.ShareId)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            // Configure ShareTransfer entity
            modelBuilder.Entity<ShareTransfer>(entity =>
            {
                entity.ToTable("ShareTransfers");
                entity.HasKey(e => e.TransferId);

                entity.Property(e => e.ShareAmount)
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("Completed");

                entity.Property(e => e.Notes)
                    .HasMaxLength(500);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.TransferDate);

                // Relationship with FromShareholder
                entity.HasOne(e => e.FromShareholder)
                    .WithMany(s => s.TransfersFrom)
                    .HasForeignKey(e => e.FromShareholderId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relationship with ToShareholder
                entity.HasOne(e => e.ToShareholder)
                    .WithMany(s => s.TransfersTo)
                    .HasForeignKey(e => e.ToShareholderId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Check constraints
                //entity.ToTable(t =>
                //{
                //    t.HasCheckConstraint("CK_Shareholder_Balance", "CurrentBalance >= 0");
                //    t.HasCheckConstraint("CK_Shareholder_Shares", "TotalShares >= 0");
                //});

            });

            // Configure Dividend entity
            modelBuilder.Entity<Dividend>(entity =>
            {
                entity.ToTable("Dividends");
                entity.HasKey(e => e.DividendId);

                entity.Property(e => e.TotalProfit)
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TotalShares)
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.DividendRate)
                    .HasColumnType("decimal(18, 4)");

                entity.Property(e => e.TotalDividendPaid)
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("Pending");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");

                // Unique constraint for Year and Month combination
                entity.HasIndex(e => new { e.Year, e.Month })
                    .IsUnique();
            });

            // Seed initial data (optional)
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed sample shareholders
            modelBuilder.Entity<Shareholder>().HasData(
                new Shareholder
                {
                    ShareholderId = 1,
                    FullName = "Rediet Tsega",
                    Email = "redu.tga@example.com",
                    Phone = "+254-712-345-678",
                    JoinDate = new DateTime(2024, 1, 15),
                    TotalShares = 25420,
                    CurrentBalance = 25420,
                    NumberOfCertificates = 2,
                    Status = "Active",
                    MemberType = "Premium",
                    Address = "123 Main Street",
                    City = "Nairobi",
                    IdNumber = "12345678",
                    Gender = "Male",
                    CreatedAt = DateTime.Now
                },
                new Shareholder
                {
                    ShareholderId = 2,
                    FullName = "Jane Smith",
                    Email = "jane.smith@example.com",
                    Phone = "+254-723-456-789",
                    JoinDate = new DateTime(2024, 2, 20),
                    TotalShares = 18890,
                    CurrentBalance = 18890,
                    NumberOfCertificates = 1,
                    Status = "Active",
                    MemberType = "Active",
                    Address = "456 Oak Avenue",
                    City = "Mombasa",
                    IdNumber = "23456789",
                    Gender = "Female",
                    CreatedAt = DateTime.Now
                }
            );

            // Seed sample shares
            modelBuilder.Entity<Share>().HasData(
                new Share
                {
                    ShareId = 1,
                    ShareholderId = 1,
                    CertificateNumber = "CERT-2024-001",
                    ShareType = "Ordinary Shares",
                    NumberOfShares = 100,
                    ShareValue = 15000,
                    PurchaseDate = new DateTime(2024, 1, 15),
                    Status = "Active",
                    CreatedAt = DateTime.Now
                },
                new Share
                {
                    ShareId = 2,
                    ShareholderId = 2,
                    CertificateNumber = "CERT-2024-002",
                    ShareType = "Ordinary Shares",
                    NumberOfShares = 75,
                    ShareValue = 11250,
                    PurchaseDate = new DateTime(2024, 2, 20),
                    Status = "Active",
                    CreatedAt = DateTime.Now
                }
            );
        }



    }
}