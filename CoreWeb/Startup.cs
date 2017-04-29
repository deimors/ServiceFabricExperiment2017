using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CoreWeb.Config;
using CoreWeb.Factories;
using System.Fabric;

namespace CoreWeb
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public IConfigurationRoot Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Add framework services.
			services.AddMvc();

			services.Configure<MyActorAggregateFactoryConfig>(options => Configuration.GetSection("MyActorAggregate").Bind(options));
			services.Configure<MyActorFactoryConfig>(options => Configuration.GetSection("MyActor").Bind(options));

			services.AddSingleton<ICodePackageActivationContext>(provider => FabricRuntime.GetActivationContext());

			services.AddSingleton<IMyActorAggregateFactory, MyActorAggregateFactory>();
			services.AddSingleton<IMyActorFactory, MyActorFactory>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();

			app.UseMvc();
		}
	}
}
