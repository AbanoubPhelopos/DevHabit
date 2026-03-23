namespace DevHabit.Infrastructure.Database.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();
        builder.Property(a => a.EntityName).HasMaxLength(500).IsRequired();
        builder.Property(a => a.EntityId).HasMaxLength(500).IsRequired();
        builder.Property(a => a.Action).HasMaxLength(100).IsRequired();
        builder.Property(a => a.OldValues).HasColumnType("jsonb");
        builder.Property(a => a.NewValues).HasColumnType("jsonb");
        builder.Property(a => a.UserId).HasMaxLength(500);
        builder.Property(a => a.UserName).HasMaxLength(500);
        builder.Property(a => a.IpAddress).HasMaxLength(100);
        builder.Property(a => a.TablePartition).HasMaxLength(100);

        builder.HasIndex(a => a.EntityName);
        builder.HasIndex(a => a.EntityId);
        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => a.UserId);
    }
}
