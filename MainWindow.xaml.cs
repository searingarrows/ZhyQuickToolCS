using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using System.Management.Automation;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
using Windows.Win32;
using Windows.Win32.Foundation;
using WinRT;
using System.Collections.ObjectModel;
using Windows.Storage;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
namespace ZhyQuickToolCS
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public bool HandleClosedEvents { get; set; } = true;

        private const uint WM_HOTKEY = 0x0312;
        private const uint VK_OEM_3 = 0xC0;
        private const uint VK_ESCAPE = 0x1B;

        private const int HOTKEY_SHOW = 1;
        private const int HOTKEY_HIDE = 2;
        private int winWidth = 800, winHeight = 600;

        private Windows.Win32.UI.WindowsAndMessaging.WNDPROC origPrc;
        private Windows.Win32.UI.WindowsAndMessaging.WNDPROC hotKeyPrc;
        private StorageFolder? scriptsFolder;
        private ObservableCollection<Script> Scripts { get; } = new ObservableCollection<Script>();
        private bool executingTasks = false;
        private bool ExecutingTasks
        {
            get
            {
                return executingTasks;
            }
        }
        private static void Show(HWND hwnd, bool show)
        {
            if (show)
            {
                PInvoke.ShowWindow(hwnd, Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_SHOW);
                PInvoke.SetForegroundWindow(hwnd);
                PInvoke.SetFocus(hwnd);
                PInvoke.SetActiveWindow(hwnd);
            }
            else
            {
                PInvoke.ShowWindow(hwnd, Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_HIDE);
            }
        }
        public void Show(bool show)
        {
            HWND hwnd = new HWND(WinRT.Interop.WindowNative.GetWindowHandle(this).ToInt32());
            Show(hwnd, show);
        }
        private LRESULT HotKeyPrc(HWND hwnd, uint uMsg, WPARAM wParam, LPARAM lParam)
        {
            if (uMsg == WM_HOTKEY)
            {
                switch (wParam.Value)
                {
                    case HOTKEY_SHOW:
                        Show(hwnd, true);
                        break;
                    case HOTKEY_HIDE:
                        Show(hwnd, false);
                        break;
                }
                return (LRESULT)IntPtr.Zero;
            }

            return PInvoke.CallWindowProc(origPrc, hwnd, uMsg, wParam, lParam);
        }

        private async void InitializeScripstFolder()
        {
            executingTasks = true;
            const string ScriptsFolderName = "Scripts";
            try
            {
                scriptsFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(ScriptsFolderName);
            }
            catch (Exception)
            {

            }

            if (scriptsFolder == null)
            {
                scriptsFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(ScriptsFolderName);
            }
            executingTasks = false;
            RefreshScripts();
        }

        private async void RefreshScripts()
        {
            //MessageBox.show($"{ApplicationData.Current.LocalFolder.Path}");
            if (ExecutingTasks || scriptsFolder == null)
            {
                return;
            }
            executingTasks = true;
            var files = await scriptsFolder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName);


            var tasks = new List<Task<PSScript?>>();
            foreach (var file in files)
            {
                tasks.Add(PSScript.Load(file));
            }
            await Task.WhenAll(tasks);

            Scripts.Clear();
            foreach (var task in tasks)
            {
                if (task.Result != null)
                {
                    Scripts.Add(task.Result);
                }
            }
            //var items = ScriptsView.Items;
            //items.Clear();
            //foreach (var script in scripts)
            //{
            //    items.Add(script);
            //}
            executingTasks = false;
        }

        public MainWindow()
        {
            this.InitializeComponent();

            InitializeScripstFolder();

            HWND hwnd = new HWND(WinRT.Interop.WindowNative.GetWindowHandle(this).ToInt32());
            var success = PInvoke.RegisterHotKey(hwnd, HOTKEY_SHOW, Windows.Win32.UI.Input.KeyboardAndMouse.HOT_KEY_MODIFIERS.MOD_WIN | Windows.Win32.UI.Input.KeyboardAndMouse.HOT_KEY_MODIFIERS.MOD_NOREPEAT, VK_OEM_3);
            if (!success)
            {

            }
            success = PInvoke.RegisterHotKey(hwnd, HOTKEY_HIDE, Windows.Win32.UI.Input.KeyboardAndMouse.HOT_KEY_MODIFIERS.MOD_NOREPEAT, VK_ESCAPE);

            hotKeyPrc = HotKeyPrc;
            var hotKeyPrcPointer = Marshal.GetFunctionPointerForDelegate(hotKeyPrc);
            origPrc = Marshal.GetDelegateForFunctionPointer<Windows.Win32.UI.WindowsAndMessaging.WNDPROC>((IntPtr)PInvoke.SetWindowLongPtr(hwnd, Windows.Win32.UI.WindowsAndMessaging.WINDOW_LONG_PTR_INDEX.GWL_WNDPROC, hotKeyPrcPointer));

            var presenter = AppWindow.Presenter.As<Microsoft.UI.Windowing.OverlappedPresenter>();
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
            presenter.IsResizable = true;
            presenter.SetBorderAndTitleBar(true, true);

            TrayMenuItem0.Command = new RelayCommand(() => Show(true));
            TrayMenuItem1.Command = new RelayCommand(Exit);


            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            object w, h;
            if (localSettings.Values.TryGetValue("WinWidth", out w))
            {
                int.TryParse(w as string, out winWidth);
            }
            if (localSettings.Values.TryGetValue("WinHeight", out h))
            {
                int.TryParse(h as string, out winHeight);
            }
            AppWindow.Resize(new Windows.Graphics.SizeInt32(winWidth, winHeight));
        }
        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState == WindowActivationState.Deactivated)
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values["WinWidth"] = winWidth.ToString();
                localSettings.Values["WinHeight"] = winHeight.ToString();
                AppWindow.Hide();
            }
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            if (HandleClosedEvents)
            {
                args.Handled = true;
                AppWindow.Hide();
            }
        }

        private void RefreshScripts_Clicked(object sender, RoutedEventArgs e)
        {
            RefreshScripts();
        }

        private void OpenScriptsFolder(object sender, RoutedEventArgs e)
        {
            if (ExecutingTasks || scriptsFolder == null)
            {
                return;
            }
            Process.Start("explorer.exe", scriptsFolder.Path);
        }

        private void ScriptsView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (ExecutingTasks)
            {
                return;
            }
            var script = e.ClickedItem as Script;
            if (script != null)
            {
                script.Execute();
            }
        }

        private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            winWidth = (int)args.Size.Width;
            winHeight = (int)args.Size.Height;
        }

        public void Exit()
        {
            HandleClosedEvents = false;
            TrayIcon.Dispose();
            Close();
            Application.Current.Exit();
        }
    }
}
