using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using MyActorAggregate.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;

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
		{
			//yield return new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context));
			//yield return new ServiceReplicaListener(context => new FabricTransportServiceRemotingListener(context, this));
			return new[]
			{
				new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context))
			};
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

		/// <summary>
		/// This is the main entry point for your service replica.
		/// This method executes when this replica of your service becomes primary and has write status.
		/// </summary>
		/// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
		//protected override async Task RunAsync(CancellationToken cancellationToken)
		//{
		// TODO: Replace the following sample code with your own logic 
		//       or remove this RunAsync override if it's not needed in your service.

		//var actors = await StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("actors");

		/*
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();

			using (var tx = StateManager.CreateTransaction())
			{
				var result = await actors.TryGetValueAsync(tx, "Counter");

				ServiceEventSource.Current.ServiceMessage(Context, "Current Counter Value: {0}",
					result.HasValue ? result.Value.ToString() : "Value does not exist.");

				await actors.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

				// If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
				// discarded, and nothing is saved to the secondary replicas.
				await tx.CommitAsync();
			}

			await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
		}
		*/
		//}
	}
}
