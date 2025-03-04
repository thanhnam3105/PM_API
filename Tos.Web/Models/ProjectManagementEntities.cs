using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using TOS.Web.Models.CodeFirst;

namespace TOS.Web.Models
{
    public class ProjectManagementEntities : DbContext
    {
        private string defautSchema { get; set; }
        public DbSet<AspNetRoles> AspNetRoles { get; set; }
        public DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
        public DbSet<AspNetUsers> AspNetUsers { get; set; }
        public DbSet<ma_customer> ma_customer { get; set; }
        public DbSet<ma_employee> ma_employee { get; set; }
        public DbSet<vw_api_log> vw_api_log { get; set; }
        public DbSet<vw_CommonMaster_Header> vw_CommonMaster_Header { get; set; }
        public DbSet<ma_common> ma_common { get; set; }
        public DbSet<ma_project> ma_project { get; set; }
        public DbSet<ma_token_key> ma_token_key { get; set; }
        public DbSet<tr_api_data> tr_api_data { get; set; }
        public DbSet<tr_api_log> tr_api_log { get; set; }
        public DbSet<ma_employee_rdm> ma_employee_rdm { get; set; }
        public DbSet<tr_project_contract> tr_project_contract { get; set; }
        public DbSet<tr_project_cost> tr_project_cost { get; set; }
        public DbSet<tr_bug_rdm> tr_bug_rdm { get; set; }
        public DbSet<tr_project_rdm> tr_project_rdm { get; set; }
        public DbSet<ma_project_rdm> ma_project_rdm { get; set; }
        public DbSet<tr_project_member> tr_project_member { get; set; }
        public DbSet<tr_file> tr_file { get; set; }

        public ProjectManagementEntities() : base(nameOrConnectionString: "ProjectManagementEntities")
        {
            defautSchema = "dbo";
        }
        public ProjectManagementEntities(string nameOrConnectionString, string schema) : base(nameOrConnectionString: nameOrConnectionString)
        {
            defautSchema = schema;
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(defautSchema);
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AspNetRoles>().ToTable("AspNetRoles", "dbo");
            modelBuilder.Entity<AspNetUserClaims>().ToTable("AspNetUserClaims", "dbo");
            modelBuilder.Entity<AspNetUserLogins>().ToTable("AspNetUserLogins", "dbo");
            modelBuilder.Entity<AspNetUsers>().ToTable("AspNetUsers", "dbo");
            modelBuilder.Entity<AspNetRoles>()
            .HasKey(e => new { e.Id });

            modelBuilder.Entity<AspNetUserClaims>()
            .HasKey(e => new { e.Id });

            modelBuilder.Entity<AspNetUserLogins>()
            .HasKey(e => new { e.LoginProvider, e.ProviderKey, e.UserId });

            modelBuilder.Entity<AspNetUserRoles>()
             .HasKey(e => new { e.UserId, e.RoleId });

            modelBuilder.Entity<AspNetUsers>()
            .HasKey(e => new { e.Id });

            modelBuilder.Entity<ma_token_key>().ToTable("ma_token_key", "dbo");
            modelBuilder.Entity<ma_token_key>().HasKey(e => new { e.cd_employee });

            modelBuilder.Entity<ma_customer>().ToTable("ma_customer", "dbo")
            .HasKey(e => new { e.cd_customer })
            .Property(p => p.ts).IsConcurrencyToken();
            modelBuilder.Entity<ma_customer>().Property(p => p.ts).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ma_employee>().ToTable("ma_employee", "dbo")
           .HasKey(e => new { e.cd_employee });

            modelBuilder.Entity<vw_api_log>().ToTable("vw_api_log", "dbo")
           .HasKey(e => new { e.no_seq });

            modelBuilder.Entity<vw_CommonMaster_Header>().ToTable("vw_CommonMaster_Header", "dbo")
          .HasKey(e => new { e.cd_category,e.cd_common });

            modelBuilder.Entity<ma_common>().ToTable("ma_common", "dbo")
            .HasKey(e => new { e.cd_category, e.cd_common })
            .Property(p => p.ts).IsConcurrencyToken();
            modelBuilder.Entity<ma_common>().Property(p => p.ts).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<ma_project>().ToTable("ma_project", "dbo")
            .HasKey(e => new { e.cd_project })
            .Property(p => p.ts).IsConcurrencyToken();
            modelBuilder.Entity<ma_project>().Property(p => p.ts).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<tr_api_data>().ToTable("tr_api_data", "dbo")
            .HasKey(e => new { e.no_seq });

            modelBuilder.Entity<tr_api_log>().ToTable("tr_api_log", "dbo")
            .HasKey(e => new { e.no_seq });

            modelBuilder.Entity<ma_employee_rdm>().ToTable("ma_employee_rdm", "dbo")
            .HasKey(e => new { e.cd_redmine });

            modelBuilder.Entity<tr_project_contract>().ToTable("tr_project_contract", "dbo")
            .HasKey(e => new { e.cd_project })
            .Property(p => p.ts).IsConcurrencyToken();
            modelBuilder.Entity<tr_project_contract>().Property(p => p.ts).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<tr_project_cost>().ToTable("tr_project_cost", "dbo")
            .HasKey(e => new { e.cd_project, e.mm_cost })
            .Property(p => p.ts).IsConcurrencyToken();
            modelBuilder.Entity<tr_project_cost>().Property(p => p.ts).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);

            modelBuilder.Entity<tr_bug_rdm>().ToTable("tr_bug_rdm", "dbo")
            .HasKey(e => new { e.cd_identifier, e.cd_type });

            modelBuilder.Entity<tr_project_rdm>().ToTable("tr_project_rdm", "dbo")
            .HasKey(e => new { e.cd_identifier, e.mm_input });

            modelBuilder.Entity<ma_project_rdm>().ToTable("ma_project_rdm", "dbo")
            .HasKey(e => new { e.cd_project_rdm });

            modelBuilder.Entity<tr_project_member>().ToTable("tr_project_member", "dbo")
           .HasKey(e => new { e.no_seq });


            modelBuilder.Entity<tr_file>().ToTable("tr_file", "dbo")
           .HasKey(e => new { e.cd_project, e.no_seq });
        }
    }
}
