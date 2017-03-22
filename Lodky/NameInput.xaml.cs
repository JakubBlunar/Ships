using System.Windows;

namespace Lodky
{
    /// <summary>
    /// Class where player insert his name.
    /// Creates main window of client.
    /// </summary>
    public partial class NameInput
    {
        public NameInput()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Parse user name and creates main window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBox.Text.Trim()))
            {
                MessageBox.Show("you have to input name.");
                return;
            }

            var w = new MainWindow(TextBox.Text.Trim());
            w.Show();
            Close();
        }
    }
}