using System;
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ChildProcess1
{
	public class Program
	{
		public const string ServiceName = "child1-process-example";
		public const string ActivitySourceName = nameof(ChildProcess1);
		public static ActivitySource ActivitySource = new ActivitySource(ActivitySourceName);

		static void Main(string[] args)
		{
			var provider = Sdk.CreateTracerProviderBuilder()
				.SetResourceBuilder(ResourceBuilder.CreateDefault()
					.AddService(ServiceName))
				.AddSource(ActivitySourceName)
				.AddOtlpExporter()
				.Build();

			ActivityContext context = new ActivityContext();
			if (args.Length == 3)
			{
				ActivityTraceId traceId = ActivityTraceId.CreateFromString(args[0].ToString().AsSpan());
				ActivitySpanId spanId = ActivitySpanId.CreateFromString(args[1].ToString().AsSpan());
				Enum.TryParse(args[2].ToString(), out ActivityTraceFlags traceFlags);
				context = new ActivityContext(traceId, spanId: spanId, traceFlags, isRemote: true);
			}

			using (var activity = ActivitySource.StartActivity("child", ActivityKind.Server, context))
			{
			}

			provider.Dispose();
		}
	}
}