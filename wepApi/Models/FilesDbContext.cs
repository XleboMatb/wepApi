using Microsoft.EntityFrameworkCore;

namespace wepApi.Models
{
    public class FilesDbContext : DbContext
    {
        public FilesDbContext(DbContextOptions<FilesDbContext> options) : base(options)
        {
            
        }
        public DbSet<Value> Values { get; set; }
        public DbSet<Result> Result { get; set; }
        public DbSet<Models.File> Files { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конфигурация для File
            modelBuilder.Entity<File>(entity =>
            {
                entity.ToTable("files");
                entity.HasKey(e => e.FileID);
                entity.Property(e => e.FileID)
                    .HasColumnName("fileid")
                    .ValueGeneratedOnAdd(); // Автоинкремент
                entity.Property(e => e.FileName)
                    .HasColumnName("filename")
                    .IsRequired()
                    .HasMaxLength(255);
            });

            // Конфигурация для Value
            modelBuilder.Entity<Value>(entity =>
            {
                entity.ToTable("values");
                entity.HasKey(e => e.ValueID);

                entity.Property(e => e.ValueID)
                    .HasColumnName("valuesid")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ValueFileID)
                    .HasColumnName("fileid");

                entity.Property(e => e.ValueDateOfStart)
                    .HasColumnName("valuesstartdate")
                    .HasColumnType("timestamp"); // Тип для даты/времени в PostgreSQL

                entity.Property(e => e.ValueExecTime)
                    .HasColumnName("valuesexecutiontime");

                entity.Property(e => e.ValueDecimalVal)
                    .HasColumnName("valuesindicator")
                    .HasColumnType("double precision"); // Тип для float/double в PostgreSQL

                // Связь с File
                entity.HasOne<File>()
                    .WithMany()
                    .HasForeignKey(v => v.ValueFileID)
                    .HasConstraintName("fk_values_files")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация для Result
            modelBuilder.Entity<Result>(entity =>
            {
                entity.ToTable("result");
                entity.HasKey(e => e.ResultID);

                entity.Property(e => e.ResultID)
                    .HasColumnName("resultid")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.FileID)
                    .HasColumnName("fileid");

                entity.Property(e => e.MinDateOnFirstLaunch)
                    .HasColumnName("resultmindate")
                    .HasColumnType("timestamp");

                entity.Property(e => e.MaxDateOnFirstLaunch)
                    .HasColumnName("resultmaxdate")
                    .HasColumnType("timestamp");

                entity.Property(e => e.DeltaTimeInSec)
                    .HasColumnName("resultdeltatime")
                    .HasColumnType("double precision");

                entity.Property(e => e.AvgExecTime)
                    .HasColumnName("resultavgexectime")
                    .HasColumnType("double precision");

                entity.Property(e => e.AvgIndicatorVal)
                    .HasColumnName("resultavgvalindicator")
                    .HasColumnType("double precision");

                entity.Property(e => e.MedianByIndicator)
                    .HasColumnName("resultmedian")
                    .HasColumnType("double precision");

                entity.Property(e => e.MaxValIndicator)
                    .HasColumnName("resultmax")
                    .HasColumnType("double precision"); // Тип для временного интервала в PostgreSQL

                entity.Property(e => e.MinValIndicator)
                    .HasColumnName("resultmin")
                    .HasColumnType("double precision");

                // Связь с File (один-к-одному)
                entity.HasOne<File>()
                    .WithOne()
                    .HasForeignKey<Result>(r => r.FileID)
                    .HasConstraintName("fk_results_files")
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }


    }
}
