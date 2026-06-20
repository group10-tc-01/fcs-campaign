using System.Diagnostics.CodeAnalysis;
using fcs.Campaign.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace fcs.Campaign.Infrastructure.SqlServer.Persistence.Configurations;

[ExcludeFromCodeCoverage]

public abstract class BaseConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.CreatedAt).IsRequired();
        builder.Property(entity => entity.UpdatedAt);
        builder.Property(entity => entity.IsActive).IsRequired();
    }
}
