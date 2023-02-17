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

using System.Windows;
using System.Windows.Controls;

namespace CCDRSManager
{
    /// <summary>
    /// Interaction logic for VehicleDialog.xaml
    /// </summary>
    public partial class VehicleDialog : Window
    {
        public VehicleDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Method calls when user is done editing the values in the datagrid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateVehicleCountType(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Get the edited row and column
            var editedRow = e.Row.Item as VehicleCountTypeModel;
            var header = e.Column.Header.ToString();

            // Get the new value
            var editedTextbox = e.EditingElement as TextBox;
            var newValue = editedTextbox.Text;
            var vehicleCountTypeId = editedRow.Id;

            if (DataContext is VehicleViewModel vm)
            {
                vm.UpdateVehicleCountTypeData(vehicleCountTypeId, header, newValue);
            }
        }
    }
}
