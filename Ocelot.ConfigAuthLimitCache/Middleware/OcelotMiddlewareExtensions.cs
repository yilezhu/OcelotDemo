using Microsoft.AspNetCore.Builder;
using Ocelot.Configuration;
using Ocelot.Configuration.Repository;
using Ocelot.Middleware;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.Configuration.Creator;
using Ocelot.DependencyInjection;
using Ocelot.Responses;
using Ocelot.Configuration.Setter;
using Ocelot.Middleware.Pipeline;
using Microsoft.AspNetCore.Hosting;
using Ocelot.Logging;
using System.Diagnostics;
using Ocelot.Configuration.File;
using Microsoft.Extensions.Options;

namespace Ocelot.ConfigAuthLimitCache.Middleware
{
    /// <summary>
    /// yilezhu
    /// 2018.10.22
    /// 扩展IApplicationBuilder，新增use方法
    /// </summary>
    public static class OcelotMiddlewareExtensions
    {
        /// <summary>
        /// 扩展UseOcelot
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static async Task<IApplicationBuilder> UseAhphOcelot(this IApplicationBuilder builder)
        {
            await builder.UseAhphOcelot(new OcelotPipelineConfiguration());
            return builder;
        }

        /// <summary>
        /// 重写Ocelot,带参数
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="pipelineConfiguration"></param>
        /// <returns></returns>
        public static async Task<IApplicationBuilder> UseAhphOcelot(this IApplicationBuilder builder, OcelotPipelineConfiguration pipelineConfiguration)
        {
            var configuration = await CreateConfiguration(builder);

            ConfigureDiagnosticListener(builder);

            return CreateOcelotPipeline(builder, pipelineConfiguration);
        }

        private static async Task<IInternalConfiguration> CreateConfiguration(IApplicationBuilder builder)
        {
            // make configuration from file system?
            // earlier user needed to add ocelot files in startup configuration stuff, asp.net will map it to this
            //var fileConfig = builder.ApplicationServices.GetService<IOptionsMonitor<FileConfiguration>>();
            var fileConfig = await builder.ApplicationServices.GetService<IFileConfigurationRepository>().Get();
            // now create the config
            var internalConfigCreator = builder.ApplicationServices.GetService<IInternalConfigurationCreator>();
            var internalConfig = await internalConfigCreator.Create(fileConfig.Data);
            //Configuration error, throw error message
            if (internalConfig.IsError)
            {
                ThrowToStopOcelotStarting(internalConfig);
            }

            // now save it in memory
            var internalConfigRepo = builder.ApplicationServices.GetService<IInternalConfigurationRepository>();
            internalConfigRepo.AddOrReplace(internalConfig.Data);

            //fileConfig.OnChange(async (config) =>
            //{
            //    var newInternalConfig = await internalConfigCreator.Create(config);
            //    internalConfigRepo.AddOrReplace(newInternalConfig.Data);
            //});

            var adminPath = builder.ApplicationServices.GetService<IAdministrationPath>();

            var configurations = builder.ApplicationServices.GetServices<OcelotMiddlewareConfigurationDelegate>();

            // Todo - this has just been added for consul so far...will there be an ordering problem in the future? Should refactor all config into this pattern?
            foreach (var configuration in configurations)
            {
                await configuration(builder);
            }

            if (AdministrationApiInUse(adminPath))
            {
                //We have to make sure the file config is set for the ocelot.env.json and ocelot.json so that if we pull it from the 
                //admin api it works...boy this is getting a spit spags boll.
                var fileConfigSetter = builder.ApplicationServices.GetService<IFileConfigurationSetter>();

                //  await SetFileConfig(fileConfigSetter, fileConfig.Data);
            }

            return GetOcelotConfigAndReturn(internalConfigRepo);
        }

        private static bool AdministrationApiInUse(IAdministrationPath adminPath)
        {
            return adminPath != null;
        }

        //private static async Task SetFileConfig(IFileConfigurationSetter fileConfigSetter, IOptionsMonitor<FileConfiguration> fileConfig)
        //{
        //    var response = await fileConfigSetter.Set(fileConfig.CurrentValue);

        //    if (IsError(response))
        //    {
        //        ThrowToStopOcelotStarting(response);
        //    }
        //}

        private static bool IsError(Response response)
        {
            return response == null || response.IsError;
        }

        private static IInternalConfiguration GetOcelotConfigAndReturn(IInternalConfigurationRepository provider)
        {
            var ocelotConfiguration = provider.Get();

            if (ocelotConfiguration?.Data == null || ocelotConfiguration.IsError)
            {
                ThrowToStopOcelotStarting(ocelotConfiguration);
            }

            return ocelotConfiguration.Data;
        }

        private static void ThrowToStopOcelotStarting(Response config)
        {
            throw new Exception($"Unable to start Ocelot, errors are: {string.Join(",", config.Errors.Select(x => x.ToString()))}");
        }

        private static IApplicationBuilder CreateOcelotPipeline(IApplicationBuilder builder, OcelotPipelineConfiguration pipelineConfiguration)
        {
            var pipelineBuilder = new OcelotPipelineBuilder(builder.ApplicationServices);

            //重写自定义管道
            pipelineBuilder.BuildAhphOcelotPipeline(pipelineConfiguration);

            var firstDelegate = pipelineBuilder.Build();

            /*
            inject first delegate into first piece of asp.net middleware..maybe not like this
            then because we are updating the http context in ocelot it comes out correct for
            rest of asp.net..
            */

            builder.Properties["analysis.NextMiddlewareName"] = "TransitionToOcelotMiddleware";

            builder.Use(async (context, task) =>
            {
                var downstreamContext = new DownstreamContext(context);
                await firstDelegate.Invoke(downstreamContext);
            });

            return builder;
        }

        private static void ConfigureDiagnosticListener(IApplicationBuilder builder)
        {
            var env = builder.ApplicationServices.GetService<IHostingEnvironment>();
            var listener = builder.ApplicationServices.GetService<OcelotDiagnosticListener>();
            var diagnosticListener = builder.ApplicationServices.GetService<DiagnosticListener>();
            diagnosticListener.SubscribeWithAdapter(listener);
        }
    }
}
