//SOLIDWORKS libraries
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swpublished;
using SolidWorks.Interop.swconst;
using SolidWorksTools;
//System libraries
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using static com.cadmunity.xbuttons.InputFactory;

//Windows API
using static com.cadmunity.xbuttons.Win32;

namespace com.cadmunity.xbuttons
{
    /// <summary>
    /// SolidWorks add-in that intercepts X1/X2 mouse side button clicks and
    /// translates them into keyboard shortcuts that SolidWorks can be configured
    /// to respond to.
    ///
    /// When first loaded, a setup dialog allows the user to spcify what
    /// key combinations (chords) will be sent by the X1 (back) and X2 (forward)
    /// mouse buttons.
    ///
    /// These shortcuts can then be bound to any SolidWorks command via
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
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }
            if (Properties.Settings.Default.FirstLoad)
            {
                showSettingsDialog();
            }

            //Setup callbacks
            swApp.SetAddinCallbackInfo(0, this, addinID);
            createMenu();
            // Safety: stop any existing hook before installing a new one
            mouseHook?.Stop();
            mouseHook = new MouseHook(getInputStructure(XButton.X1), getInputStructure(XButton.X2));
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
            removeMenu();
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

        #region Callbacks
        /// <summary>
        /// Display the dialog that lets the user specify what key chords will be linked
        /// to the X1 and X2 mouse buttons
        /// </summary>
        public void showSettingsDialog()
        {
            using (SetupDialog dlg = new SetupDialog())
            {
                DialogResult dr = new SetupDialog().ShowDialog();
                if (dr == DialogResult.OK)
                {
                    mouseHook?.Stop();
                    mouseHook = new MouseHook(getInputStructure(XButton.X1), getInputStructure(XButton.X2));
                    mouseHook.Start();
                }
            }
        }
        #endregion

        #region Private implementation

        /// <summary>
        /// Registers the add-in's custom menus from the SOLIDWORKS user interface.
        /// </summary>
        /// <remarks>
        /// This method loops through all standard document frames (Part, Assembly, Drawing) 
        /// to ensure the "XButtons" and "Settings" menus are added during add-in startup 
        /// </remarks>
        private void createMenu()
        {
            int[] docTypes = {
                (int)swDocumentTypes_e.swDocNONE,
                (int)swDocumentTypes_e.swDocPART,
                (int)swDocumentTypes_e.swDocASSEMBLY,
                (int)swDocumentTypes_e.swDocDRAWING
            };
            foreach (int docType in docTypes)
            {
                int commandID = swApp.AddMenuItem5(docType, addinID, "&Settings...@&XButtons@&Tools", -1, "showSettingsDialog", "", "Change XButtons settings", new string[3]);
            }
        }

        /// <summary>
        /// Unregisters the add-in's custom menus from the SOLIDWORKS user interface.
        /// </summary>
        /// <remarks>
        /// This method loops through all standard document frames (Part, Assembly, Drawing) 
        /// to ensure the "XButtons" and "Settings" menus are removed during add-in shutdown 
        /// or cleanup, preventing "ghost" menu items.
        /// </remarks>
        private void removeMenu()
        {
            int[] docTypes = {
                (int)swDocumentTypes_e.swDocNONE,
                (int)swDocumentTypes_e.swDocPART,
                (int)swDocumentTypes_e.swDocASSEMBLY,
                (int)swDocumentTypes_e.swDocDRAWING
            };
            foreach (int docType in docTypes)
            {
                swApp.RemoveMenu(docType, "&Settings...@&XButtons@&Tools", "");
               swApp.RemoveMenu(docType, "&XButtons@&Tools", "");
            }
        }
        #endregion
    }
}
