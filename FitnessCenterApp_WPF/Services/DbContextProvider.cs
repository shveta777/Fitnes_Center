using FitnessCenterApp.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Services;

public static class DbContextProvider
{
    public static FitnessCenterDbContext Create()
    {
        var options = new DbContextOptionsBuilder<FitnessCenterDbContext>()
            .UseSqlServer(Utils.Constants.ConnectionString)
            .Options;

        return new FitnessCenterDbContext(options);
    }
}
