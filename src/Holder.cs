namespace Repro;

public static class Holder
{
	public static readonly BindableProperty ItemsProperty = BindableProperty.CreateAttached(
		"Items", typeof(IList<View>), typeof(Holder), null, defaultValueCreator: _ => new List<View>());

	public static IList<View> GetItems(BindableObject b) => (IList<View>)b.GetValue(ItemsProperty);
	public static void SetItems(BindableObject b, IList<View> value) => b.SetValue(ItemsProperty, value);
}
