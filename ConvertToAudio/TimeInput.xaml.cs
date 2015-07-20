using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ConvertToAudio
{
    /// <summary>
    /// Interaction logic for TimeNumberInput.xaml
    /// </summary>
    public partial class TimeInput : UserControl
    {
        Dictionary<TextBox, int> TextBoxInputLimits;

        public TimeInput()
        {
            InitializeComponent();

            TextBoxInputLimits = new Dictionary<TextBox, int>();

            AllowOverride(tb_Hours, 2);
            AllowOverride(tb_Minutes, 2);
            AllowOverride(tb_Seconds, 2);
            AllowOverride(tb_Milliseconds, 4);
        }

        void AllowOverride(TextBox textBox, int maxLength)
        {
            if (!TextBoxInputLimits.ContainsKey(textBox))
            {
                // Set length
                TextBoxInputLimits.Add(textBox, maxLength);
                textBox.MaxLength = maxLength;

                // Allow override
                PropertyInfo textEditorProperty = typeof(TextBox).GetProperty(
                      "TextEditor", BindingFlags.NonPublic | BindingFlags.Instance);

                object textEditor = textEditorProperty.GetValue(textBox, null);

                // set _OvertypeMode on the TextEditor
                PropertyInfo overtypeModeProperty = textEditor.GetType().GetProperty(
                               "_OvertypeMode", BindingFlags.NonPublic | BindingFlags.Instance);

                overtypeModeProperty.SetValue(textEditor, true, null);
            }
            else
            {
                // Set length
                TextBoxInputLimits[textBox] = maxLength;
                textBox.MaxLength = maxLength;
            }
        }

        private void tb_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxInputLimits == null)
                return;

            TextBox textBox = sender as TextBox;

            textBox.CaretIndex = 0;

            if (textBox.MaxLength == TextBoxInputLimits[textBox])
            {
                textBox.MaxLength++;
            }
        }

        private void tb_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxInputLimits == null)
                return;

            TextBox textBox = sender as TextBox;
            int maxLength = TextBoxInputLimits[textBox];

            if (textBox.Text.Length > maxLength)
                textBox.Text = textBox.Text.Remove(maxLength - 1, textBox.Text.Length - maxLength);

            textBox.MaxLength = maxLength;

            // Format text to always contain leading zeroes (bit hacky)
            int n;
            int.TryParse(textBox.Text, out n);
            textBox.Text = (maxLength == 2) ? string.Format("{0:00}", n) : string.Format("{0:0000}", n);
        }

        private void tb_KeyUp(object sender, KeyEventArgs e)
        {
            if (TextBoxInputLimits == null)
                return;

            TextBox textBox = sender as TextBox;
            int maxLength = TextBoxInputLimits[textBox];

            if (textBox.Text.Length >= maxLength)
            {
                textBox.Text = textBox.Text.Remove(maxLength - 1, textBox.Text.Length - maxLength);

                if (textBox.CaretIndex >= maxLength)
                    textBox.CaretIndex = 0;
            }

        }
    }
}

