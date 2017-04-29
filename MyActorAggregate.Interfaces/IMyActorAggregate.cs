using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyActorAggregate.Interfaces
{
	public interface IMyActorAggregate : IService
	{
		Task<ActorId> Create();

		Task<ActorId[]> GetAll();

		Task<bool> Contains(ActorId actorId);
	}
}
