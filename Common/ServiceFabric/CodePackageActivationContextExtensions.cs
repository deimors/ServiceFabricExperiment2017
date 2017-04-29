using System;
using System.Fabric;

namespace Common.ServiceFabric
{
	public static class CodePackageActivationContextExtensions
	{
		public static Uri BuildServiceUri(this ICodePackageActivationContext context, string serviceName)
			=> new Uri($"{context.ApplicationName}/{serviceName}");
	}
}
