using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;


public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.Property<DateTime>("LastUpdated");
        // Primary Key
        builder.HasKey(s => s.Id);

        // Properties
        builder.Property(s => s.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(s => s.RegistrationNumber)
               .IsRequired()
               .HasMaxLength(150);
        builder.Property(s=>s.GPA)
                .IsRequired();
            
                

    }
}