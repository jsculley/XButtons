using System;
using System.Runtime.InteropServices;

namespace com.cadmunity.xbuttons
{
    internal class Win32
    {
        /// <summary>INPUT struct type value for keyboard events.</summary>
        internal const int INPUT_KEYBOARD = 1;

        /// <summary>dwFlags value to indicate a key-up (release) event.</summary>
        internal const uint KEYEVENTF_KEYUP = 0x0002;

        /// <summary>
        /// dwFlags value indicating the wScan field contains a hardware scan code.
        /// Always combined with key-down and key-up flags for reliable delivery.
        /// </summary>
        internal const uint KEYEVENTF_SCANCODE = 0x0008;

        /// <summary>Delegate signature matching the HOOKPROC expected by SetWindowsHookEx.</summary>
        internal delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);


        /// <summary>
        /// Builds a keyboard INPUT struct for the given virtual key code and flags.
        /// KEYEVENTF_SCANCODE is always OR'd in so both the virtual key and
        /// hardware scan code fields are populated.
        /// </summary>
        /// <param name="vk">Virtual-key code (e.g. 0x11 for Ctrl).</param>
        /// <param name="flags">KEYEVENTF_* flags; typically 0 (key down) or KEYEVENTF_KEYUP.</param>
        internal static INPUT CreateInput(ushort vk, uint flags)
        {
            ushort scanCode = (ushort)MapVirtualKey(vk, 0);
            uint finalFlags = flags | KEYEVENTF_SCANCODE;
            return new INPUT
            {
                type = 1, // INPUT_KEYBOARD
                ki = new KEYBDINPUT
                {
                    wVk = vk,
                    wScan = scanCode,
                    dwFlags = finalFlags,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            };
        }

        #region Win32 API Imports and Structs

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT { public int x; public int y; }

        /// <summary>
        /// Data block passed to WH_MOUSE_LL hook callbacks via the lParam pointer.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public int mouseData;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        /// <summary>Keyboard event data used in the INPUT union.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;      // Virtual-key code (e.g., 0x11 for CTRL)
            public ushort wScan;    // Hardware scan code
            public uint dwFlags;    // 0 for key down, 0x0002 for key up
            public uint time;
            public IntPtr dwExtraInfo;
        }

        /// <summary>Mouse event data used in the INPUT union (unused here, present for completeness).</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        /// <summary>Hardware event data used in the INPUT union (unused here, present for completeness).</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        /// <summary>
        /// Input event descriptor passed to SendInput.
        /// The type field selects which union member is active.
        /// Field offset 8 is required for the union on 64-bit processes
        /// due to pointer-size padding after the 4-byte type field.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct INPUT
        {
            [FieldOffset(0)] public uint type; // INPUT_KEYBOARD = 1
            [FieldOffset(8)] public KEYBDINPUT ki;
            [FieldOffset(8)] public MOUSEINPUT mi;
            [FieldOffset(8)] public HARDWAREINPUT hi;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        internal static extern uint MapVirtualKey(uint uCode, uint uMapType);
        #endregion
    }
}
