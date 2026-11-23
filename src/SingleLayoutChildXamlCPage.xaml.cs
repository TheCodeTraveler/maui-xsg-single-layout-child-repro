namespace Repro;
public partial class SingleLayoutChildXamlCPage : ContentPage
{
	public SingleLayoutChildXamlCPage() => InitializeComponent();
	public VerticalStackLayout HostLayout => Host;
}
