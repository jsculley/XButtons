using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace com.cadmunity.xbuttons
{
    public partial class SetupDialog : Form
    {
        private bool x1SettingsActive = true;
        private readonly HashSet<Keys> currentlyPressedKeys = new HashSet<Keys>();
        private readonly HashSet<Keys> storedChordKeys = new HashSet<Keys>();
        private List<Keys> keyChordForX1Button = new List<Keys>();
        private List<Keys> keyChordForX2Button = new List<Keys>();
        public SetupDialog()
        {
            InitializeComponent();
            this.KeyPreview = true;
            readSettings();
            storedChordKeys.UnionWith(keyChordForX1Button);
            updateLabels(keyChordForX1Button);
        }

        #region Event handlers

        /// <summary>
        /// Look at the pressed key.  Add any modifier keys to the chord.  If it isn't
        /// a modifer and a non-modifier hasn't already been added, then add it to the chord
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void processKeyDown(object sender, KeyEventArgs e)
        {

            //Replace any non-modifier with this non-modifier
            if (!isModifier(e.KeyCode))
            {
                currentlyPressedKeys.RemoveWhere(k => !isModifier(k));
                storedChordKeys.RemoveWhere(k => !isModifier(k));
            }
            currentlyPressedKeys.Add(e.KeyCode);
            storedChordKeys.Add(e.KeyCode);
            updateLabels(storedChordKeys.ToList());
            updateButtonState();
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        /// <summary>
        /// Removes the released key from the currently active keys
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void processKeyUp(object sender, KeyEventArgs e)
        {
            Keys k = e.KeyCode;
            //Windows typically reports Keys.ControlKey for down events, but up events might be one of 4
            //different key codes
            if (k == Keys.ControlKey || k == Keys.Control || k == Keys.LControlKey || k == Keys.RControlKey) k = Keys.ControlKey;
            if (k == Keys.LShiftKey || k == Keys.RShiftKey || k == Keys.Shift) k = Keys.ShiftKey;
            if (k == Keys.LMenu || k == Keys.RMenu || k == Keys.Alt) k = Keys.Menu;
            currentlyPressedKeys.Remove(k);

        }
        /// <summary>
        /// Removes all list entries ands updates the label text and button state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearButtonPressed(object sender, EventArgs e)
        {
            currentlyPressedKeys.Clear();
            storedChordKeys.Clear();
            updateLabels(storedChordKeys.ToList());
            updateButtonState();
        }

        /// <summary>
        /// Stores the current chord in the SelectedX2Keys property
        /// and loads the SelectedX1Keys
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void previousButtonClicked(object sender, EventArgs e)
        {
            if (!x1SettingsActive)
            {
                x1SettingsActive = true;
                keyChordForX2Button = storedChordKeys.ToList();
                storedChordKeys.Clear();
                currentlyPressedKeys.Clear();
                if (keyChordForX1Button != null)
                {
                    storedChordKeys.UnionWith(keyChordForX1Button);
                }
                updateLabels(storedChordKeys.ToList());
                updateButtonState();
            }
        }

        /// <summary>
        /// If currently editing the X1 settings, store them in SelectedX1Keys
        /// and load the SelectedX2Keys.  If editing the X2 settings, store
        /// the keys in the Settings object and set DialogResult property to OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nextOrFinishedClicked(object sender, EventArgs e)
        {
            if (x1SettingsActive) //Next
            {
                x1SettingsActive = false;
                keyChordForX1Button = storedChordKeys.ToList();
                storedChordKeys.Clear();
                currentlyPressedKeys.Clear();
                if (keyChordForX2Button != null)
                {
                    storedChordKeys.UnionWith(keyChordForX2Button);
                }
                updateLabels(storedChordKeys.ToList());
                updateButtonState();
            }
            else //Finish
            {
                keyChordForX2Button = storedChordKeys.ToList();
                Properties.Settings.Default.X1Input = string.Join("-", keyChordForX1Button);
                Properties.Settings.Default.X2Input = string.Join("-", keyChordForX2Button);
                Properties.Settings.Default.FirstLoad = false;
                Properties.Settings.Default.Save();
                this.DialogResult = DialogResult.OK;
            }
        }

        /// <summary>
        /// Don't store changes, set DialogResult to Cancel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelPressed(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
        #endregion

        #region Private implementation

        /// <summary>
        /// Read the keystroke strings from the Settings object and
        /// turn them into Lists of Keys enum objects
        /// </summary>
        private void readSettings()
        {
            string x1Setting = Properties.Settings.Default.X1Input;
            string x2Setting = Properties.Settings.Default.X2Input;
            string[] keyStrings;
            if (x1Setting != null)
            {
                keyStrings = x1Setting.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string nextKeyString in keyStrings)
                {
                    if (Enum.TryParse(nextKeyString, out Keys nextKey))
                    {
                        keyChordForX1Button.Add(nextKey);
                    }

                }
            }
            if (x2Setting != null)
            {
                keyStrings = x2Setting.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string nextKeyString in keyStrings)
                {
                    if (Enum.TryParse(nextKeyString, out Keys nextKey))
                    {
                        keyChordForX2Button.Add(nextKey);
                    }

                }
            }
        }

        /// <summary>
        /// Sorts the list of keys into Windows preferred order and formats
        /// a string of the list entries separated by '-'
        /// </summary>
        /// <param name="keyChord"></param>
        private void updateLabels(List<Keys> keyChord)
        {
            if (keyChord == null) { return; }
            var ordered = keyChord.OrderBy(GetSortPriority);
            string labelText = string.Join("-", ordered.Select(k => getFriendlyKeyName(k)));
            currentChordLabel.Text = labelText.Equals("") ? "<No keys pressed>" : labelText;
            if (x1SettingsActive)
            {
                instructionLabel.Text = "Press the keys to send for the X1 (Back) button";
            }
            else
            {
                instructionLabel.Text = "Press the keys to send for the X2 (Forward) button";
            }
        }

        /// <summary>
        /// The Keys enumeration string values don't match what users expect in
        /// some cases.
        /// </summary>
        /// <param name="k"></param>
        /// <returns>the expected string for a key</returns>
        private string getFriendlyKeyName(Keys k)
        {
            switch (k)
            {
                case Keys.ControlKey: return "Ctrl";
                case Keys.Menu: return "Alt";
                case Keys.ShiftKey: return "Shift";
                case Keys.Back: return "Backspace";
                case Keys.Left: return "Left Arrow";
                case Keys.Right: return "Right Arrow";
                case Keys.Up: return "Up Arrow";
                case Keys.Down: return "Down Arrow";
                case Keys.PageUp: return "Page Up";
                case Keys.PageDown: return "Page Down";
                case Keys.Home: return "Home";
                case Keys.End: return "End";
                case Keys.Insert: return "Insert";
                case Keys.Delete: return "Delete";
                default:
                    string name = k.ToString();
                    // Handle the "D4" -> "4" logic here
                    if (name.Length == 2 && name.StartsWith("D") && char.IsDigit(name[1]))
                        return name.Substring(1);

                    return name;
            }
        }

        /// <summary>
        /// Windows has a recommended order when describing key chords
        /// </summary>
        /// <param name="k"></param>
        /// <returns>the propert sorting position per Windows recommended order</returns>
        private int GetSortPriority(Keys k)
        {
            switch (k)
            {
                case Keys.ControlKey: return 1;
                case Keys.Menu: return 2; // Alt
                case Keys.ShiftKey: return 3;
                default: return 4;
            }
        }

        /// <summary>
        /// Is the specified key a modifier (Ctrl, Shift, Alt) or a 'normal' key
        /// </summary>
        /// <param name="key"></param>
        /// <returns>true if key isa modifier, false otherwise</returns>
        private bool isModifier(Keys key)
        {
            return key == Keys.ControlKey || key == Keys.ShiftKey || key == Keys.Menu;
        }

        /// <summary>
        /// Enable/disable buttons and change button text as required.
        /// </summary>
        private void updateButtonState()
        {
            if (x1SettingsActive)
            {
                previousButton.Enabled = false;
                nextOrFinishButton.Enabled = true;
                nextOrFinishButton.Text = "Next";
            }
            else
            {
                previousButton.Enabled = true;
                nextOrFinishButton.Text = "Finish";
            }
            clearButton.Enabled = (storedChordKeys.Count == 0) ? false : true;
        }
        #endregion
    }
}
