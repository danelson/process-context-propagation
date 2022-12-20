using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ChildProcess2
{
	public class Program
	{
		public const string ServiceName = "child2-process-example";
		public const string ActivitySourceName = nameof(ChildProcess2);
		public static ActivitySource ActivitySource = new ActivitySource(ActivitySourceName);

		static void Main(string[] args)
		{
			var provider = Sdk.CreateTracerProviderBuilder()
				.SetResourceBuilder(ResourceBuilder.CreateDefault()
					.AddService(ServiceName))
				.AddSource(ActivitySourceName)
				.AddOtlpExporter()
				.Build();

			string traceContextJson = Environment.GetEnvironmentVariable("TRACECONTEXT");
			Dictionary<string, string> props = JsonSerializer.Deserialize<Dictionary<string, string>>(traceContextJson);
			TextMapPropagator propagator = new TraceContextPropagator();
			PropagationContext propagationContext = propagator.Extract(default, props, Getter);

			using (var activity = ActivitySource.StartActivity("child", ActivityKind.Server, propagationContext.ActivityContext))
			{
			}

			provider.Dispose();
		}

		private static IEnumerable<string> Getter(Dictionary<string, string> props, string key)
		{
			if (props.TryGetValue(key, out string value))
			{
				return new[] { value };
			}
			return Enumerable.Empty<string>();
		}
	}
}