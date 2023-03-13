using System;
using System.Windows;

namespace CCDRSManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Method which runs on button click to open the wizard as a dialog box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunWizard(object sender, RoutedEventArgs e)
        {
            // initialize the CCDRS Wizard Window object.
            var win = new CCDRSWizard()
            {
                Owner = this,
            };
            // Open the window as a dialog box.
            win.ShowDialog();
        }

        /// <summary>
        /// Edit a VehicleCountType technology.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditVehicle(object sender, RoutedEventArgs e)
        {
            VehicleWindow win = new();
            win.ShowDialog();
        }

        /// <summary>
        /// Add Screenline data to the database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddScreenline(object sender, RoutedEventArgs e)
        {
            ScreenlineDialog dialog = new();
            dialog.ShowDialog();
        }

        private void DeleteSurvey(object sender, RoutedEventArgs e)
        {
            DeleteSurveyWindow window = new();
            window.ShowDialog();
        }
    }
}
