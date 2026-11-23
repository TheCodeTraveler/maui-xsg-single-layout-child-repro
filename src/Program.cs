using System.Text.Json;
using Repro;

const string expectedTargetType = "to type 'System.Collections.Generic.IList`1[Microsoft.Maui.Controls.View]'.";

var leg = args.Length > 0 ? args[0] : "";
var cases = leg switch
{
	"bug" => new (string Name, Func<int> Create, bool ExpectBug, string ChildType, int ExpectedCount)[]
	{
		(nameof(SingleLayoutChildPage), () => Holder.GetItems(new SingleLayoutChildPage().HostLayout).Count, true, "VerticalStackLayout", 1),
		(nameof(SingleGridChildPage), () => Holder.GetItems(new SingleGridChildPage().HostLayout).Count, true, "Grid", 1),
		(nameof(PocoSingleGridChildPage), () => new PocoSingleGridChildPage().HostView.Views.Count, true, "Grid", 1),
	},
	"control" => new (string, Func<int>, bool, string, int)[]
	{
		(nameof(SingleLabelChildPage), () => Holder.GetItems(new SingleLabelChildPage().HostLayout).Count, false, "", 1),
		(nameof(TwoLayoutChildrenPage), () => Holder.GetItems(new TwoLayoutChildrenPage().HostLayout).Count, false, "", 2),
		(nameof(SingleLayoutChildXamlCPage), () => Holder.GetItems(new SingleLayoutChildXamlCPage().HostLayout).Count, false, "", 1),
		(nameof(PocoSingleGridChildXamlCPage), () => new PocoSingleGridChildXamlCPage().HostView.Views.Count, false, "", 1),
	},
	"workaround" => new (string, Func<int>, bool, string, int)[]
	{
		(nameof(WrappedLayoutChildPage), () => Holder.GetItems(new WrappedLayoutChildPage().HostLayout).Count, false, "", 1),
	},
	_ => throw new ArgumentException("Usage: dotnet run -- bug|control|workaround"),
};

var controlsVersion = typeof(Microsoft.Maui.Controls.Label).Assembly
	.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
	.Cast<System.Reflection.AssemblyInformationalVersionAttribute>()
	.FirstOrDefault()?.InformationalVersion ?? "unknown";
Console.WriteLine($"REPRO-C02 leg={leg} controlsVersion={controlsVersion}");

var results = new List<object>();
var allAsExpected = true;
foreach (var (name, create, expectBug, childType, expectedCount) in cases)
{
	string observed;
	string? exception = null;
	bool asExpected;
	try
	{
		var count = create();
		observed = $"OK Count={count}";
		asExpected = !expectBug && count == expectedCount;
	}
	catch (InvalidCastException e)
	{
		observed = $"InvalidCastException: {e.Message}";
		exception = e.ToString();
		var expectedSource = $"Unable to cast object of type 'Microsoft.Maui.Controls.{childType}' ";
		asExpected = expectBug
			&& e.Message.Contains(expectedSource, StringComparison.Ordinal)
			&& e.Message.EndsWith(expectedTargetType, StringComparison.Ordinal)
			&& e.StackTrace?.Contains($"{name}.InitializeComponent", StringComparison.Ordinal) is true;
	}
	catch (Exception e)
	{
		observed = $"{e.GetType().FullName}: {e.Message}";
		exception = e.ToString();
		asExpected = false;
	}

	allAsExpected &= asExpected;
	Console.WriteLine($"REPRO-C02 leg={leg} case={name} expected={(expectBug ? "InvalidCastException" : "OK")} observed={observed} asExpected={asExpected}");
	if (exception is not null)
	{
		Console.WriteLine($"REPRO-C02-STACK case={name}");
		Console.WriteLine(exception);
		Console.WriteLine($"REPRO-C02-STACK-END case={name}");
	}

	results.Add(new { leg, name, expected = expectBug ? $"InvalidCastException from {name}.InitializeComponent casting {childType} to IList<View>" : "OK", observed, exception, asExpected });
}

Directory.CreateDirectory("results");
File.WriteAllText($"results/{leg}.json", JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"REPRO-C02 leg={leg} DONE allAsExpected={allAsExpected}");
return allAsExpected ? 0 : 1;
