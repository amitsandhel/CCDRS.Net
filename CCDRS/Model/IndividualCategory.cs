/*
    Copyright 2022 University of Toronto
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

using System;
using System.Collections.Generic;

namespace CCDRS.Model;

/// <summary>
/// Class that maps to the individual_categories view.
/// </summary>
public partial class IndividualCategory
{
    /// <summary>
    /// Primary serial key of type int that is auto generated
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the Vehicle
    /// </summary>
    public string VehicleName { get; set; } = string.Empty;

    /// <summary>
    /// The number of occupants that can sit in a vehicle
    /// </summary>
    public int Occupancy { get; set; }

    /// <summary>
    /// Human readable description of the vehicle
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to the vehicle_count_type table associated to the vehicle_count_type primary key attribute.
    /// </summary>
    public int VehicleCountTypeId { get; set; }

    /// <summary>
    /// Stores the type of vehicle. Used to determine drop down options
    /// </summary>
    public int CountType { get; set; }

    /// <summary>
    /// Name of the region
    /// </summary>
    public string RegionName { get; set; } = string.Empty;

    /// <summary>
    /// Primary serial key of the rgion
    /// </summary>
    public int RegionId { get; set; }

    /// <summary>
    /// Year of which the data is selected for
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Foreign key to the survey table associated to the survey primary key attribute
    /// </summary>
    public int SurveyId { get; set; }
}
