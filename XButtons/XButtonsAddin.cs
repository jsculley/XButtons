//SOLIDWORKS libraries
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swpublished;
using SolidWorksTools;
//System libraries
using System;
using System.Runtime.InteropServices;

namespace com.cadmunity.xbuttons
{
    /// <summary>
    /// SolidWorks add-in that intercepts X1/X2 mouse side button clicks and
    /// translates them into keyboard shortcuts that SolidWorks can be configured
    /// to respond to.
    ///
    /// X1 (back button) → Ctrl+Alt+Shift+B
    /// X2 (forward button) → Ctrl+Alt+Shift+F
    ///
    /// These shortcuts can be bound to any SolidWorks command via
    /// Tools → Customize → Keyboard.
    /// </summary>
    [Guid("FADD2C64-FD1A-4624-B8B5-8C0597E36AB6"), ComVisible(true)]
    [SwAddin(
         Description = "Pass X1/X2 mouse button clicks to SOLIDWORKS",
         Title = "XButtons",
         LoadAtStartup = true
    )]


    public class XButtonsAddin : SwAddin
    {
        #region Private Variables

        /// <summary>Reference to the SolidWorks application object.</summary>
        private ISldWorks swApp = null;

        /// <summary>Cookie assigned by SolidWorks, required for callback registration.</summary>
        private int addinID = 0;

        /// <summary>Manages the lifetime of the low-level mouse hook.</summary>
        private static MouseHook mouseHook;
        #endregion


        #region SolidWorks COM Registration

        /// <summary>
        /// Writes the add-in's registry entries when the DLL is registered with regasm.
        /// Adds entries under both HKLM (add-in discovery) and HKCU (startup preference).
        /// </summary>
        [ComRegisterFunctionAttribute]
        public static void RegisterFunction(Type t)
        {
            SwAddinAttribute SWattr = null;
            Type type = typeof(XButtonsAddin);

            foreach (System.Attribute attr in type.GetCustomAttributes(false))
            {
                if (attr is SwAddinAttribute)
                {
                    SWattr = attr as SwAddinAttribute;
                    break;
                }
            }

            try
            {
                Microsoft.Win32.RegistryKey hklm = Microsoft.Win32.Registry.LocalMachine;
                Microsoft.Win32.RegistryKey hkcu = Microsoft.Win32.Registry.CurrentUser;

                // HKLM entry makes the add-in visible to SolidWorks
                string keyname = "SOFTWARE\\SolidWorks\\Addins\\{" + t.GUID.ToString() + "}";
                Microsoft.Win32.RegistryKey addinkey = hklm.CreateSubKey(keyname);
                addinkey.SetValue(null, 0);
                addinkey.SetValue("Description", SWattr.Description);
                addinkey.SetValue("Title", SWattr.Title);

                // HKCU entry controls whether the add-in loads automatically at startup
                keyname = "Software\\SolidWorks\\AddInsStartup\\{" + t.GUID.ToString() + "}";
                addinkey = hkcu.CreateSubKey(keyname);
                addinkey.SetValue(null, Convert.ToInt32(SWattr.LoadAtStartup), Microsoft.Win32.RegistryValueKind.DWord);
            }
            catch (System.NullReferenceException nl)
            {
                System.Windows.Forms.MessageBox.Show("There was a problem registering this dll: SWattr is null.\n\"" + nl.Message + "\"");
            }

            catch (System.Exception e)
            {
                System.Windows.Forms.MessageBox.Show("There was a problem registering the function: \n\"" + e.Message + "\"");
            }
        }

        /// <summary>
        /// Removes the add-in's registry entries when the DLL is unregistered with regasm.
        /// </summary>
        [ComUnregisterFunctionAttribute]
        public static void UnregisterFunction(Type t)
        {
            try
            {
                Microsoft.Win32.RegistryKey hklm = Microsoft.Win32.Registry.LocalMachine;
                Microsoft.Win32.RegistryKey hkcu = Microsoft.Win32.Registry.CurrentUser;

                string keyname = "SOFTWARE\\SolidWorks\\Addins\\{" + t.GUID.ToString() + "}";
                hklm.DeleteSubKey(keyname);

                keyname = "Software\\SolidWorks\\AddInsStartup\\{" + t.GUID.ToString() + "}";
                hkcu.DeleteSubKey(keyname);
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("There was a problem unregistering this dll: \n\"" + ex.Message + "\"");
            }
        }

        #endregion

        #region ISwAddin Implementation

        /// <summary>
        /// Called by SolidWorks when the add-in is loaded. Stores the application
        /// reference, registers callbacks, and starts the mouse hook.
        /// </summary>
        /// <param name="ThisSW">The SolidWorks application instance.</param>
        /// <param name="cookie">Unique identifier assigned to this add-in by SolidWorks.</param>
        /// <returns>True if the add-in connected successfully.</returns>
        public bool ConnectToSW(object ThisSW, int cookie)
        {
            swApp = (ISldWorks)ThisSW;
            addinID = cookie;

            //Setup callbacks
            swApp.SetAddinCallbackInfo(0, this, addinID);

            // Safety: stop any existing hook before installing a new one
            mouseHook?.Stop();
            mouseHook = new MouseHook();
            mouseHook.Start();
            return true;
        }

        /// <summary>
        /// Called by SolidWorks when the add-in is unloaded. Stops the mouse hook
        /// and releases the SolidWorks COM reference. The double GC.Collect() calls
        /// are required by the SolidWorks add-in contract to ensure all managed
        /// COM wrappers are released before SolidWorks unloads the DLL.
        /// </summary>
        /// <returns>True if the add-in disconnected successfully.</returns>
        public bool DisconnectFromSW()
        {
            mouseHook.Stop();
            mouseHook = null;
            if (swApp != null)
            {
                Marshal.ReleaseComObject(swApp);
                swApp = null;
            }
            // Required by SolidWorks add-in contract: flush all COM RCWs
            GC.Collect();
            GC.WaitForPendingFinalizers();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            return true;
        }
        #endregion
    }
}
