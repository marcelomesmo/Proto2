using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_STANDALONE_WIN
using System;
using System.Runtime.InteropServices;
#endif

namespace Core.Camera
{
    public class TransparentWindowMoveableUI :
        MonoBehaviour,
        IPointerDownHandler,
        IDragHandler,
        IPointerUpHandler
    {
#if UNITY_STANDALONE_WIN

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(
            IntPtr hWnd,
            out RECT lpRect
        );

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags
        );

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        private bool dragging = false;

        private POINT dragStartMousePos;
        private RECT dragStartWindowRect;
        
        void Start()
        {
            bool supported =
                Application.platform == RuntimePlatform.WindowsPlayer;

            // Do this in case we want to hide in non-Desktop builds
            //gameObject.SetActive(supported);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            dragging = true;

            GetCursorPos(out dragStartMousePos);

            IntPtr hwnd = GetActiveWindow();

            GetWindowRect(hwnd, out dragStartWindowRect);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            dragging = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!dragging)
                return;

            GetCursorPos(out POINT currentMousePos);

            int deltaX = currentMousePos.X - dragStartMousePos.X;
            int deltaY = currentMousePos.Y - dragStartMousePos.Y;

            IntPtr hwnd = GetActiveWindow();

            SetWindowPos(
                hwnd,
                IntPtr.Zero,
                dragStartWindowRect.Left + deltaX,
                dragStartWindowRect.Top + deltaY,
                0,
                0,
                SWP_NOSIZE | SWP_NOZORDER
            );
        }

#endif
    }
}