using Microsoft.ServiceFabric.Actors;
using MyActor.Interfaces;

namespace CoreWeb.Factories
{
	public interface IMyActorFactory
	{
		IMyActor Create(ActorId actorId);
	}
}