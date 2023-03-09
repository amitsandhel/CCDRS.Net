using System.Windows;
using System.Windows.Controls;

namespace CCDRSManager
{
    /// <summary>
    /// Interaction logic for ScreenlineDialog.xaml
    /// </summary>
    public partial class ScreenlineDialog : Window
    {
        public ScreenlineDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Method to add screenline data to the database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddScreenlineSurvey(object sender, RoutedEventArgs e)
        {
            if (DataContext is CCDRSManagerViewModel vm)
            {
                await vm.AddScreenlineAsync();
            }
        }

        /// <summary>
        /// Generic method to open a window dialog box to upload a file. 
        /// </summary>
        /// <param name="nameOfTextbox">Name of Textbox control.</param>
        public void OpenFileDialog(TextBox nameOfTextbox)
        {
            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dialog.FileName;
                nameOfTextbox.Text = filename;
            }
        }

        /// <summary>
        /// Method to open the screenline csv file provided by the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenScreenlineFileDialog(object sender, RoutedEventArgs e)
        {
            OpenFileDialog(ScreenlineFileName);
        }
    }
}
