using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BankWebApp.Tests
{
    /// <summary>
    /// Test-only: SQLite can't sort or sum decimal columns, so in the test databases money is
    /// stored as double. The app's own model and SQL Server are unchanged.
    /// </summary>
    public class SqliteDecimalModelCustomizer : ModelCustomizer
    {
        public SqliteDecimalModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies) { }

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);
            foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetProperties()))
            {
                if (property.ClrType == typeof(decimal)) property.SetProviderClrType(typeof(double));
                if (property.ClrType == typeof(decimal?)) property.SetProviderClrType(typeof(double?));
            }
        }
    }

    public static class SqliteTestOptions
    {
        public static DbContextOptionsBuilder UseTestSqlite(this DbContextOptionsBuilder options, System.Data.Common.DbConnection connection) =>
            options.UseSqlite(connection).ReplaceService<IModelCustomizer, SqliteDecimalModelCustomizer>();
    }
}
