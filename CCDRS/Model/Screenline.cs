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
/// Class that maps to the screenline table
/// </summary>
public partial class Screenline
{
    /// <summary>
    /// Id is primary serial key of type int that is auto generated.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the region table associated to the region primary key attribute
    /// </summary>
    public int RegionId { get; set; }

    /// <summary>
    /// String representation of the screenline code 
    /// </summary>
    public string SlineCode { get; set; } = string.Empty;

    /// <summary>
    /// Description of the screenline
    /// </summary>
    public string Note { get; set; } = string.Empty;

    /// <summary>
    /// Modify Region Class since screenline has a foreign key to region table. 
    /// </summary>
    public virtual Region Region { get; set; } = null!;
}
