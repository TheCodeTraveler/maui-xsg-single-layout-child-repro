using System.Text.Json;
using Repro;

const string expectedBugMessageEnd = "to type 'System.Collections.Generic.IList`1[Microsoft.Maui.Controls.View]'.";

var leg = args.Length > 0 ? args[0] : "";
var cases = leg switch
{
	"bug" => new (string Name, Func<int> Create, bool ExpectBug)[]
	{
		(nameof(SingleLayoutChildPage), () => Holder.GetItems(new SingleLayoutChildPage().HostLayout).Count, true),
		(nameof(SingleGridChildPage), () => Holder.GetItems(new SingleGridChildPage().HostLayout).Count, true),
		(nameof(PocoSingleGridChildPage), () => new PocoSingleGridChildPage().HostView.Views.Count, true),
	},
	"control" => new (string, Func<int>, bool)[]
	{
		(nameof(SingleLabelChildPage), () => Holder.GetItems(new SingleLabelChildPage().HostLayout).Count, false),
		(nameof(TwoLayoutChildrenPage), () => Holder.GetItems(new TwoLayoutChildrenPage().HostLayout).Count, false),
		(nameof(SingleLayoutChildXamlCPage), () => Holder.GetItems(new SingleLayoutChildXamlCPage().HostLayout).Count, false),
		(nameof(PocoSingleGridChildXamlCPage), () => new PocoSingleGridChildXamlCPage().HostView.Views.Count, false),
	},
	"workaround" => new (string, Func<int>, bool)[]
	{
		(nameof(WrappedLayoutChildPage), () => Holder.GetItems(new WrappedLayoutChildPage().HostLayout).Count, false),
	},
	_ => throw new ArgumentException("Usage: dotnet run -- bug|control|workaround"),
};

var results = new List<object>();
var allAsExpected = true;
foreach (var (name, create, expectBug) in cases)
{
	string observed;
	bool asExpected;
	try
	{
		var count = create();
		observed = $"OK Count={count}";
		var expectedCount = name == nameof(TwoLayoutChildrenPage) ? 2 : 1;
		asExpected = !expectBug && count == expectedCount;
	}
	catch (InvalidCastException e)
	{
		observed = $"InvalidCastException: {e.Message}";
		asExpected = expectBug && e.Message.EndsWith(expectedBugMessageEnd, StringComparison.Ordinal);
	}
	catch (Exception e)
	{
		observed = $"{e.GetType().FullName}: {e.Message}";
		asExpected = false;
	}

	allAsExpected &= asExpected;
	Console.WriteLine($"REPRO-C02 leg={leg} case={name} expected={(expectBug ? "InvalidCastException" : "OK")} observed={observed} asExpected={asExpected}");
	results.Add(new { leg, name, expected = expectBug ? "InvalidCastException (IList<View>)" : "OK", observed, asExpected });
}

Directory.CreateDirectory("results");
File.WriteAllText($"results/{leg}.json", JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"REPRO-C02 leg={leg} DONE allAsExpected={allAsExpected}");
return allAsExpected ? 0 : 1;
