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

public partial class StationCountObservation
{
    /// <summary>
    /// Foreign key to the survey_station table associated to the survey_station primary key attribute.
    /// </summary>
    public int SurveyStationId { get; set; }

    /// <summary>
    /// Foreign key to the vehicle_count_type table associated to the vehicle_count_type primary key attribute.
    /// </summary>
    public int VehicleCountTypeId { get; set; }

    /// <summary>
    /// The results of the number of vehicles observed. 
    /// </summary>
    public int Observation { get; set; }

    /// <summary>
    /// Time measured in minutes. e.g. 600
    /// </summary>
    public int Time { get; set; }

    /// <summary>
    /// Modify SurveyStation Class since SurveyStation has a foreign key to survey_station table.
    /// </summary>
    public virtual SurveyStation SurveyStation { get; set; } = null!;

    /// <summary>
    /// Modify VehicleCountType class since VehicleCountType has a foreign key to vehicle_count_type table.
    /// </summary>
    public virtual VehicleCountType VehicleCountType { get; set; } = null!;
}
