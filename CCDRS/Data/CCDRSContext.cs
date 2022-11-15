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

    /// <summary>
    /// Allow pages to access Vehicle class as a service.
    /// </summary>
    public virtual DbSet<Vehicle> Vehicles { get; set; }

    /// <summary>
    /// Allow pages to access VehicleCountType class as a service.
    /// </summary>
    public virtual DbSet<VehicleCountType> VehicleCountTypes { get; set; }

    /// <summary>
    /// Allow pages to access Station class as a service.
    /// </summary>
    public virtual DbSet<Station> Stations { get; set; }

    /// <summary>
    /// Allow pages to access Screenline class as a service.
    /// </summary>
    public virtual DbSet<Screenline> Screenlines { get; set; }

    /// <summary>
    /// Allow pages to access SurveyStation class as a service.
    /// </summary>
    public virtual DbSet<SurveyStation> SurveyStations { get; set; }

    /// <summary>
    /// Allow pages to access StationCountObservation as a service.
    /// </summary>
    public virtual DbSet<StationCountObservation> StationCountObservations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Association of Direction class to direction database attributes
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

        // Association of Region class to region database attributes
        modelBuilder.Entity<Region>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("region_pkey");

            entity.ToTable("region");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        // Association of Survey class to survey database attributes
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

        // Association of Vehicle class to vehicle database attributes.
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("vehicle_pkey");

            entity.ToTable("vehicle");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        // Association of VehicleCountType class to vehicle_count_type database attributes.
        modelBuilder.Entity<VehicleCountType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("vehicle_count_type_pkey");

            entity.ToTable("vehicle_count_type");

            entity.HasIndex(e => new { e.VehicleId, e.Occupancy }, "vehicle_count_type_vehicle_id_occupancy_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CountType).HasColumnName("count_type");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Occupancy).HasColumnName("occupancy");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.VehicleCountTypes)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("vehicle_count_type_vehicle_id_fkey");
        });

        // Association of Station class to station database attributes.
        modelBuilder.Entity<Station>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("station_pkey");

            entity.ToTable("station");

            entity.HasIndex(e => new { e.RegionId, e.StationCode }, "station_region_id_station_code_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Direction)
                .HasMaxLength(1)
                .HasColumnName("direction");
            entity.Property(e => e.RegionId).HasColumnName("region_id");
            entity.Property(e => e.StationCode).HasColumnName("station_code");
            entity.Property(e => e.StationNum).HasColumnName("station_num");

            entity.HasOne(d => d.Region).WithMany(p => p.Stations)
                .HasForeignKey(d => d.RegionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("station_region_id_fkey");
        });

        // Association of Screenline class to screenline database attributes.
        modelBuilder.Entity<Screenline>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("screenline_pkey");

            entity.ToTable("screenline");

            entity.HasIndex(e => new { e.RegionId, e.SlineCode }, "screenline_region_id_sline_code_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.RegionId).HasColumnName("region_id");
            entity.Property(e => e.SlineCode).HasColumnName("sline_code");

            entity.HasOne(d => d.Region).WithMany(p => p.Screenlines)
                .HasForeignKey(d => d.RegionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("screenline_region_id_fkey");
        });

        // Association of SurveyStation to survey_station database attributes
        modelBuilder.Entity<SurveyStation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("survey_station_pkey");

            entity.ToTable("survey_station");

            entity.HasIndex(e => new { e.StationId, e.SurveyId }, "survey_station_station_id_survey_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.StationId).HasColumnName("station_id");
            entity.Property(e => e.SurveyId).HasColumnName("survey_id");

            entity.HasOne(d => d.Station).WithMany(p => p.SurveyStations)
                .HasForeignKey(d => d.StationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("survey_station_station_id_fkey");

            entity.HasOne(d => d.Survey).WithMany(p => p.SurveyStations)
                .HasForeignKey(d => d.SurveyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("survey_station_survey_id_fkey");
        });

        // Association of StationCountObservation to station_count_observation attributes.
        modelBuilder.Entity<StationCountObservation>(entity =>
        {
            entity.HasKey(e => new { e.Time, e.SurveyStationId, e.VehicleCountTypeId }).HasName("station_count_observation_pkey");

            entity.ToTable("station_count_observation");

            entity.Property(e => e.Time).HasColumnName("time");
            entity.Property(e => e.SurveyStationId).HasColumnName("survey_station_id");
            entity.Property(e => e.VehicleCountTypeId).HasColumnName("vehicle_count_type_id");
            entity.Property(e => e.Observation).HasColumnName("observation");

            entity.HasOne(d => d.SurveyStation).WithMany(p => p.StationCountObservations)
                .HasForeignKey(d => d.SurveyStationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("station_count_observation_survey_station_id_fkey");

            entity.HasOne(d => d.VehicleCountType).WithMany(p => p.StationCountObservations)
                .HasForeignKey(d => d.VehicleCountTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("station_count_observation_vehicle_count_type_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
