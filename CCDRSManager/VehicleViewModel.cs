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

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CCDRSManager;

/// <summary>
/// Manage and provides executes the various use cases in regards to VehicleCountType.
/// In this case the only use case is editing the vehicles.
/// </summary>
public class VehicleViewModel : INotifyPropertyChanged
{
    private readonly VehicleRepository _vehicleRepository = Configuration.VehicleRepository;

    /// <summary>
    /// Collection of all VehicleCountType objects.
    /// </summary>
    public ObservableCollection<VehicleCountTypeModel> VehicleCountTypes { get; }

    /// <summary>
    /// Controls access to the VehicleViewModel and get a list of all vehicle_count_types in the database.
    /// </summary>
    public VehicleViewModel()
    {
        VehicleCountTypes = _vehicleRepository.Vehicles;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Method to update the vehicle_count_type with new user supplied data.
    /// </summary>
    /// <param name="vehicleCountTypeId">Primary key of VehicleCountType object.</param>
    /// <param name="header">Name of column to be edited. E.g. description or occupancy.</param>
    /// <param name="newValue">The new value the user provided.</param>
    public void UpdateVehicleCountTypeData(int vehicleCountTypeId, string header, string newValue)
    {
        _vehicleRepository.UpdateVehicleCountTypeData(vehicleCountTypeId, header, newValue);
    }

}
