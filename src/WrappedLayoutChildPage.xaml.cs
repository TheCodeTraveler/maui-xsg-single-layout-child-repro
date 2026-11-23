namespace Repro;
public partial class WrappedLayoutChildPage : ContentPage
{
	public WrappedLayoutChildPage() => InitializeComponent();
	public VerticalStackLayout HostLayout => Host;
}
