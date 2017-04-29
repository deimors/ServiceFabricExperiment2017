using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using MyActorAggregate.Interfaces;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace MyActorAggregate
{
	/// <summary>
	/// An instance of this class is created for each service replica by the Service Fabric runtime.
	/// </summary>
	internal sealed class MyActorAggregate : StatefulService, IMyActorAggregate
	{
		public MyActorAggregate(StatefulServiceContext context)
			: base(context)
		{ }

		public async Task<ActorId> Create()
		{
			var actors = await GetActors();

			while (true)
			{
				var newId = new ActorId(Guid.NewGuid());

				using (var tx = StateManager.CreateTransaction())
				{
					if (!(await actors.ContainsKeyAsync(tx, newId)))
					{
						await actors.AddAsync(tx, newId, newId);

						await tx.CommitAsync();

						return newId;
					}
				}
			}
		}
		
		public async Task<ActorId[]> GetAll()
		{
			var actors = await GetActors();
			
			using (var tx = StateManager.CreateTransaction())
			{
				var result = new ActorId[await actors.GetCountAsync(tx)];

				var actorsEnumerable = await actors.CreateEnumerableAsync(tx);

				using (var actorsEnumerator = actorsEnumerable.GetAsyncEnumerator())
				{
					for (int i = 0;  await actorsEnumerator.MoveNextAsync(CancellationToken.None); i++)
					{
						result[i] = actorsEnumerator.Current.Key;
					}
				}

				await tx.CommitAsync();

				return result;
			}
		}

		public async Task<bool> Contains(ActorId actorId)
		{
			var actors = await GetActors();

			using (var tx = StateManager.CreateTransaction())
			{
				var contains = await actors.ContainsKeyAsync(tx, actorId);

				await tx.CommitAsync();

				return contains;
			}
		}

		public async Task<bool> Delete(ActorId actorId)
		{
			var actors = await GetActors();

			using (var tx = StateManager.CreateTransaction())
			{
				var removed = await actors.TryRemoveAsync(tx, actorId);

				await tx.CommitAsync();

				return removed.HasValue;
			}
		}


		private async Task<IReliableDictionary<ActorId, ActorId>> GetActors()
		{
			return await StateManager.GetOrAddAsync<IReliableDictionary<ActorId, ActorId>>("actors");
		}

		/// <summary>
		/// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
		/// </summary>
		/// <remarks>
		/// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
		/// </remarks>
		/// <returns>A collection of listeners.</returns>
		protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
			=> new[]
			{
				new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context))
			};
	}
}
