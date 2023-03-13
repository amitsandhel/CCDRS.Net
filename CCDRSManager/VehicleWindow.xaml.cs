using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CCDRSManager
{
    /// <summary>
    /// Interaction logic for VehicleWindow.xaml
    /// </summary>
    public partial class VehicleWindow : Window
    {
        [GeneratedRegex("[^0-9]+")]
        private static partial Regex CheckNumber();

        public VehicleWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Method to ensure that the values entered in the survey textbox are only integer numbers.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private new void PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = CheckNumber().IsMatch(e.Text);
        }

        /// <summary>
        /// Populate the window with the data depending on the vehicle object user selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is CCDRSManagerViewModel vm)
            {
                vm.GetVehicleData();
            }
        }

        /// <summary>
        /// Update the Vehicle and VehicleCountType tables.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void UpdateTechnology(object sender, RoutedEventArgs e)
        private void UpdateTechnology(object sender, RoutedEventArgs e)
        {
            if (DataContext is CCDRSManagerViewModel vm)
            {
                vm.UpdateVehicleData();
                // clear the textboxes of all data after successful data upload.
                VehicleDescription.Clear();
                VehicleName.Clear();
                CountType.Clear();
                Occupancy.Clear();
            }
        }
    }
}
