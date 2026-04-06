using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace com.cadmunity.xbuttons
{


    /// <summary>
    /// Represents which extended mouse button was pressed.
    /// </summary>
    enum XKey
    {
        /// <summary>The X1 (back) side button, mapped to Ctrl+Alt+Shift+B in SolidWorks.</summary>
        X1,
        /// <summary>The X2 (forward) side button, mapped to Ctrl+Alt+Shift+F in SolidWorks.</summary>
        X2
    }

    /// <summary>
    /// Installs a system-wide low-level mouse hook (WH_MOUSE_LL) to intercept X1/X2
    /// side button clicks and translate them into SolidWorks keyboard shortcuts.
    /// 
    /// X1 → Ctrl+Alt+Shift+B
    /// X2 → Ctrl+Alt+Shift+F
    /// 
    /// The hook delegate is held in a static field to prevent garbage collection
    /// while the hook is active.
    /// </summary>
    public class MouseHook
    {
        /// <summary>INPUT struct type value for keyboard events.</summary>
        const int INPUT_KEYBOARD = 1;

        /// <summary>dwFlags value to indicate a key-up (release) event.</summary>
        const uint KEYEVENTF_KEYUP = 0x0002;

        /// <summary>
        /// dwFlags value indicating the wScan field contains a hardware scan code.
        /// Always combined with key-down and key-up flags for reliable delivery.
        /// </summary>
        const uint KEYEVENTF_SCANCODE = 0x0008;

        /// <summary>
        /// Handle returned by SetWindowsHookEx. Stored so the hook can be
        /// removed by UnhookWindowsHookEx when the add-in disconnects.
        /// </summary>
        private static IntPtr _hookID = IntPtr.Zero;

        /// <summary>
        /// Static reference to the hook callback delegate. Must be static to
        /// prevent the GC from collecting it while the unmanaged hook is active.
        /// </summary>
        private static LowLevelMouseProc _proc = HookCallback;

        /// <summary>Delegate signature matching the HOOKPROC expected by SetWindowsHookEx.</summary>
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        
        /// <summary>
        /// Installs the low-level mouse hook. Call once when the add-in connects.
        /// Has no effect if the hook is already active.
        /// </summary>
        public void Start()
        {
            _hookID = SetHook(_proc);
        }

        /// <summary>
        /// Removes the low-level mouse hook. Call when the add-in disconnects
        /// to avoid leaving a dangling system-wide hook.
        /// </summary>
        public void Stop()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Registers the low-level mouse hook with Windows.
        /// For WH_MOUSE_LL (hook id 14), hMod and dwThreadId must both be zero —
        /// the hook is system-wide and the module handle is ignored by Windows.
        /// </summary>
        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            return SetWindowsHookEx(14, proc, IntPtr.Zero, 0);
        }

        /// <summary>
        /// Called by Windows for every mouse event in the system while the hook is active.
        /// 
        /// Only WM_XBUTTONDOWN (0x020C) events are acted on; all other events are
        /// passed through immediately. The mouseData high-word identifies which
        /// X button was pressed: 1 = X1, 2 = X2.
        /// 
        /// CallNextHookEx is always called to ensure other applications and hooks
        /// continue to receive mouse events normally.
        /// </summary>
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // nCode < 0 means we must pass the event on without processing
            if (nCode < 0 || wParam != (IntPtr)0x020C)

            {
                return CallNextHookEx(_hookID, nCode, wParam, lParam);
            }

            // mouseData is at byte offset 8 in MSLLHOOKSTRUCT.
            // The high-word identifies which X button: 1 = X1, 2 = X2.
            int mouseData = Marshal.ReadInt32(lParam, 8);
            int button = (mouseData >> 16) & 0xFFFF;

            switch (button)
            {
                case 1:
                    SendXKey(XKey.X1);
                    break;
                case 2:
                    SendXKey(XKey.X2);
                    break;
            }
            // Always pass the event to the next hook in the chain
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        /// <summary>
        /// Synthesizes a Ctrl+Alt+Shift+B (X1) or Ctrl+Alt+Shift+F (X2) key chord
        /// using SendInput. The chord is sent as 4 key-down events followed by
        /// 4 key-up events in reverse order.
        /// 
        /// KEYEVENTF_SCANCODE is always included so the events are recognized
        /// correctly by applications that read hardware scan codes.
        /// </summary>
        private static void SendXKey(XKey key)
        {
            INPUT[] inputs = new INPUT[8];
            ushort VK_CONTROL = 0X11;
            ushort VK_MENU = 0x12;
            ushort VK_SHIFT = 0x10;
            ushort VK_F = 0x46;
            ushort VK_B = 0x42;
            // Key-down sequence: Ctrl → Alt → Shift → B/F
            inputs[0] = CreateInput(VK_CONTROL, 0);                  //----|
            inputs[1] = CreateInput(VK_MENU, 0);    //Alt            //    |
            inputs[2] = CreateInput(VK_SHIFT, 0);                    //    |---Press
            inputs[3] = CreateInput(key == XKey.X1 ? VK_B : VK_F, 0);//----|
           
            // Key-up sequence (reverse order): B/F → Shift → Alt → Ctrl
            inputs[4] = CreateInput(key == XKey.X1 ? VK_B : VK_F, KEYEVENTF_KEYUP);//----|
            inputs[5] = CreateInput(VK_SHIFT, KEYEVENTF_KEYUP);                    //    |
            inputs[6] = CreateInput(VK_MENU, KEYEVENTF_KEYUP);                     //    |--Release
            inputs[7] = CreateInput(VK_CONTROL, KEYEVENTF_KEYUP);                  //----|

            uint retval = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        /// <summary>
        /// Builds a keyboard INPUT struct for the given virtual key code and flags.
        /// KEYEVENTF_SCANCODE is always OR'd in so both the virtual key and
        /// hardware scan code fields are populated.
        /// </summary>
        /// <param name="vk">Virtual-key code (e.g. 0x11 for Ctrl).</param>
        /// <param name="flags">KEYEVENTF_* flags; typically 0 (key down) or KEYEVENTF_KEYUP.</param>
        private static INPUT CreateInput(ushort vk, uint flags)
        {
            ushort scanCode = (ushort)MapVirtualKey(vk, 0);
            uint finalFlags = flags | KEYEVENTF_SCANCODE;
            return new INPUT
            {
                type = 1, // INPUT_KEYBOARD
                ki = new KEYBDINPUT { 
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
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);
        #endregion
    }
}