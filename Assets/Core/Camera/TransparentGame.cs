using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace Core.Camera
{
    public class TransparentGame : MonoBehaviour
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("Dwmapi.dll")]
        private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

        struct MARGINS { public int leftWidth, rightWidth, topHeight, bottomHeight; }

        void Start()
        {
            IntPtr hWnd = GetActiveWindow();
            MARGINS margins = new MARGINS { leftWidth = -1, rightWidth = -1, topHeight = -1, bottomHeight = -1 };

            SetWindowLong(hWnd, -16, 0x00000000 | 0x00080000); // Set window style
            DwmExtendFrameIntoClientArea(hWnd, ref margins);
            SetWindowPos(hWnd, new IntPtr(-1), 0, 0, 0, 0, 0x0001 | 0x0002); // Topmost
        }
#endif
    }
}
