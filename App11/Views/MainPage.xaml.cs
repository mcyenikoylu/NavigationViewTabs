using App11.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace App11.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();

        nvSample.SelectedItem = nvSample.MenuItems.OfType<Microsoft.UI.Xaml.Controls.NavigationViewItem>().First();

    }

    private void nvSample_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        try
        {
            FrameNavigationOptions navOptions = new FrameNavigationOptions();
            navOptions.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;
            if (sender.PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
            {
                navOptions.IsNavigationStackEnabled = false;
            }
            Type pageType = null;
            if (args.InvokedItem == SamplePage1Item.Content)
            {
                pageType = typeof(BlankPage1);
            }
            else if (args.InvokedItem == SamplePage2Item.Content)
            {
                pageType = typeof(Blank1Page);
            }
            else if (args.InvokedItem == SamplePage3Item.Content)
            {
                pageType = typeof(DataGridPage);
            }
            else if (args.InvokedItem == SamplePage4Item.Content)
            {
                pageType = typeof(SettingsPage);
            }
            contentFrame.NavigateToType(pageType, null, navOptions);
        }
        catch (Exception ex)
        {
            throw;
        }

    }

    private void nvSample_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            contentFrame.Navigate(typeof(SettingsPage));
        }
        else
        {
            var selectedItem = (Microsoft.UI.Xaml.Controls.NavigationViewItem)args.SelectedItem;
            string pageName = "App11.Views." + ((string)selectedItem.Tag);
            Type pageType = Type.GetType(pageName);

            contentFrame.Navigate(pageType, null, args.RecommendedNavigationTransitionInfo);
        }
    }
}
