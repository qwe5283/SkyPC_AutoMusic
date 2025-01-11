using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SkyPC_AutoMusic
{
    public class Win32
    {
        public const int KEYEVENTF_KEYDOWN = 0x0000;
        public const int KEYEVENTF_KEYUP = 0x0002;

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_NOACTIVATE = 0x08000000;

        //PostMessage参数

        public const uint WM_KEYDOWN = 0x0100; // 键盘按键按下消息
        public const uint WM_KEYUP = 0x0101;   // 键盘按键释放消息
        public const uint WM_CHAR = 0x0102;
        public const uint WM_NCACTIVATE = 0x0086;
        public const uint WM_ACTIVATE = 0x0006;// 激活
        public const uint WM_ACTIVATEAPP = 0x001C;

        public const uint WA_ACTIVE = 1;
        public const uint WA_INACTIVE = 0;

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern byte MapVirtualKey(byte wCode, int wMap);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr PostMessage(IntPtr hWnd,uint Msg,IntPtr wParam,IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }
}
