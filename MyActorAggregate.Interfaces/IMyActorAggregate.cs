using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Threading.Tasks;

namespace MyActorAggregate.Interfaces
{
	public interface IMyActorAggregate : IService
	{
		Task<ActorId> Create();

		Task<ActorId[]> GetAll();

		Task<bool> Contains(ActorId actorId);

		Task<bool> Delete(ActorId actorId);
	}
}
