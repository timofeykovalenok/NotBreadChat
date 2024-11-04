using Context;
using FluentMigrator.Runner;
using LinqToDB;
using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;
using LinqToDB.Mapping;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.DAL
{
    public static class Configure
    {
        public static IServiceCollection AddDAL(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default")!;

            var mappingSchema = MappingSchema.Default
                .SetConverter<DateTime, DateTime>(x => DateTime.SpecifyKind(x, DateTimeKind.Utc));

            services.AddLinqToDBContext<PostgresDb>((provider, options) =>
                options
                    .UsePostgreSQL(connectionString)
                    .UseMappingSchema(mappingSchema)
                    .UseDefaultLogging(provider)
                    .UseGenerateExpressionTest(true));

            var provider = services
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddPostgres()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(Configure).Assembly).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                .BuildServiceProvider(false);

            var runner = provider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();

            return services;
        }
    }
}
