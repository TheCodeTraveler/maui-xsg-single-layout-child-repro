namespace Repro;
public partial class TwoLayoutChildrenPage : ContentPage
{
	public TwoLayoutChildrenPage() => InitializeComponent();
	public VerticalStackLayout HostLayout => Host;
}
