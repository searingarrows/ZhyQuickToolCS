using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.Win32;
using Windows.Win32.Foundation;
namespace ZhyQuickToolCS
{
    internal class MessageBox
    {
        public static void Show(string message, string title = "Message")
        {
            HWND hwnd = new HWND(0);
            PInvoke.MessageBox(hwnd, message, title, Windows.Win32.UI.WindowsAndMessaging.MESSAGEBOX_STYLE.MB_OK);
        }
    }
}
