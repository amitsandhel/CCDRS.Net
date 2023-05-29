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

namespace CCDRS.Model;

/// <summary>
/// Class that maps to the screenline_station table
/// </summary>
public partial class ScreenlineStation
{
    /// <summary>
    /// Foreign key to the screenline table associated to the screenline primary key attribute
    /// </summary>
    public int ScreenlineId { get; set; }

    /// <summary>
    /// Foreign key to the station table associated to the station primary key attribute
    /// </summary>
    public int StationId { get; set; }

    /// <summary>
    ///  Modify Screenline Class.
    /// </summary>
    public virtual Screenline Screenline { get; set; } = null!;
    /// <summary>
    /// Modify Station class.
    /// </summary>
    public virtual Station Station { get; set; } = null!;
}
