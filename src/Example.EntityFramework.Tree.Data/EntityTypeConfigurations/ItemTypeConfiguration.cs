using System;
using Example.EntityFramework.Tree.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Example.EntityFramework.Tree.Data.EntityTypeConfigurations
{
    public class ItemTypeConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.ToTable(nameof(Item));

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(36)
                ;
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(1000)
                ;

            builder.Property(x => x.Url)
                .IsRequired(false)
                .HasMaxLength(1000)
                ;

            builder.Property(x => x.Order)
                .IsRequired()
                .HasDefaultValue(1)
                ;

            builder.Property(x => x.Level)
                .IsRequired()
                .HasDefaultValue(1)
                ;
            builder.Property(x => x.LanguageCode)
                .IsRequired()
                .HasDefaultValue("en")
                .HasMaxLength(10)
                ;

            builder.Property(x => x.ParentId)
                .IsRequired(false)
                .HasConversion<string>()
                .HasMaxLength(36)
                ;

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

