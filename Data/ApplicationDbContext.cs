using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using concerts_gate.server.Entities;

namespace concerts_gate.server.Data;

/// <summary>
/// Primary Entity Framework Core database context for the Concerts Gate system.
/// Extends <see cref="IdentityDbContext{TUser, TRole, TKey}"/> to manage ASP.NET Core Identity authentication and roles.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    /// <summary>
    /// Initializes a new instance of <see cref="ApplicationDbContext"/>.
    /// </summary>
    /// <param name="options">Database context options.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Concerts entity set.
    /// </summary>
    public DbSet<Concert> Concerts => Set<Concert>();

    /// <summary>
    /// Ticket categories entity set.
    /// </summary>
    public DbSet<TicketCategory> TicketCategories => Set<TicketCategory>();

    /// <summary>
    /// Bookings entity set.
    /// </summary>
    public DbSet<Booking> Bookings => Set<Booking>();

    /// <summary>
    /// Booking line items entity set.
    /// </summary>
    public DbSet<BookingItem> BookingItems => Set<BookingItem>();

    /// <summary>
    /// Issued electronic tickets entity set.
    /// </summary>
    public DbSet<Ticket> Tickets => Set<Ticket>();

    /// <summary>
    /// Promotional vouchers entity set.
    /// </summary>
    public DbSet<Voucher> Vouchers => Set<Voucher>();

    /// <summary>
    /// Voucher redemption usage history entity set.
    /// </summary>
    public DbSet<VoucherUsage> VoucherUsages => Set<VoucherUsage>();

    /// <summary>
    /// Idempotency records entity set.
    /// </summary>
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    /// <summary>
    /// Operational audit log records entity set.
    /// </summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    /// <summary>
    /// Configures Fluent API mappings, indices, relationships, and concurrency tokens.
    /// </summary>
    /// <param name="builder">Model builder instance.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // --- Concert Configuration ---
        builder.Entity<Concert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(250).IsRequired();
            entity.Property(e => e.Artist).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Venue).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Genre).HasMaxLength(100);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.EventDate);
        });

        // --- TicketCategory Configuration ---
        builder.Entity<TicketCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.RowVersion).IsRowVersion(); // Optimistic concurrency token
            entity.HasOne(e => e.Concert)
                  .WithMany(c => c.TicketCategories)
                  .HasForeignKey(e => e.ConcertId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Booking Configuration ---
        builder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BookingCode).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.BookingCode).IsUnique();
            entity.Property(e => e.OriginalAmount).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.FinalAmount).HasPrecision(18, 2);
            entity.Property(e => e.IdempotencyKey).HasMaxLength(100);
            entity.HasIndex(e => e.IdempotencyKey);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ReservationExpiresAt);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Bookings)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Concert)
                  .WithMany(c => c.Bookings)
                  .HasForeignKey(e => e.ConcertId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // --- BookingItem Configuration ---
        builder.Entity<BookingItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.HasOne(e => e.Booking)
                  .WithMany(b => b.BookingItems)
                  .HasForeignKey(e => e.BookingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TicketCategory)
                  .WithMany(tc => tc.BookingItems)
                  .HasForeignKey(e => e.TicketCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Ticket Configuration ---
        builder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TicketCode).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.TicketCode).IsUnique();
            entity.Property(e => e.QrCodePayload).HasMaxLength(500).IsRequired();

            entity.HasOne(e => e.Booking)
                  .WithMany(b => b.Tickets)
                  .HasForeignKey(e => e.BookingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TicketCategory)
                  .WithMany(tc => tc.Tickets)
                  .HasForeignKey(e => e.TicketCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Voucher Configuration ---
        builder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.DiscountValue).HasPrecision(18, 2);
            entity.Property(e => e.MaxDiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.MinOrderAmount).HasPrecision(18, 2);
            entity.Property(e => e.RowVersion).IsRowVersion(); // Optimistic concurrency token
        });

        // --- VoucherUsage Configuration ---
        builder.Entity<VoucherUsage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DiscountApplied).HasPrecision(18, 2);

            entity.HasOne(e => e.Voucher)
                  .WithMany(v => v.Usages)
                  .HasForeignKey(e => e.VoucherId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.VoucherUsages)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Booking)
                  .WithMany()
                  .HasForeignKey(e => e.BookingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.VoucherId, e.UserId });
        });

        // --- IdempotencyRecord Configuration ---
        builder.Entity<IdempotencyRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => new { e.Key, e.UserId }).IsUnique();
        });

        // --- AuditLog Configuration ---
        builder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
            entity.Property(e => e.TargetEntity).HasMaxLength(100).IsRequired();
            entity.Property(e => e.TargetId).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Timestamp);
        });
    }
}
