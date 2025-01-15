using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GeneralUtility
{
    /// <summary>
    /// スクリーンキャプチャ用のユーティリティクラス
    /// </summary>
    public static partial class CaptureHelper
    {
        /// <summary>
        /// 現在のプロセスのメインウィンドウのスクリーンショットを取得する
        /// </summary>
        /// <returns></returns>
        public static Image CaptureMainWindow()
        {
            var hWnd = Process.GetCurrentProcess().MainWindowHandle;
            return CaptureWindow(hWnd);
        }


        /// <summary>
        /// アクティブウィンドウのスクリーンショットを取得する
        /// </summary>
        /// <returns></returns>
        public static Image CaptureActiveWindow()
        {
            var hWnd = User32.GetForegroundWindow();
            return CaptureWindow(hWnd);
        }


        /// <summary>
        /// アクティブウィンドウのスクリーンショットを取得する
        /// </summary>
        /// <returns></returns>
        public static Image CaptureWindow(IntPtr hWnd)
        {
            var winRect = new User32.RECT();
            User32.GetWindowRect(hWnd, ref winRect);
            Dwmapi.DwmGetWindowAttribute(
                hWnd,
                (int)Dwmapi.DwmWindowAttribute.DWMWA_EXTENDED_FRAME_BOUNDS,
                out var bounds,
                Marshal.SizeOf(typeof(User32.RECT)));
            //Console.WriteLine($"DwmWindowAttribute left={bounds.left}, top={bounds.top}, right={bounds.right}, bottom={bounds.bottom}");
            //Console.WriteLine($"GetWindowRect left={winRect.left}, top={winRect.top}, right={winRect.right}, bottom={winRect.bottom}");
            if (bounds.left != 0 && bounds.top != 0 && bounds.right != 0 && bounds.bottom != 0)
            {
                return CaptureScreen(bounds.ToRectangle());
            }
            return CaptureScreen(winRect.ToRectangle());
        }


        /// <summary>
        /// 指定領域のスクリーンショットを取得する
        /// </summary>
        /// <param name="rect">取得する範囲</param>
        /// <returns></returns>
        public static Image CaptureScreen(Rectangle rect)
        {
            Bitmap bmp = new Bitmap(rect.Width, rect.Height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(rect.Location, new Point(0, 0), rect.Size, CopyPixelOperation.SourceCopy);
            }
            return bmp;
        }


        /// <summary>
        /// Helper class containing User32 API functions
        /// </summary>
        private static class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;

                public Rectangle ToRectangle()
                {
                    return new Rectangle(left, top, right - left, bottom - top);
                }
            }

            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();
            [DllImport("User32.dll")]
            public static extern IntPtr MonitorFromPoint(Point pt, UInt32 dwFlags);
        }

        private static class Dwmapi
        {
            [Flags]
            public enum DwmWindowAttribute : uint
            {
                DWMWA_NCRENDERING_ENABLED = 1,
                DWMWA_NCRENDERING_POLICY,
                DWMWA_TRANSITIONS_FORCEDISABLED,
                DWMWA_ALLOW_NCPAINT,
                DWMWA_CAPTION_BUTTON_BOUNDS,
                DWMWA_NONCLIENT_RTL_LAYOUT,
                DWMWA_FORCE_ICONIC_REPRESENTATION,
                DWMWA_FLIP3D_POLICY,
                DWMWA_EXTENDED_FRAME_BOUNDS,
                DWMWA_HAS_ICONIC_BITMAP,
                DWMWA_DISALLOW_PEEK,
                DWMWA_EXCLUDED_FROM_PEEK,
                DWMWA_CLOAK,
                DWMWA_CLOAKED,
                DWMWA_FREEZE_REPRESENTATION,
                DWMWA_LAST
            }

            [DllImport("dwmapi.dll")]
            public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out User32.RECT pvAttribute, int cbAttribute);
        }
    }
}
