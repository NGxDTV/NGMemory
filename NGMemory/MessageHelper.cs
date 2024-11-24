using System;
using System.Windows.Forms;
using static NGMemory.Kernel32;

namespace NGMemory
{
    public class MessageHelper
    {
        /// <summary>
        /// Displays a message box with a specified message, title, buttons, and icon.
        /// Uses the .NET MessageBox class to present the dialog to the user.
        /// </summary>
        /// <param name="message">The text to display in the message box.</param>
        /// <param name="title">The title of the message box window.</param>
        /// <param name="button">The buttons to display in the message box. Defaults to OK.</param>
        /// <param name="icon">The icon to display in the message box. Defaults to Information.</param>
        public static void ShowMessage(string message, string title, MessageBoxButtons button = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            MessageBox.Show(message, title, button, icon);
        }
    }
}
