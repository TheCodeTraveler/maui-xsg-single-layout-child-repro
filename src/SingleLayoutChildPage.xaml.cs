namespace Repro;
public partial class SingleLayoutChildPage : ContentPage
{
	public SingleLayoutChildPage() => InitializeComponent();
	public VerticalStackLayout HostLayout => Host;
}
