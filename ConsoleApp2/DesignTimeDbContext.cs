using Microsoft.EntityFrameworkCore.Design;

namespace ConsoleApp2
{
    public class DesignTimeDbContext : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            return new ApplicationDbContext();
        }
    }
}
