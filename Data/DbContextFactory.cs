using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class DbContextFactory : IDesignTimeDbContextFactory<FeedbackDbContext>
    {
        public FeedbackDbContext CreateDbContext(string[] args)
        {
            var dbContextBuilder = new DbContextOptionsBuilder<FeedbackDbContext>();
            var connectionString = "server=localhost;database=feedbacksys;uid=svlabs;password=Stars@5me$;port=3306; SSL Mode=None;";
            dbContextBuilder.UseMySQL(connectionString);

            return new FeedbackDbContext(dbContextBuilder.Options);
        }
    }
}
