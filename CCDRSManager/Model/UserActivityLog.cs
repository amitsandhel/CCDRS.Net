/*
    Copyright 2024 University of Toronto
    This file is part of IDRS.
    IDRS is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    IDRS is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with CCDRS.  If not, see <http://www.gnu.org/licenses/>.
*/

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace CCDRSManager.Model;

[Keyless]
[Table("useractivitylog")]
public partial class UserActivityLog
{
    /// <summary>
    /// Primary key of the year table.
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Username who ran the query. This is the accout email address.
    /// </summary>
    [Column("username")]
    public string UserName { get; set; }

    /// <summary>
    /// Datetime of when the query was executed by the user.
    /// </summary>
    [Column("logindaytime", TypeName = "timestamp without time zone")]
    public DateTime? Logindaytime { get; set; }

    /// <summary>
    /// Selected user page e.g. allstations, allscreenlines.
    /// </summary>
    [Column("pagetype")]
    public string? PageType { get; set; }

    /// <summary>
    /// Calculation type e.g. fifteen minutes or total volume
    /// </summary>
    [Column("calculationtimeperiod")]
    public string? CalculationTimePeriod { get; set; }

    /// <summary>
    /// User selected Region e.g. Toronto
    /// </summary>
    [Column("region")]
    public string? Region { get; set; }

    /// <summary>
    /// Year of selected query e.g. 2016, 2022.
    /// </summary>
    [Column("year")]
    public int Year { get; set; }
}
