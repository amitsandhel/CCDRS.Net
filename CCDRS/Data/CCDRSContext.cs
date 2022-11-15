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
using CCDRS.Model;
using Microsoft.EntityFrameworkCore;
namespace CCDRS.Data;

public partial class CCDRSContext : DbContext
{
    public CCDRSContext()
    {
    }

    public CCDRSContext(DbContextOptions<CCDRSContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Allow pages to access the Direction class as a service.
    /// </summary>
    public virtual DbSet<Direction> Directions { get; set; }

    /// <summary>
    /// Allow pages to access Region class as a service.
    /// </summary>
    public virtual DbSet<Region> Regions { get; set; }

    /// <summary>
    /// Allow pages to access survey class as a service.
    /// </summary>
    public virtual DbSet<Survey> Surveys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ///<summary>
        ///Association of Direction class to direction database attributes
        /// </summary>
        modelBuilder.Entity<Direction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("direction_pkey");

            entity.ToTable("direction");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Abbreviation)
                .HasMaxLength(1)
                .HasColumnName("abbreviation");
            entity.Property(e => e.Compass).HasColumnName("compass");
        });

        ///<summary>
        ///Association of Region class to region database attributes
        /// </summary>
        modelBuilder.Entity<Region>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("region_pkey");

            entity.ToTable("region");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        ///<summary>
        ///Association of Survey class to survey database attributes
        /// </summary>
        modelBuilder.Entity<Survey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("survey_pkey");

            entity.ToTable("survey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RegionId).HasColumnName("region_id");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.Region).WithMany(p => p.Surveys)
                .HasForeignKey(d => d.RegionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("survey_region_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
