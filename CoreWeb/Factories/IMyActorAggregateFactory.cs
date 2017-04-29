using MyActorAggregate.Interfaces;

namespace CoreWeb.Factories
{
	public interface IMyActorAggregateFactory
	{
		IMyActorAggregate Create();
	}
}