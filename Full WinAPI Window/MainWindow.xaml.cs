
using Full_WinAPI_Window.WindowsAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Full_WinAPI_Window
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static IntPtr hinst;
        private static UInt16 atom;
        public MainWindow()
        {
            InitializeComponent();
            CreateMessagePump();
        }

        private void CreateMessagePump()
        {
            //Main2(System.Diagnostics.Process.GetCurrentProcess().Handle, IntPtr.Zero, string.Empty, (int)ShowWindowCommands.Normal);
            MessagePump messagePump = new MessagePump();
            messagePump.CreateMessagePump(IntPtr.Zero, IntPtr.Zero, "some string", 0);
        }


        static bool Main2(IntPtr hinstance, IntPtr hPrevInstance, string lpCmdLine, int nCmdShow)
        {
            MSG msg;

            if (!InitApplication(hinstance))
                return false;

            if (!InitInstance(hinstance, nCmdShow))
                return false;

            sbyte hasMessage;

            while ((hasMessage = WinAPI.GetMessage(out msg, IntPtr.Zero, 0, 0)) != 0 && hasMessage != -1)
            {
                WinAPI.TranslateMessage(ref msg);
                WinAPI.DispatchMessage(ref msg);
            }
            return Equals(msg.wParam, UIntPtr.Zero);
            //UNREFERENCED_PARAMETER(lpCmdLine);   


        }


        private static bool InitApplication(IntPtr hinstance)
        {

            WNDCLASSEX wcx = new WNDCLASSEX();

            wcx.cbSize = Marshal.SizeOf(wcx);
            wcx.style = (int)(ClassStyles.VerticalRedraw | ClassStyles.HorizontalRedraw);

            unsafe
            {
                //IntPtr address = MainWndProc; -- this is not necessary to put inside a Unsafe context  
                IntPtr address2 = Marshal.GetFunctionPointerForDelegate((Delegate)(WndProc)MainWndProc);
                wcx.lpfnWndProc = address2;
            }

            wcx.cbClsExtra = 0;
            wcx.cbWndExtra = 0;
            wcx.hInstance = hinstance;
            wcx.hIcon = WinAPI.LoadIcon(
                    IntPtr.Zero, new IntPtr(10));
            //wndClass.hCursor = WinAPI.LoadCursor(IntPtr.Zero, (int)IdcStandardCursor.IDC_ARROW);  
            wcx.hCursor = WinAPI.LoadCursor(IntPtr.Zero, (int)Win32_IDC_Constants.IDC_ARROW);
            wcx.hbrBackground = WinAPI.GetStockObject(StockObjects.WHITE_BRUSH);
            wcx.lpszMenuName = "MainMenu";
            wcx.lpszClassName = "MainWClass";
            //     wcx.hIconSm = LoadImage(hinstance, // small class icon   
            //MAKEINTRESOURCE(5),  
            //IMAGE_ICON,  
            //GetSystemMetrics(SM_CXSMICON),  
            //GetSystemMetrics(SM_CYSMICON),  
            //LR_DEFAULTCOLOR);   

            // it might be as this:  
            //   problems with p/invoke CreateWindowEx() and RegisterClassEx()  
            //  http://social.msdn.microsoft.com/Forums/vstudio/en-US/8580a805-383b-4b17-8bd8-514da4a5f3a4/problems-with-pinvoke-createwindowex-and-registerclassex   
            // ATOM?  
            UInt16 ret = WinAPI.RegisterClassEx2(ref wcx);
            if (ret != 0)
            {
                string message = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                Console.WriteLine("Failed to call RegisterClasEx, error = {0}", message);
            }
            //return WinAPI.RegisterClassEx(ref wcx) != 0;  
            atom = ret;
            return ret != 0;
        }

        private static bool InitInstance(IntPtr hInstance, int nCmdShow)
        {
            IntPtr hwnd;

            hinst = hInstance;
            short a = 0;

            hwnd = WinAPI.CreateWindowEx2(
                0,
                //"MainWClass",  
                atom,
                "Sample",
                WindowStyles.WS_OVERLAPPED,
                Win32_CW_Constant.CW_USEDEFAULT,
                Win32_CW_Constant.CW_USEDEFAULT,
                Win32_CW_Constant.CW_USEDEFAULT,
                Win32_CW_Constant.CW_USEDEFAULT,
                IntPtr.Zero,
                IntPtr.Zero,
                hInstance,
                IntPtr.Zero);
            if (hwnd == IntPtr.Zero)
            {
                string error = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                Console.WriteLine("Failed to InitInstance , error = {0}", error);
                return false;
            }
            WinAPI.ShowWindow(hwnd, (ShowWindowCommands)nCmdShow);
            WinAPI.UpdateWindow(hwnd);
            return true;
        }

        // check this post - http://stackoverflow.com/questions/1969049/c-sharp-p-invoke-marshalling-structures-containing-function-pointers  
        //   
        static IntPtr MainWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            IntPtr hdc;
            PAINTSTRUCT ps;
            RECT rect;
            //switch ((WM) message)  
            //{  
            //    WinAPI.BeginPaint(hWnd, out ps);  
            //    break;  
            //}  
            switch ((WM)msg)
            {
                case WM.PAINT:
                    hdc = WinAPI.BeginPaint(hWnd, out ps);
                    WinAPI.GetClientRect(hWnd, out rect);
                    WinAPI.DrawText(hdc, "Hello, Windows 98!", -1, ref rect, Win32_DT_Constant.DT_SINGLELINE | Win32_DT_Constant.DT_CENTER | Win32_DT_Constant.DT_VCENTER);
                    WinAPI.EndPaint(hWnd, ref ps);
                    return IntPtr.Zero;
                    break;
                case WM.DESTROY:
                    WinAPI.PostQuitMessage(0);
                    return IntPtr.Zero;
                    break;
            }

            return WinAPI.DefWindowProc(hWnd, (WM)msg, wParam, lParam);
        }
    }
}
