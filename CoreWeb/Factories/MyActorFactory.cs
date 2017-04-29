using Common.ServiceFabric;
using CoreWeb.Config;
using Microsoft.Extensions.Options;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using MyActor.Interfaces;
using System.Fabric;

namespace CoreWeb.Factories
{
	public class MyActorFactory : IMyActorFactory
	{
		private readonly MyActorFactoryConfig _config;
		private readonly ICodePackageActivationContext _context;

		public MyActorFactory(IOptions<MyActorFactoryConfig> configOptions, ICodePackageActivationContext context)
		{
			_config = configOptions.Value;
			_context = context;
		}

		public IMyActor Create(ActorId actorId)
			=> ActorProxy.Create<IMyActor>(actorId, _context.BuildServiceUri(_config.ServiceName));
	}
}
