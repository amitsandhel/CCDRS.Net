﻿using System.Windows;

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

        private void EditVehicle(object sender, RoutedEventArgs e)
        {
            VehicleDialog dialog = new();
            dialog.ShowDialog();
        }
    }
}
