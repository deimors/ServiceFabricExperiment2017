﻿using Common;
using CoreWeb.Factories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using MyActor.Interfaces;
using MyActorAggregate.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreWeb.Controllers
{
	[Route("api/[controller]")]
	public class ActorsController : Controller
	{
		private readonly IMyActorAggregateFactory _aggregateFactory;
		private readonly IMyActorFactory _actorFactory;

		public ActorsController(IMyActorAggregateFactory aggregateFactory, IMyActorFactory actorFactory)
		{
			_aggregateFactory = aggregateFactory;
			_actorFactory = actorFactory;
		}

		[HttpGet]
		public async Task<IActionResult> Get()
			=> SuccessResult((await GetActorAggregate().GetAll()).Select(actorId => actorId.GetGuidId()));

		[HttpGet("{id}")]
		public Task<IActionResult> Get(string id)
			=> WithActor(id).MatchAsync(actor => GetActorCount(actor));

		private async Task<IActionResult> GetActorCount(IMyActor actor)
			=> SuccessResult(await actor.GetCountAsync(CancellationToken.None));

		[HttpPost]
		public async Task<IActionResult> Post([FromBody]int value)
		{
			var newActorId = await GetActorAggregate().Create();
			
			await GetActor(newActorId).SetCountAsync(value, CancellationToken.None);

			return SuccessResult(newActorId.GetGuidId());
		}

		[HttpPut("{id}")]
		public Task<IActionResult> Put(string id, [FromBody]int value)
			=> WithActor(id).MatchAsync(actor => SetActorCount(actor, value));

		private async Task<IActionResult> SetActorCount(IMyActor actor, int value)
			=> SuccessResult(await actor.SetCountAsync(value, CancellationToken.None));

		[HttpDelete("{id}")]
		public Task<IActionResult> Delete(string id)
			=> ParseActorId(id).MatchAsync(actorId => DeleteActor(actorId));

		private async Task<IActionResult> DeleteActor(ActorId actorId)
			=> (await GetActorAggregate().Delete(actorId))
				? SuccessResult()
				: UnknownActorResult(actorId);

		private Result<ActorId, IActionResult> ParseActorId(string id)
			=> Guid.TryParse(id, out Guid guidId)
				? Result<ActorId, IActionResult>.Succeed(new ActorId(guidId))
				: Result<ActorId, IActionResult>.Fail(InvalidActorIdResult(id));

		private async Task<Result<IMyActor, IActionResult>> WithActor(ActorId actorId)
			=> (await GetActorAggregate().Contains(actorId))
				? Result<IMyActor, IActionResult>.Succeed(GetActor(actorId))
				: Result<IMyActor, IActionResult>.Fail(UnknownActorResult(actorId));

		private Task<Result<IMyActor, IActionResult>> WithActor(string id)
			=> ParseActorId(id).Match(
				actorId => WithActor(actorId),
				error => Task.FromResult(Result<IMyActor, IActionResult>.Fail(error))
			);

		private IActionResult SuccessResult()
			=> new OkResult();

		private IActionResult SuccessResult(object value)
			=> new OkObjectResult(value);

		private IActionResult InvalidActorIdResult(string id)
			=> new BadRequestObjectResult($"Invalid Actor Id {id}");

		private IActionResult UnknownActorResult(ActorId actorId)
			=> new NotFoundObjectResult($"Unknown Actor {actorId}");

		private IMyActorAggregate GetActorAggregate()
			=> _aggregateFactory.Create();

		private IMyActor GetActor(ActorId actorId)
			=> _actorFactory.Create(actorId);
	}
}
