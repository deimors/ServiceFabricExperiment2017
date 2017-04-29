using Common.ServiceFabric;
using CoreWeb.Config;
using Microsoft.Extensions.Options;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using MyActorAggregate.Interfaces;
using System.Fabric;

namespace CoreWeb.Factories
{
	public class MyActorAggregateFactory : IMyActorAggregateFactory
	{
		private readonly MyActorAggregateFactoryConfig _config;
		private readonly ICodePackageActivationContext _context;

		public MyActorAggregateFactory(IOptions<MyActorAggregateFactoryConfig> configOptions, ICodePackageActivationContext context)
		{
			_config = configOptions.Value;
			_context = context;
		}
		
		public IMyActorAggregate Create()
			=> ServiceProxy.Create<IMyActorAggregate>(_context.BuildServiceUri(_config.ServiceName), new ServicePartitionKey(0));
	}
}
