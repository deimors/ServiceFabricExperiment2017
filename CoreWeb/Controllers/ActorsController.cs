using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors.Client;
using MyActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using System.Threading;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using MyActorAggregate.Interfaces;
using Microsoft.ServiceFabric.Services.Client;
using Common;

namespace CoreWeb.Controllers
{
	[Route("api/[controller]")]
	public class ActorsController : Controller
	{
		// GET api/actors
		[HttpGet]
		public async Task<IEnumerable<Guid>> Get()
			=> (await GetActorAggregate().GetAll()).Select(actorId => actorId.GetGuidId());


		// GET api/actors/5
		[HttpGet("{id}")]
		public Task<IActionResult> Get(string id)
			=> WithActor(id).MatchAsync(
				async actor => new ObjectResult(await actor.GetCountAsync(CancellationToken.None)) as IActionResult
			);

		// POST api/actors
		[HttpPost]
		public async Task<Guid> Post([FromBody]int value)
		{
			var actorAggregate = GetActorAggregate();

			var newActorId = await actorAggregate.Create();

			var actor = GetActor(newActorId);

			await actor.SetCountAsync(value, CancellationToken.None);

			return newActorId.GetGuidId();
		}

		// PUT api/actors/5
		[HttpPut("{id}")]
		public Task<IActionResult> Put(string id, [FromBody]int value)
			=> WithActor(id).MatchAsync(
				async actor =>
				{
					await actor.SetCountAsync(value, CancellationToken.None);

					return new OkObjectResult(value) as IActionResult;
				}
			);

		// DELETE api/actors/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}
		
		private Result<ActorId, IActionResult> ParseActorId(string id)
			=> Guid.TryParse(id, out Guid guidId)
				? Result<ActorId, IActionResult>.Succeed(new ActorId(guidId))
				: Result<ActorId, IActionResult>.Fail(new BadRequestObjectResult($"Invalid Actor Id {id}"));

		private async Task<Result<IMyActor, IActionResult>> WithActor(ActorId actorId)
			=> (await GetActorAggregate().Contains(actorId))
				? Result<IMyActor, IActionResult>.Succeed(GetActor(actorId))
				: Result<IMyActor, IActionResult>.Fail(new NotFoundObjectResult($"Unknown Actor {actorId}"));

		private Task<Result<IMyActor, IActionResult>> WithActor(string id)
			=> ParseActorId(id).Match(
				actorId => WithActor(actorId), 
				error => Task.FromResult(Result<IMyActor, IActionResult>.Fail(error))
			);

		private static IMyActorAggregate GetActorAggregate()
			=> ServiceProxy.Create<IMyActorAggregate>(new Uri("fabric:/ServiceFabricExperiment2017/MyActorAggregate"), new ServicePartitionKey(0));

		private IMyActor GetActor(ActorId actorId)
			=> ActorProxy.Create<IMyActor>(actorId, new Uri("fabric:/ServiceFabricExperiment2017/MyActorService"));
	}
}
