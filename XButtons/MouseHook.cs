using System;
using System.Runtime.InteropServices;

using static com.cadmunity.xbuttons.Win32;

namespace com.cadmunity.xbuttons
{


    /// <summary>
    /// Represents which extended mouse button was pressed.
    /// </summary>
    enum XButton
    {
        /// <summary>The X1 (back) side button</summary>
        X1,
        /// <summary>The X2 (forward) side button</summary>
        X2
    }

    /// <summary>
    /// Installs a system-wide low-level mouse hook (WH_MOUSE_LL) to intercept X1/X2
    /// side button clicks and translate them into SolidWorks keyboard shortcuts.
    /// 
    /// The hook delegate is held in a static field to prevent garbage collection
    /// while the hook is active.
    /// </summary>
    public class MouseHook
    {
        /// <summary>
        /// Handle returned by SetWindowsHookEx. Stored so the hook can be
        /// removed by UnhookWindowsHookEx when the add-in disconnects.
        /// </summary>
        private static IntPtr _hookID = IntPtr.Zero;

        /// <summary>
        /// The INPUT arrays that hole the key chords that will be sent went
        /// the X! or X2 buttons are pressed
        /// </summary>
        private static INPUT[] x1Inputs, x2Inputs;

        /// <summary>
        /// Static reference to the hook callback delegate. Must be static to
        /// prevent the GC from collecting it while the unmanaged hook is active.
        /// </summary>
        private static LowLevelMouseProc _proc = HookCallback;

        internal MouseHook(INPUT[] selectedX1Inputs, INPUT[]  selectedX2Inputs)
        {
            x1Inputs = selectedX1Inputs;
            x2Inputs = selectedX2Inputs;
        }

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
                    if (x1Inputs != null)
                    {
                        SendInput((uint)x1Inputs.Length, x1Inputs, Marshal.SizeOf(typeof(INPUT)));
                    }
                    break;
                case 2:
                    if (x2Inputs != null)
                    {
                        SendInput((uint)x2Inputs.Length, x2Inputs, Marshal.SizeOf(typeof(INPUT)));
                    }
                    break;
            }
            // Always pass the event to the next hook in the chain
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}