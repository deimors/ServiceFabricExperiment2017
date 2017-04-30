using CoreWeb.Controllers;
using CoreWeb.Factories;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using MyActor.Interfaces;
using MyActorAggregate.Interfaces;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoFakeItEasy;
using Ploeh.AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CoreWeb.Tests.Controllers
{
	public class ActorsControllerTests
	{
		public class BaseCustomization : ICustomization
		{
			public void Customize(IFixture fixture)
			{
				fixture.Customize(new AutoFakeItEasyCustomization());

				fixture.Register(() => new ActorsController(fixture.Create<IMyActorAggregateFactory>(), fixture.Create<IMyActorFactory>()));

				var aggregate = fixture.Freeze<IMyActorAggregate>();
				var aggregateFactory = fixture.Freeze<IMyActorAggregateFactory>();
				A.CallTo(() => aggregateFactory.Create()).Returns(aggregate);

				var actors = new Dictionary<ActorId, IMyActor>();
				fixture.Inject<IDictionary<ActorId, IMyActor>>(actors);

				var actorFactory = fixture.Freeze<IMyActorFactory>();

				fixture.Register(() => CreateMyActor(actors, actorFactory));

				A.CallTo(() => aggregate.Create()).ReturnsLazily(() => CreateMyActor(fixture, actors));
				A.CallTo(() => aggregate.GetAll()).ReturnsLazily(() => actors.Keys.ToArray());
				A.CallTo(() => aggregate.Contains(A<ActorId>._)).ReturnsLazily((ActorId id) => actors.ContainsKey(id));
				A.CallTo(() => aggregate.Delete(A<ActorId>._)).ReturnsLazily((ActorId id) => actors.Remove(id));
			}

			private ActorId CreateMyActor(IFixture fixture, IDictionary<ActorId, IMyActor> actors)
			{
				var actor = fixture.Create<IMyActor>();
				
				return actors.First(kvp => kvp.Value == actor).Key;
			}

			private IMyActor CreateMyActor(IDictionary<ActorId, IMyActor> actors, IMyActorFactory actorFactory)
			{
				var actorId = new ActorId(Guid.NewGuid());
				var actor = A.Fake<IMyActor>();
				
				actors[actorId] = actor;

				A.CallTo(() => actorFactory.Create(actorId)).Returns(actor);

				return actor;
			}
		}

		public class MyActorCustomization : ICustomization
		{
			private int _value;

			public MyActorCustomization(int value)
			{
				_value = value;
			}

			public void Customize(IFixture fixture)
			{
				var actor = fixture.Create<IMyActor>();

				A.CallTo(() => actor.SetCountAsync(A<int>._, A<CancellationToken>._))
					.ReturnsLazily((int value, CancellationToken t) => _value = value);

				A.CallTo(() => actor.GetCountAsync(A<CancellationToken>._))
					.Returns(_value);
			}
		}

		public class WhenEmpty
		{
			public class Arrange : AutoDataAttribute
			{
				public Arrange() : base(
					new Fixture()
						.Customize(new BaseCustomization())
				)
				{ }
			}

			[Theory, Arrange]
			public async Task PostReturnsOkObjectResult(ActorsController sut, int value)
			{
				var result = await sut.Post(value);

				result.Should().BeOfType<OkObjectResult>();
			}

			[Theory, Arrange]
			public async Task PostReturnsOkObjectResultWithGuid(ActorsController sut, int value)
			{
				var result = await sut.Post(value) as OkObjectResult;

				result.Value.Should().BeOfType<Guid>();
			}

			[Theory, Arrange]
			public async Task GetReturnsOkObjectResult(ActorsController sut)
			{
				var result = await sut.Get();

				result.Should().BeOfType<OkObjectResult>();
			}

			[Theory, Arrange]
			public async Task GetReturnsResultWithGuidSequence(ActorsController sut)
			{
				var result = (await sut.Get()) as OkObjectResult;

				result.Value.Should().BeAssignableTo<IEnumerable<Guid>>();
			}

			[Theory, Arrange]
			public async Task GetReturnsResultWithEmptyGuidSequence(ActorsController sut)
			{
				var result = (await sut.Get() as OkObjectResult).Value as IEnumerable<Guid>;

				result.Should().BeEmpty();
			}

			[Theory, Arrange]
			public async Task GetWithRandomStringIdReturnsBadRequestResult(ActorsController sut, string randomId)
			{
				var result = await sut.Get(randomId);

				result.Should().BeOfType<BadRequestObjectResult>();
			}

			[Theory, Arrange]
			public async Task GetWithRandomGuidIdReturnsNotFoundResult(ActorsController sut, Guid randomId)
			{
				var result = await sut.Get(randomId.ToString());

				result.Should().BeOfType<NotFoundObjectResult>();
			}

			[Theory, Arrange]
			public async Task PutWithRandomStringIdReturnsBadRequestResult(ActorsController sut, string randomId, int value)
			{
				var result = await sut.Put(randomId, value);

				result.Should().BeOfType<BadRequestObjectResult>();
			}

			[Theory, Arrange]
			public async Task GetWithRandomGuidReturnsNotFoundResult(ActorsController sut, Guid randomId, int value)
			{
				var result = await sut.Put(randomId.ToString(), value);

				result.Should().BeOfType<NotFoundObjectResult>();
			}
		}

		public class WhenOneActor
		{
			public class Arrange : AutoDataAttribute
			{
				public Arrange() : base(
					new Fixture()
						.Customize(new BaseCustomization())
						.Customize(new MyActorCustomization(42))
				)
				{ }
			}

			[Theory, Arrange]
			public async Task GetReturnsResultWithSingleElementGuidSequence(ActorsController sut)
			{
				var result = (await sut.Get() as OkObjectResult).Value as IEnumerable<Guid>;

				result.Should().HaveCount(1);
			}
		}
	}
}
