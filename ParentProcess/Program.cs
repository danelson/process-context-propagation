using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ParentProcess
{
	public class Program
	{
		public const string ServiceName = "parent-process-example";
		public const string ActivitySourceName = nameof(ParentProcess);
		public static ActivitySource ActivitySource = new ActivitySource(ActivitySourceName);

		static void Main(string[] args)
		{
			var provider = Sdk.CreateTracerProviderBuilder()
				.SetResourceBuilder(ResourceBuilder.CreateDefault()
					.AddService(ServiceName))
				.AddSource(ActivitySourceName)
				.AddOtlpExporter()
				.Build();

			using (var activity = ActivitySource.StartActivity("parent", ActivityKind.Server))
			{
				Process child1 = new Process();
				child1.StartInfo.FileName = $"{AppDomain.CurrentDomain.BaseDirectory}{nameof(ChildProcess1)}.exe";
				if (activity != null)
				{
					child1.StartInfo.Arguments = $"{activity.TraceId} {activity.SpanId} {activity.ActivityTraceFlags}";
				}
				child1.Start();
				child1.WaitForExit((int)TimeSpan.FromSeconds(10).TotalMilliseconds);

				Process child2 = new Process();
				child2.StartInfo.FileName = $"{AppDomain.CurrentDomain.BaseDirectory}{nameof(ChildProcess2)}.exe";
				if (activity != null)
				{
					Dictionary<string, object> props = new Dictionary<string, object>();
					TextMapPropagator propagator = new TraceContextPropagator();
					propagator.Inject(new PropagationContext(activity.Context, Baggage.Current), props, InjectContext);
					string value = JsonSerializer.Serialize(props);
					Environment.SetEnvironmentVariable("TRACECONTEXT", value);
				}

				child2.Start();
				child2.WaitForExit((int)TimeSpan.FromSeconds(10).TotalMilliseconds);
			}

			provider.Dispose();
		}

		private static void InjectContext(Dictionary<string, object> props, string key, string value)
		{
			props[key] = value;
		}
	}
}