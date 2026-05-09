using Microsoft.EntityFrameworkCore;
using SSRd.Models;

namespace SSRd.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TmMembership> TmMemberships { get; set; }
    public DbSet<TtRdDeposit> TtRdDeposit { get; set; }
    public DbSet<TmOtp> TmOtps { get; set; }
    public DbSet<TtRdDepositOnline> TtRdDepositOnlines { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TmMembership>(entity =>
        {
            entity.ToTable("tm_membership");
            entity.HasKey(e => e.MemberId);
            entity.Property(e => e.MemberId).HasColumnName("MEMBER_ID").ValueGeneratedOnAdd();
            entity.Property(e => e.AuthStatus).HasDefaultValue("U");
            entity.Property(e => e.Age).HasDefaultValue(0.0);
            entity.HasIndex(e => e.MembershipNo).HasDatabaseName("index2");
        });
        
        modelBuilder.Entity<TtRdDepositOnline>(entity =>
        {
            entity.ToTable("tt_rddepositonline");
            entity.HasKey(e => e.RdDepositsId);
            entity.Property(e => e.AuthStatus).HasDefaultValue("U");
            entity.HasIndex(e => e.MEMBERSHIP_NO).HasDatabaseName("index2");
        });

        modelBuilder.Entity<TmOtp>(entity =>
        {
            entity.ToTable("tm_otp");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IsUsed).HasDefaultValue(false);
            entity.HasIndex(e => e.PhoneNo).HasDatabaseName("idx_otp_phone");
        });
    }
}
