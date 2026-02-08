using Core.Contracts.Repositories;
using Core.Contracts.Services;
using EfDataStorage;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services;

namespace UnitTests.AppServices
{
    [TestCategory("Integration")]
    [TestClass]
    public class BaseAppServiceTest
    {
        protected ServiceProvider _provider = null!;
        protected string _dir = null!;

        [TestInitialize]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            //init DB
            var connection = new SqliteConnection("DataSource=:memory:");
            // Keep connection alive for whole test
            connection.Open();
            services.AddDbContext<AppDbContext>(o =>
                o.UseSqlite(connection));

            // repositories
            services.AddScoped<IPatientsRepository, PatientsRepository>();
            services.AddScoped<ISettingsRepository, SettingsRepository>();

            // services (see note below)
            services.AddScoped<IPatientsService, PatientsService>();
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<ISyncService, SyncService>();

            
            _provider = services.BuildServiceProvider();

            // initialize DB using SAME provider
            using var scope = _provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            _dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_dir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _provider.Dispose();
            Directory.Delete(_dir, true);
        }


    }
}
