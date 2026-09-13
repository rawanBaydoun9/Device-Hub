using Microsoft.EntityFrameworkCore;
using Task2.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace Task2.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Device> Devices { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Client> Clients { get; set; }

        public DbSet<PhoneNumber> PhoneNumbers { get; set; }

        public DbSet<PhoneNumberReservation> PhoneNumberReservations { get; set; }

        public DbSet<ImportHistory> ImportHistories { get; set; }

        public DbSet<ImportHistoryRow> ImportHistoryRows { get; set; }

        public DbSet<HomeItemSetting> HomeItemSettings { get; set; }

        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<DeviceRequest> DeviceRequests { get; set; }

        public DbSet<DeviceRequestItem> DeviceRequestItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
    .Property(u => u.FullName)
    .HasMaxLength(80);


            modelBuilder.Entity<Device>()
                .Property(d => d.Name)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Device>()
                .HasIndex(d => d.Name)
                .IsUnique();

            modelBuilder.Entity<Device>()
                .Property(d => d.Quantity)
                .HasDefaultValue(1);

            modelBuilder.Entity<Category>()
                .Property(c => c.Name)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<Device>()
                .HasOne(d => d.Category)
                .WithMany(c => c.Devices)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Client>()
                .Property(c => c.Name)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Name)
                .IsUnique();

            modelBuilder.Entity<Client>()
                .Property(c => c.Type)
                .HasConversion<int>()
                .IsRequired();

            modelBuilder.Entity<PhoneNumber>()
                .Property(p => p.Number)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<PhoneNumber>()
                .HasIndex(p => p.Number)
                .IsUnique();

            modelBuilder.Entity<PhoneNumber>()
                .HasOne(p => p.Device)
                .WithMany(d => d.PhoneNumbers)
                .HasForeignKey(p => p.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PhoneNumberReservation>()
    .HasOne(r => r.Client)
    .WithMany(c => c.PhoneNumberReservations)
    .HasForeignKey(r => r.ClientId)
    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PhoneNumberReservation>()
                .HasOne(r => r.PhoneNumber)
                .WithMany(p => p.PhoneNumberReservations)
                .HasForeignKey(r => r.PhoneNumberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PhoneNumberReservation>()
                .Property(r => r.BED)
                .IsRequired();

            modelBuilder.Entity<ImportHistory>()
    .Property(i => i.ImportType)
    .HasMaxLength(50)
    .IsRequired();

            modelBuilder.Entity<ImportHistory>()
                .Property(i => i.FileName)
                .HasMaxLength(255)
                .IsRequired();

            modelBuilder.Entity<ImportHistory>()
                .Property(i => i.Status)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<ImportHistory>()
                .HasMany(i => i.Rows)
                .WithOne(r => r.ImportHistory)
                .HasForeignKey(r => r.ImportHistoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ImportHistoryRow>()
                .Property(r => r.RecordName)
                .HasMaxLength(100);

            modelBuilder.Entity<ImportHistoryRow>()
                .Property(r => r.Status)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<ImportHistoryRow>()
                .Property(r => r.Message)
                .HasMaxLength(255);




            modelBuilder.Entity<HomeItemSetting>()
    .Property(h => h.Key)
    .HasMaxLength(50)
    .IsRequired();

            modelBuilder.Entity<HomeItemSetting>()
                .HasIndex(h => h.Key)
                .IsUnique();

            modelBuilder.Entity<HomeItemSetting>()
                .Property(h => h.Title)
                .HasMaxLength(80)
                .IsRequired();

            modelBuilder.Entity<HomeItemSetting>()
                .Property(h => h.Description)
                .HasMaxLength(150);

            modelBuilder.Entity<HomeItemSetting>()
                .Property(h => h.IconClass)
                .HasMaxLength(80);

            modelBuilder.Entity<HomeItemSetting>()
                .Property(h => h.ControllerName)
                .HasMaxLength(50);

            modelBuilder.Entity<HomeItemSetting>()
                .Property(h => h.ActionName)
                .HasMaxLength(50);

            modelBuilder.Entity<CartItem>()
    .Property(c => c.UserId)
    .IsRequired();

            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Device)
                .WithMany()
                .HasForeignKey(c => c.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CartItem>()
                .HasIndex(c => new { c.UserId, c.DeviceId })
                .IsUnique();


            modelBuilder.Entity<DeviceRequest>()
    .Property(r => r.UserId)
    .IsRequired();

            modelBuilder.Entity<DeviceRequest>()
                .Property(r => r.Status)
                .HasMaxLength(30)
                .IsRequired();

            modelBuilder.Entity<DeviceRequest>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DeviceRequest>()
                .HasMany(r => r.Items)
                .WithOne(i => i.DeviceRequest)
                .HasForeignKey(i => i.DeviceRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DeviceRequestItem>()
                .HasOne(i => i.Device)
                .WithMany()
                .HasForeignKey(i => i.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}