using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static com.cadmunity.xbuttons.Win32;

namespace com.cadmunity.xbuttons
{
    internal class InputFactory
    {
        /// <summary>
        /// Retrieves a string from the Settings object and converts it to
        /// a proper INPUT strucure
        /// </summary>
        /// <param name="button"></param>
        /// <returns>the appropriate INPUT structure for the specified X button</returns>
        internal static INPUT[] getInputStructure(XButton button)
        {
            string keySetting = null;
            switch (button)
            {
                case XButton.X1:
                    keySetting = Properties.Settings.Default.X1Input;
                    break;
                case XButton.X2:
                    keySetting = Properties.Settings.Default.X2Input;
                    break;
            }
            if (keySetting == null)
            {
                return null;
            }
            return getInputForSetting(keySetting);
        }

        private static INPUT[] getInputForSetting(string setting)
        {
            string[] keys = setting.Split('-');
            INPUT[] inputs = new INPUT[keys.Length * 2];
            int index = 0;

            //Add the key presses
            foreach (string key in keys)
            {
                ushort code = getKeyCode(key);
                if (code == 0) { return null; } //invalid key
                inputs[index] = CreateInput(getKeyCode(key), 0);
                index++;
            }
            //Add the key releases
            foreach (string key in keys.Reverse())
            {
                ushort code = getKeyCode(key);
                if (code == 0) { return null; } //invalid key
                inputs[index] = CreateInput(getKeyCode(key), KEYEVENTF_KEYUP);
                index++;
            }
            return inputs;
        }

        private static ushort getKeyCode(string keyString)
        {
            if (Enum.TryParse(keyString, true, out Keys key))
            {
                return (ushort)key;
            }
            return 0;
        }
    }
}
