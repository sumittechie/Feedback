
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class FeedbackDbContext : IdentityDbContext<Users>
    {
        public FeedbackDbContext(DbContextOptions<FeedbackDbContext> options) : base(options)
        {
  
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            int stringMaxLength =255;
            builder.Entity<IdentityUserClaim<string>>(x => x.Property(m => m.UserId).HasMaxLength(stringMaxLength));
            builder.Entity<IdentityUserRole<string>>(x => x.Property(m => m.UserId).HasMaxLength(stringMaxLength));
            builder.Entity<IdentityUserRole<string>>(x => x.Property(m => m.RoleId).HasMaxLength(stringMaxLength));
            builder.Entity<IdentityUserLogin<string>>(x => x.Property(m => m.LoginProvider).HasMaxLength(stringMaxLength));
            builder.Entity<IdentityUserLogin<string>>(x => x.Property(m => m.ProviderKey).HasMaxLength(stringMaxLength));

            builder.Entity<IdentityUserToken<string>>(x => x.Property(m => m.UserId).HasMaxLength(stringMaxLength));
            builder.Entity<IdentityUserToken<string>>(x => x.Property(m => m.LoginProvider).HasMaxLength(stringMaxLength));
            builder.Entity<IdentityUserToken<string>>(x => x.Property(m => m.Name).HasMaxLength(stringMaxLength));

        }

        #region DBSets

        public DbSet<Tokens> Tokens { get; set; }

        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<FeedbackAssigned> FeedbackAssigned { get; set; }
        public DbSet<FeedbackReplys> FeedbackReplys { get; set; }

        #endregion
    }
}
