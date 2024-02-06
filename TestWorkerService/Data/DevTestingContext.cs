using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TestWorkerService.Models;

namespace TestWorkerService.Data
{
    public partial class DevTestingContext : DbContext
    {
        public DevTestingContext()
        {
        }

        public DevTestingContext(DbContextOptions<DevTestingContext> options)
            : base(options)
        {
        }

        public virtual DbSet<EmployeeTemp> EmployeeTemps { get; set; } = null!;

        public virtual DbSet<StoredProc_Result> StoredProc_Result { get; set; }=null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=KTP-KIT008;Database=MOLExcelData;User Id=sa;Password=Password@1;MultipleActiveResultSets=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StoredProc_Result>(ent =>
            {
                ent.HasNoKey();
            });
            modelBuilder.Entity<EmployeeTemp>(entity =>
            {
                entity.ToTable("EmployeeTemp");

                entity.Property(e => e.DOCIMO).ValueGeneratedNever();

                entity.Property(e => e.船主名)
                    .HasMaxLength(100)
                    .IsUnicode(true);


                entity.Property(e => e.DOCName)
                    .HasMaxLength(100)
                    .IsUnicode(true);

                entity.Property(e => e.OriginalOperatorName)
                    .HasMaxLength(100)
                    .IsUnicode(true);

                entity.Property(e => e.BeneficialOwnerName)
                    .HasMaxLength(100)
                    .IsUnicode(true);

                entity.Property(e => e.RegisteredOwnerName)
             .HasMaxLength(100)
             .IsUnicode(true);

                entity.Property(e => e.OriginalOwner)
          .HasMaxLength(100)
          .IsUnicode(true);
                entity.Property(e => e.MFDJA)
     .HasMaxLength(100)
     .IsUnicode(true);

            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
