using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Job.BlockchainOperationsExecutor.AppServices.Lifecycle;
using Lykke.Common.Log;
using Lykke.Job.BlockchainOperationsExecutor.Modules;
using Lykke.Job.BlockchainOperationsExecutor.Settings;
using Lykke.Logs;
using Lykke.Logs.Loggers.LykkeSlack;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Job.BlockchainOperationsExecutor
{
    [UsedImplicitly]
    public class Startup
    {
        private IHealthNotifier _healthNotifier;

        private IContainer ApplicationContainer { get; set; }
        private IConfigurationRoot Configuration { get; }
        private ILog Log { get; set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddMvc()
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.ContractResolver =
                            new Newtonsoft.Json.Serialization.DefaultContractResolver();
                    });

                services.AddSwaggerGen(options =>
                {
                    options.DefaultLykkeConfiguration("v1", "BlockchainOperationsExecutor API");

                });

                var builder = new ContainerBuilder();
                var appSettings = Configuration.LoadSettings<AppSettings>(o =>
                    {
                        o.SetConnString(s => s.SlackNotifications.AzureQueue.ConnectionString);
                        o.SetQueueName(s => s.SlackNotifications.AzureQueue.QueueName);
                        o.SenderName = $"{AppEnvironment.Name} {AppEnvironment.Version}";
                    });
                var settings = appSettings.CurrentValue;

                services.AddLykkeLogging(
                    appSettings.ConnectionString(s => s.BlockchainOperationsExecutorJob.Db.LogsConnString),
                    "BlockchainOperationsExecutorLog",
                    settings.SlackNotifications.AzureQueue.ConnectionString,
                    settings.SlackNotifications.AzureQueue.QueueName,
                    options =>
                    {
                        options.AddAdditionalSlackChannel("CommonBlockChainIntegration", o =>
                            {
                                o.MinLogLevel = Microsoft.Extensions.Logging.LogLevel.Information;
                                o.SpamGuard.DisableGuarding();
                            });
                        options.AddAdditionalSlackChannel("CommonBlockChainIntegrationImportantMessages", o =>
                        {
                            o.MinLogLevel = Microsoft.Extensions.Logging.LogLevel.Warning;
                            o.SpamGuard.DisableGuarding();
                        });
                    });

                builder.Populate(services);

                builder.RegisterModule(new JobModule(
                    settings.Assets,
                    settings.BlockchainOperationsExecutorJob.ChaosKitty));
                builder.RegisterModule(
                    new RepositoriesModule(appSettings.Nested(x => x.BlockchainOperationsExecutorJob.Db)));
                builder.RegisterModule(new BlockchainsModule(
                    settings.BlockchainOperationsExecutorJob,
                    settings.BlockchainsIntegration,
                    settings.BlockchainSignFacadeClient));
                builder.RegisterModule(
                    new CqrsModule(settings.BlockchainOperationsExecutorJob.Cqrs));

                ApplicationContainer = builder.Build();

                var logFactory = ApplicationContainer.Resolve<ILogFactory>();
                Log = logFactory.CreateLog(this);
                _healthNotifier = ApplicationContainer.Resolve<IHealthNotifier>();

                return new AutofacServiceProvider(ApplicationContainer);
            }
            catch (Exception ex)
            {
                Log?.Critical(ex);
                throw;
            }
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            try
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseLykkeMiddleware(ex => ErrorResponse.Create("Technical problem"));

                app.UseMvc();
                app.UseSwagger(c =>
                {
                    c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);
                });
                app.UseSwaggerUI(x =>
                {
                    x.RoutePrefix = "swagger/ui";
                    x.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                });
                app.UseStaticFiles();

                appLifetime.ApplicationStarted.Register(() => StartApplication().GetAwaiter().GetResult());
                appLifetime.ApplicationStopping.Register(() => StopApplication().GetAwaiter().GetResult());
                appLifetime.ApplicationStopped.Register(CleanUp);
            }
            catch (Exception ex)
            {
                Log?.Critical(ex);
                throw;
            }
        }

        private async Task StartApplication()
        {
            try
            {
                // NOTE: Job not yet recieve and process IsAlive requests here

                await ApplicationContainer.Resolve<IStartupManager>().StartAsync();
                _healthNotifier?.Notify("Started", Program.EnvInfo);
            }
            catch (Exception ex)
            {
                Log.Critical(ex);
                throw;
            }
        }

        private async Task StopApplication()
        {
            try
            {
                // NOTE: Job still can recieve and process IsAlive requests here, so take care about it if you add logic here.

                await ApplicationContainer.Resolve<IShutdownManager>().StopAsync();
            }
            catch (Exception ex)
            {
                Log?.Critical(ex);
                throw;
            }
        }

        private void CleanUp()
        {
            try
            {
                // NOTE: Job can't recieve and process IsAlive requests here, so you can destroy all resources
                _healthNotifier?.Notify("Terminating", Program.EnvInfo);
                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    Log.Critical(ex);
                    (Log as IDisposable)?.Dispose();
                }
                throw;
            }
        }
    }
}
