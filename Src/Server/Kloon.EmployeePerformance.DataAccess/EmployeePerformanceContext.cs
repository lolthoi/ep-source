using Kloon.EmployeePerformance.DataAccess.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess
{
    public class EmployeePerformanceContext : DbContext
    {
        public EmployeePerformanceContext(DbContextOptions options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region ConfigBuilder

            modelBuilder.Entity<Position>().ToTable("Position");
            modelBuilder.Entity<Position>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(e => e.Name)
                    .IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRequired(true)
                    .HasColumnType("timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.FirstName).IsRequired();
                entity.Property(x => x.LastName).IsRequired();
                entity.Property(x => x.Email).IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRequired(true)
                    .HasColumnType("timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(x => x.Position).WithMany(x => x.Users).HasForeignKey(x => x.PositionId).HasConstraintName("FK_User_Position");
            });

            modelBuilder.Entity<ProjectUser>().ToTable("ProjectUser");
            modelBuilder.Entity<ProjectUser>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(e => e.Id)
                    .IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRequired(true)
                    .HasColumnType("timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(x => x.User).WithMany(x => x.ProjectUsers).HasForeignKey(x => x.UserId).HasConstraintName("FK_ProjectUser_User");
                entity.HasOne(x => x.Project).WithMany(x => x.ProjectUsers).HasForeignKey(x => x.ProjectId).HasConstraintName("FK_ProjectUser_Project");
            });

            modelBuilder.Entity<Project>().ToTable("Project");
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(e => e.Name)
                    .IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRequired(true)
                    .HasColumnType("timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();
            });


            modelBuilder.Entity<CriteriaType>().ToTable("CriteriaType");
            modelBuilder.Entity<CriteriaType>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(e => e.Name)
                    .IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRequired(true)
                    .HasColumnType("timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();
                entity.HasOne(x => x.QuarterCriteriaTemplate).WithMany(x => x.CriteriaTypes).HasForeignKey(x => x.QuarterCriteriaTemplateId)
                    .HasConstraintName("FK_CriteriaType_QuarterCriteriaTemplate");
            });

            modelBuilder.Entity<Criteria>().ToTable("Criteria");
            modelBuilder.Entity<Criteria>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(e => e.Name)
                    .IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRequired(true)
                    .HasColumnType("timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(x => x.CriteriaType).WithMany(x => x.Criterias).HasForeignKey(x => x.CriteriaTypeId).HasConstraintName("FK_Criteria_CriteriaType");
            });

            modelBuilder.Entity<CriteriaQuarterEvaluation>().ToTable("CriteriaQuarterEvaluation");
            modelBuilder.Entity<CriteriaQuarterEvaluation>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(e => e.Id)
                    .IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRequired(true)
                    .HasColumnType("timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(x => x.Criteria).WithMany(x => x.CriteriaQuarterEvaluations).HasForeignKey(x => x.CriteriaId).HasConstraintName("FK_CriteriaQuarterEvaluation_Criteria");
                entity.HasOne(x => x.QuarterEvaluation).WithMany(x => x.CriteriaQuarterEvaluations).HasForeignKey(x => x.QuarterEvaluationId).HasConstraintName("FK_CriteriaQuarterEvaluation_QuarterEvaluation");
            });

            modelBuilder.Entity<CriteriaTypeQuarterEvaluation>().ToTable("CriteriaTypeQuarterEvaluation");
            modelBuilder.Entity<CriteriaTypeQuarterEvaluation>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(e => e.Id)
                    .IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRequired(true)
                    .HasColumnType("timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(x => x.CriteriaType).WithMany(x => x.CriteriaTypeQuarterEvaluations).HasForeignKey(x => x.CriteriaTypeId).HasConstraintName("FK_CriteriaTypeQuarterEvaluation_CriteriaType");
                entity.HasOne(x => x.QuarterEvaluation).WithMany(x => x.CriteriaTypeQuarterEvaluations).HasForeignKey(x => x.QuarterEvaluationId).HasConstraintName("FK_CriteriaTypeQuarterEvaluation_QuarterEvaluation");
            });

            modelBuilder.Entity<QuarterEvaluation>().ToTable("QuarterEvaluation");
            modelBuilder.Entity<QuarterEvaluation>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(e => e.Id)
                    .IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRequired(true)
                    .HasColumnType("timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();

            });

            modelBuilder.Entity<UserQuarterEvaluation>().ToTable("UserQuarterEvaluation");
            modelBuilder.Entity<UserQuarterEvaluation>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(e => e.Id)
                    .IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRequired(true)
                    .HasColumnType("timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(y => y.QuarterEvaluation).WithOne(x => x.UserQuarterEvaluation).HasForeignKey<UserQuarterEvaluation>(y => y.QuarterEvaluationId);
            });

            modelBuilder.Entity<AppSetting>().ToTable("AppSetting");
            modelBuilder.Entity<AppSetting>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Id)
                .IsRequired();

                entity.Property(x => x.RowVersion)
                   .IsRequired(true)
                   .HasColumnType("timestamp")
                   .IsConcurrencyToken()
                   .ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<CriteriaTypeStore>().ToTable("CriteriaTypeStore");
            modelBuilder.Entity<CriteriaTypeStore>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRequired(true)
                    .HasColumnType("timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();

            });

            modelBuilder.Entity<CriteriaStore>().ToTable("CriteriaStore");
            modelBuilder.Entity<CriteriaStore>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(e => e.Name)
                    .IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRequired(true)
                    .HasColumnType("timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(x => x.CriteriaTypeStore).WithMany(x => x.CriteriaStores).HasForeignKey(x => x.CriteriaTypeId).HasConstraintName("FK_CriteriaStore_CriteriaTypeStore");
            });

            modelBuilder.Entity<EvaluationTemplate>().ToTable("EvaluationTemplate");
            modelBuilder.Entity<EvaluationTemplate>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired();
                entity.Property(x => x.PositionId).IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRequired(true)
                    .HasColumnType("timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();

            });

            modelBuilder.Entity<CriteriaTypeTemplate>().ToTable("CriteriaTypeTemplate");
            modelBuilder.Entity<CriteriaTypeTemplate>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.CriteriaTypeStoreId).IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRequired(true)
                    .HasColumnType("timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();
                entity.HasOne(x => x.EvaluationTemplate).WithMany(x => x.CriteriaTypeTemplates).HasForeignKey(x => x.EvaluationTemplateId).HasConstraintName("FK_CriteriaTypeTemplate_EvaluationTemplate");
            });

            modelBuilder.Entity<CriteriaTemplate>().ToTable("CriteriaTemplate");
            modelBuilder.Entity<CriteriaTemplate>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.EvaluationTemplateId).IsRequired();
                entity.Property(x => x.CriteriaStoreId).IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRequired(true)
                    .HasColumnType("timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();
                entity.HasOne(x => x.CriteriaTypeTemplate).WithMany(x => x.CriteriaTemplates).HasForeignKey(x => x.CriteriaTypeTemplateId).HasConstraintName("FK_CriteriaTemplate_CriteriaTypeTemplate");
            });

            modelBuilder.Entity<QuarterCriteriaTemplate>().ToTable("QuarterCriteriaTemplate");
            modelBuilder.Entity<QuarterCriteriaTemplate>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired();
                entity.Property(x => x.PositionId).IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRequired(true)
                    .HasColumnType("timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<ActivityGroupUser>().ToTable("ActivityGroupUser");
            modelBuilder.Entity<ActivityGroupUser>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.TSActivityGroupId).IsRequired();
                entity.Property(x => x.UserId).IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRequired(true)
                    .HasColumnType("timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(t => t.TSActivityGroup).WithMany(t => t.ActivityGroupUsers).HasForeignKey(t => t.TSActivityGroupId).HasConstraintName("FK_ActivityGroup_ActivityGroupUser");
            });

            modelBuilder.Entity<TSActivityGroup>().ToTable("TSActivityGroup");
            modelBuilder.Entity<TSActivityGroup>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRequired(true)
                    .HasColumnType("timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();

            });

            modelBuilder.Entity<TSActivity>().ToTable("TSActivity");
            modelBuilder.Entity<TSActivity>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.TSActivityGroupId).IsRequired();
                entity.Property(x => x.Name).IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRequired(true)
                    .HasColumnType("timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(t => t.TSActivityGroup).WithMany(t => t.TSActivities).HasForeignKey(t => t.TSActivityGroupId).HasConstraintName("FK_TSActivityGroup_TSActivity");
            });

            modelBuilder.Entity<TSRecord>().ToTable("TSRecord");
            modelBuilder.Entity<TSRecord>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.TSActivityId).IsRequired();
                entity.Property(x => x.UserId).IsRequired();
                entity.Property(x => x.Name).IsRequired();

                entity.Property(x => x.RowVersion)
                    .IsRequired(true)
                    .HasColumnType("timestamp")
                    .IsConcurrencyToken()
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(t => t.TSActivity).WithMany(t => t.TSRecords).HasForeignKey(t => t.TSActivityId).HasConstraintName("FK_TSActivity_TSRecord");
            });

            #endregion
        }

        public virtual DbSet<Criteria> Criterias { get; set; }
        public virtual DbSet<CriteriaType> CriteriaTypes { get; set; }
        public virtual DbSet<CriteriaQuarterEvaluation> CriteriaQuarterEvaluations { get; set; }
        public virtual DbSet<CriteriaTypeQuarterEvaluation> CriteriaTypeQuarterEvaluations { get; set; }
        public virtual DbSet<Position> Positions { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<ProjectUser> ProjectUsers { get; set; }
        public virtual DbSet<QuarterEvaluation> QuarterEvaluations { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserQuarterEvaluation> UserQuarterEvaluations { get; set; }
        public virtual DbSet<AppSetting> AppSettings { get; set; }

        public virtual DbSet<CriteriaTypeStore> CriteriaTypeStores { get; set; }
        public virtual DbSet<CriteriaStore> CriteriaStores { get; set; }
        public virtual DbSet<EvaluationTemplate> EvaluationTemplates { get; set; }
        public virtual DbSet<CriteriaTypeTemplate> CriteriaTypeTemplates { get; set; }
        public virtual DbSet<CriteriaTemplate> CriteriaTemplates { get; set; }
        public virtual DbSet<QuarterCriteriaTemplate> QuarterCriteriaTemplates { get; set; }

        public virtual DbSet<ActivityGroupUser> ActivityGroupUser { get; set; }
        public virtual DbSet<TSActivityGroup> TSActivityGroup { get; set; }
        public virtual DbSet<TSActivity> TSActivity { get; set; }
        public virtual DbSet<TSRecord> TSRecord { get; set; }

    }
}
