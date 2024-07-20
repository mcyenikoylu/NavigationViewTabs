using App11.Contracts.Services;
using App11.Helpers;
using App11.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

using Windows.System;

using Microsoft.UI.Input;
using Windows.ApplicationModel;
using Windows.UI.ViewManagement;
using Rect = Windows.Foundation.Rect;
using Windows.UI.WindowManagement;
using Microsoft.UI.Windowing;

namespace App11.Views;

// TODO: Update NavigationViewItem titles and icons in ShellPage.xaml.
public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel
    {
        get;
    }

    private Microsoft.UI.Windowing.AppWindow m_AppWindow;

    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        ViewModel.NavigationService.Frame = NavigationFrame;
        ViewModel.NavigationViewService.Initialize(NavigationViewControl);

        //// TODO: Set the title bar icon by updating /Assets/WindowIcon.ico.
        //// A custom title bar is required for full window theme and Mica support.
        //// https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
        //App.MainWindow.ExtendsContentIntoTitleBar = true;
        //App.MainWindow.SetTitleBar(AppTitleBar);
        //App.MainWindow.Activated += MainWindow_Activated;
        //AppTitleBarText.Text = "AppDisplayName".GetLocalized();


        // Assumes "this" is a XAML Window. In projects that don't use 
        // WinUI 3 1.3 or later, use interop APIs to get the AppWindow.
        m_AppWindow = App.MainWindow.AppWindow;
        m_AppWindow.Changed += AppWindow_Changed;
        App.MainWindow.Activated += MainWindow_Activated;
        AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;
        AppTitleBar.Loaded += AppTitleBar_Loaded;

        App.MainWindow.ExtendsContentIntoTitleBar = true;
        if (App.MainWindow.ExtendsContentIntoTitleBar == true)
        {
            m_AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        }
        TitleBarTextBlock.Text = AppInfo.Current.DisplayInfo.DisplayName;
    }

    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);

        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));
    }

    //private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    //{
    //    App.AppTitlebar = AppTitleBarText as UIElement;
    //}

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBar.Margin = new Thickness()
        {
            Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
            Top = AppTitleBar.Margin.Top,
            Right = AppTitleBar.Margin.Right,
            Bottom = AppTitleBar.Margin.Bottom
        };
    }

    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        var keyboardAccelerator = new KeyboardAccelerator() { Key = key };

        if (modifiers.HasValue)
        {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;

        return keyboardAccelerator;
    }

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var navigationService = App.GetService<INavigationService>();

        var result = navigationService.GoBack();

        args.Handled = result;
    }

    // custom title bar

    private void AppTitleBar_Loaded(object sender, RoutedEventArgs e)
    {
        if (App.MainWindow.ExtendsContentIntoTitleBar == true)
        {
            // Set the initial interactive regions.
            SetRegionsForCustomTitleBar();
        }
    }

    private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if(App.MainWindow.ExtendsContentIntoTitleBar == true)
        {
            // Update interactive regions if the size of the window changes.
            SetRegionsForCustomTitleBar();
        }
    }

    private void SetRegionsForCustomTitleBar()
    {
        // Specify the interactive regions of the title bar.

        double scaleAdjustment = AppTitleBar.XamlRoot.RasterizationScale;

        RightPaddingColumn.Width = new GridLength(m_AppWindow.TitleBar.RightInset / scaleAdjustment);
        LeftPaddingColumn.Width = new GridLength(m_AppWindow.TitleBar.LeftInset / scaleAdjustment);

        // Get the rectangle around the AutoSuggestBox control.
        GeneralTransform transform = TitleBarSearchBox.TransformToVisual(null);
        Rect bounds = transform.TransformBounds(new Rect(0, 0,
                                                         TitleBarSearchBox.ActualWidth,
                                                         TitleBarSearchBox.ActualHeight));
        Windows.Graphics.RectInt32 SearchBoxRect = GetRect(bounds, scaleAdjustment);

        // Get the rectangle around the PersonPicture control.
        transform = PersonPic.TransformToVisual(null);
        bounds = transform.TransformBounds(new Rect(0, 0,
                                                    PersonPic.ActualWidth,
                                                    PersonPic.ActualHeight));
        Windows.Graphics.RectInt32 PersonPicRect = GetRect(bounds, scaleAdjustment);

        var rectArray = new Windows.Graphics.RectInt32[] { SearchBoxRect, PersonPicRect };

        InputNonClientPointerSource nonClientInputSrc =
            InputNonClientPointerSource.GetForWindowId(m_AppWindow.Id);
        nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rectArray);
    }

    private Windows.Graphics.RectInt32 GetRect(Rect bounds, double scale)
    {
        return new Windows.Graphics.RectInt32(
            _X: (int)Math.Round(bounds.X * scale),
            _Y: (int)Math.Round(bounds.Y * scale),
            _Width: (int)Math.Round(bounds.Width * scale),
            _Height: (int)Math.Round(bounds.Height * scale)
        );
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState == WindowActivationState.Deactivated)
        {
            TitleBarTextBlock.Foreground =
                (SolidColorBrush)App.Current.Resources["WindowCaptionForegroundDisabled"];
        }
        else
        {
            TitleBarTextBlock.Foreground =
                (SolidColorBrush)App.Current.Resources["WindowCaptionForeground"];
        }
    }

    private void AppWindow_Changed(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowChangedEventArgs args)
    {
        if (args.DidPresenterChange)
        {
            switch (sender.Presenter.Kind)
            {
                case AppWindowPresenterKind.CompactOverlay:
                    // Compact overlay - hide custom title bar
                    // and use the default system title bar instead.
                    AppTitleBar.Visibility = Visibility.Collapsed;
                    sender.TitleBar.ResetToDefault();
                    break;

                case AppWindowPresenterKind.FullScreen:
                    // Full screen - hide the custom title bar
                    // and the default system title bar.
                    AppTitleBar.Visibility = Visibility.Collapsed;
                    sender.TitleBar.ExtendsContentIntoTitleBar = true;
                    break;

                case AppWindowPresenterKind.Overlapped:
                    // Normal - hide the system title bar
                    // and use the custom title bar instead.
                    AppTitleBar.Visibility = Visibility.Visible;
                    sender.TitleBar.ExtendsContentIntoTitleBar = true;
                    break;

                default:
                    // Use the default system title bar.
                    sender.TitleBar.ResetToDefault();
                    break;
            }
        }
    }

    private void SwitchPresenter(object sender, RoutedEventArgs e)
    {
        if (m_AppWindow != null)
        {
            AppWindowPresenterKind newPresenterKind;
            switch ((sender as Button).Name)
            {
                case "CompactoverlaytBtn":
                    newPresenterKind = AppWindowPresenterKind.CompactOverlay;
                    break;

                case "FullscreenBtn":
                    newPresenterKind = AppWindowPresenterKind.FullScreen;
                    break;

                case "OverlappedBtn":
                    newPresenterKind = AppWindowPresenterKind.Overlapped;
                    break;

                default:
                    newPresenterKind = AppWindowPresenterKind.Default;
                    break;
            }

            // If the same presenter button was pressed as the
            // mode we're in, toggle the window back to Default.
            if (newPresenterKind == m_AppWindow.Presenter.Kind)
            {
                m_AppWindow.SetPresenter(AppWindowPresenterKind.Default);
            }
            else
            {
                // Else request a presenter of the selected kind
                // to be created and applied to the window.
                m_AppWindow.SetPresenter(newPresenterKind);
            }
        }
    }

}
