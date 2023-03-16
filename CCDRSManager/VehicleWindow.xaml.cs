/*
    Copyright 2023 University of Toronto
    This file is part of CCDRS.
    CCDRS is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    CCDRS is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with CCDRS.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Text.RegularExpressions;
using System.Windows;

namespace CCDRSManager;

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
