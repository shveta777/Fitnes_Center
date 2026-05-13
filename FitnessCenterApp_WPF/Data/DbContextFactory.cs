using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FitnessCenterApp.Data;


public class FitnessCenterDbContextFactory : IDesignTimeDbContextFactory<FitnessCenterDbContext>
{
    public FitnessCenterDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FitnessCenterDbContext>();
        optionsBuilder.UseSqlServer(Utils.Constants.ConnectionString);
        return new FitnessCenterDbContext(optionsBuilder.Options);
    }
}
