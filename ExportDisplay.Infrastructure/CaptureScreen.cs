using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ExportDisplay.Infrastructure
{
    class CaptureScreen
    {
        //This structure shall be used to keep the size of the screen.
        public struct SIZE
        {
            public int Cx;
            public int Cy;
        }

        public static Bitmap CaptureDesktop()
        {
			Bitmap bmp = null;
//			lock (_lock)
			{
				IntPtr hDC = IntPtr.Zero;
				try
				{
					SIZE size;
					hDC = Win32Stuff.GetDC(Win32Stuff.GetDesktopWindow());
					IntPtr hMemDC = GDIStuff.CreateCompatibleDC(hDC);

					size.Cx = Win32Stuff.GetSystemMetrics
							  (Win32Stuff.SM_CXSCREEN);

					size.Cy = Win32Stuff.GetSystemMetrics
							  (Win32Stuff.SM_CYSCREEN);

					IntPtr hBitmap = GDIStuff.CreateCompatibleBitmap(hDC, size.Cx, size.Cy);

					if (hBitmap != IntPtr.Zero)
					{
						IntPtr hOld = GDIStuff.SelectObject
							(hMemDC, hBitmap);

						GDIStuff.BitBlt(hMemDC, 0, 0, size.Cx, size.Cy, hDC,
													   0, 0, GDIStuff.SRCCOPY);

						GDIStuff.SelectObject(hMemDC, hOld);
						GDIStuff.DeleteDC(hMemDC);
						bmp = Image.FromHbitmap(hBitmap);
						GDIStuff.DeleteObject(hBitmap);
						GC.Collect();
					}
				}
				finally
				{
					if (hDC != IntPtr.Zero)
					{
						Win32Stuff.ReleaseDC(Win32Stuff.GetDesktopWindow(), hDC);
					}
				}
			}
            return bmp;
        }

        public static Bitmap CaptureCursor(ref int x, ref int y)
        {
			Bitmap bmp = null;
//			lock (_lock)
			{
				Win32Stuff.CURSORINFO ci = new Win32Stuff.CURSORINFO();
				Win32Stuff.ICONINFO icInfo;
				ci.cbSize = Marshal.SizeOf(ci);
				if (Win32Stuff.GetCursorInfo(out ci))
				{
					if (ci.flags == Win32Stuff.CURSOR_SHOWING)
					{
						IntPtr hicon = Win32Stuff.CopyIcon(ci.hCursor);
						if (Win32Stuff.GetIconInfo(hicon, out icInfo))
						{
							if (icInfo.hbmMask != IntPtr.Zero)
							{
								GDIStuff.DeleteObject(icInfo.hbmMask);
							}
							if (icInfo.hbmColor != IntPtr.Zero)
							{
								GDIStuff.DeleteObject(icInfo.hbmColor);
							}
							x = ci.ptScreenPos.x - icInfo.xHotspot;
							y = ci.ptScreenPos.y - icInfo.yHotspot;

							Icon ic = Icon.FromHandle(hicon);
							if (ic.Width > 0 && ic.Height > 0)
							{
								bmp = ic.ToBitmap();
							}
							Win32Stuff.DestroyIcon(hicon);
						}
					}
				}
			}

			return bmp;
        }

        public static Bitmap CaptureDesktopWithCursor()
        {
            int cursorX = 0;
            int cursorY = 0;
        	Graphics g;

        	Bitmap desktopBmp = CaptureDesktop();
            Bitmap cursorBmp = CaptureCursor(ref cursorX, ref cursorY);
            if (desktopBmp != null)
            {
            	if (cursorBmp != null)
                { 
                    Rectangle r = new Rectangle(cursorX, cursorY, cursorBmp.Width, cursorBmp.Height);
                    g = Graphics.FromImage(desktopBmp);
                    g.DrawImage(cursorBmp, r);
                    g.Flush();

                    return desktopBmp;
                }
            	return desktopBmp;
            }

        	return null;

        }
    }
}
