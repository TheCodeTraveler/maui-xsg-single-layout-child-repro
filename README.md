# maui-xsg-single-layout-child-repro

The XAML source generator (XSG, the default inflator in .NET MAUI 11) casts a property element's single child to the property's `IList<T>` type when the child is a Layout. `InitializeComponent` then throws `InvalidCastException`.

```xml
<local:Holder.Items>
    <VerticalStackLayout />   <!-- one Layout child: generated as (IList<View>)child -->
</local:Holder.Items>
```

## Reproduce locally

```sh
cd src && dotnet run -- bug
```

- Observed: `InvalidCastException: Unable to cast object of type 'Microsoft.Maui.Controls.VerticalStackLayout' to type 'System.Collections.Generic.IList`1[Microsoft.Maui.Controls.View]'.`
- Expected: the child is added to the list (`Count == 1`), as with a `Label` child, two children, or XamlC.

Versions: .NET SDK 11.0.100, `net11.0`, Microsoft.Maui.Controls 11.0.10. No workload or device needed: the defect is in generated C#.

## Legs (`.github/workflows/repro.yml`, ubuntu-latest)

| Leg | Cases | Proves |
|---|---|---|
| `bug` | `SingleLayoutChildPage`, `SingleGridChildPage` (attached `IList<View>`), `PocoSingleGridChildPage` (CLR `IList<View>` property) | each throws the exact exception above; the generated `*.xsg.cs` contains the bad cast |
| `control` | single `Label` child, two Layout children, single Layout child with `Inflator="XamlC"`, CLR property with XamlC | each is added (`Count` 1 or 2) |
| `workaround` | single Layout child wrapped in a `ContentView` | the child is added |

The run is green only when the `bug` leg shows the bug and the other legs are clean. A leg fails if its marker line `REPRO-C02 leg=<leg> DONE allAsExpected=True` is missing.

No fix leg: the defect is in the generator (`Microsoft.Maui.Controls.SourceGen`), which user code cannot patch. Workarounds: wrap the child in a `ContentView`, or use `Inflator="XamlC"` (deprecated in .NET 11).

CI: (run link added after the first run)
Upstream issue: (to be filed)

## Root cause and recommended fix

- Regression from dotnet/maui#32780 (first in 10.0.20; 10.0.11 works). Same source in 11.0.10 and `main`.
- `SetPropertyHelpers.HasExplicitConversion` (lines 645-660) returns true when both types implement `IEnumerable` and the target is an interface. Every Layout implements `IList<IView>`, so a Layout counts as a collection.
- `CanSetValue` (line 490) and `CanSet` (line 350) run before `CanAdd`, so `SetValue` (line 547) emits `(IList<View>)child`. C# allows the cast of an unsealed class to any interface, so it compiles and fails at run time.
- XamlC requires real assignability (`InheritsFromOrImplements`, `Build.Tasks/SetPropertiesVisitor.cs:1684`) and falls back to `Add`.
- Fix: allow the explicit cast only when it can succeed (interface to interface, or base to derived), or try `CanAdd` before the cast for property-element children. Keep the #32758 case (`IReadOnlyList<string>` to `IList`) working.
