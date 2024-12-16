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

using CCDRS.Model;
using Microsoft.EntityFrameworkCore;

namespace CCDRS;

/// <summary>
/// Contains cached data. Call Initialize before usage.
/// </summary>
public static class Utility
{
    /// <summary>
    /// Private method to generate a list of vehicles and associated occupancy. 
    /// </summary>
    /// <param name="context">The CCDRS context service</param>
    /// <returns>A list of the query with the vehicle id, name and occupancy</returns>
    private static List<(int id, string name)> GenerateNumberOfOccupantsInVehicle(CCDRS.Data.CCDRSContext context)
    {
        return (from vehicle in context.Vehicles
                join vehicleCount in context.VehicleCountTypes on vehicle.Id equals vehicleCount.VehicleId
                select new
                {
                    Id = vehicleCount.Id,
                    Name = vehicle.Name + vehicleCount.Occupancy
                })
               // The first ToList finishes the logic for EF,
               // then we select a value tuple from that since EF can't use ValueTuples
               .ToList()
               .Select(x => (x.Id, x.Name))
               .ToList();
    }

    /// <summary>
    /// List of tuple containing the id and new name of technology with vehicle and occupancy number
    /// </summary>
    public static List<(int id, string name)> TechnologyNames { get; private set; } = null!;

    /// <summary>
    /// List of Directions user may select from. Accessible by other pages
    /// </summary>
    public static List<Direction> Directions { get; private set; } = null!;

    /// <summary>
    /// Function to run the database query to return a list of all directions available
    /// </summary>
    /// <param name="context">The CCDRSContet service</param>
    /// <returns>A list of the Direction objects</returns>
    private static List<Direction> GenerateDirections(CCDRS.Data.CCDRSContext context)
    {
        return context.Directions.ToList();
    }

    /// <summary>
    /// List of IndividualCategory objects
    /// </summary>
    public static IList<IndividualCategory> IndividualCategories { get; private set; } = null!;

    /// <summary>
    /// Method to call the database and run the query and return a list of all rows of individual categories
    /// </summary>
    /// <param name="context">The CCDRSContet service</param>
    /// <returns>A list of IndividualCategory Objects</returns>
    private static List<IndividualCategory> GenerateIndividualCategories(CCDRS.Data.CCDRSContext context)
    {
        return context.IndividualCategories.ToList();
    }

    /// <summary>
    /// Invoke this method to initialize cached data.
    /// </summary>
    /// <param name="context">The CCDRS context service</param>
    public static void Initialize(CCDRS.Data.CCDRSContext context)
    {
        TechnologyNames = GenerateNumberOfOccupantsInVehicle(context);
        Directions = GenerateDirections(context);
    }

    /// <summary>
    /// Method to convert DMG Time to minutes
    /// </summary>
    /// <param name="DMGTime">the DMG Start Time</param>
    /// <returns>An integer of the minutes</returns>
    public static int FromDMGTimeToMinutes(int DMGTime)
    {
        return (((int)(DMGTime / 100) * 60) + (DMGTime % 100));
    }

    /// <summary>
    /// Convert minutes to DMG TIME
    /// </summary>
    /// <param name="startTimeMinutes"></param>
    /// <returns></returns>
    internal static int MinutesToDMGTime(int startTimeMinutes)
    {
        return ((startTimeMinutes / 60) * 100) + (startTimeMinutes % 60);
    }

    /// <summary>
    /// public function to calculate the start time. First converts DMGTIme into minutes
    /// Subtracts 14 minutes to get the start time and then does the reverse and converts 
    /// the minutes back into DMGTime
    /// </summary>
    /// <param name="dmgTime">the input DMGTime that exists in the database </param>
    /// <returns>the start time in DMGTime</returns>
    public static int CalculateStartTime(int dmgTime)
    {
        // static integer value of the number of minutes to subtract to get the start time
        const int minutesToSubtract = 14;
        int res = FromDMGTimeToMinutes(dmgTime) - minutesToSubtract;
        return MinutesToDMGTime(res);
    }

    /// <summary>
    /// Get the stations that were actually counted in a given CCDRS survey.
    /// </summary>
    /// <param name="context">instance of DB context service.</param>
    /// <param name="regionId">int of selected regionId e.g. Toronto.</param>
    /// <param name="surveyId">int of selected surveyId.</param>
    /// <param name="slineCode">string of screenline.</param>
    /// <param name="direction">direction of the selected station belonging to given screenline.</param>
    /// <returns>an int count of the number of stations that were counted for selected CCDRS survey.</returns>
    public static int GetStationCount(CCDRS.Data.CCDRSContext context, int regionId, int surveyId, string slineCode, char direction)
    {
        var count = (from regions in context.Regions
                     join surveys in context.Surveys on regions.Id equals surveys.RegionId
                     join stations in context.Stations on regions.Id equals stations.RegionId
                     join screenlines in context.Screenlines on regions.Id equals screenlines.RegionId
                     join screenlinStations in context.ScreenlineStations on
                     new { ScreenlineId = screenlines.Id, StationId = stations.Id }
                     equals new { ScreenlineId = screenlinStations.ScreenlineId, StationId = screenlinStations.StationId }
                     join surveystations in context.SurveyStations
                     on new { StationId = stations.Id, SurveyId = surveys.Id }
                     equals new { StationId = surveystations.StationId, SurveyId = surveystations.SurveyId }
                     join stncounts in context.StationCountObservations on surveystations.Id equals stncounts.SurveyStationId
                     where
                     regions.Id == regionId
                     && surveys.Id == surveyId
                     && screenlines.SlineCode == slineCode
                     & stations.Direction == direction
                     group new { stations }
                     by new { stations.Id }
                   into newgrp
                     select new
                     {
                         Station = newgrp.Key.Id
                     }
                     ).Distinct().Count();

        return count;
    }

    /// <summary>
    /// Query to add the executed query to the database for logging purposes.
    /// </summary>
    /// <param name="context">DbContext Instance.</param>
    /// <param name="username">user email address.</param>
    /// <param name="pageType">Integer of user selected page e.g. AllStations, etc...</param>
    /// <param name="calculationType">Type of calculation user selected e.g. Total volume or fifteen minute.</param>
    /// <param name="region">User selected region of CCDRS survey.</param>
    /// <param name="year">User selected year of CCDRS Survey.</param>
    public static void WriteToUserActivityLog(CCDRS.Data.CCDRSContext context, string username, int pageType, 
        int calculationType, string region, int year)
    {
        string page;
        string calculationTimePeriod;

        // switch to determine which page user queried.
        switch (pageType)
        {
            case 1:
                page =  "AllStations";
                break;
            case 2:
                page = "AllScreenlines";
                break;
            case 3:
                page = "SpecificStation";
                break;
            case 4:
                page = "SpecificScreenline";
                break;
            default:
                page = "Index";
                break;
        }

        // switch to determine which calculation
        switch(calculationType)
        {
            case 1:
                calculationTimePeriod = "TotalVolume";
                break;
            case 2:
                calculationTimePeriod = "FifteenMinuteInterval";
                break;
            default:
                calculationTimePeriod = "";
                break;
        }

        UserActivityLog userActivityLog = new()
        {
            UserName = username,
            Logindaytime = DateTime.Now,
            PageType = page,
            CalculationTimePeriod = calculationTimePeriod,
            Region = region,
            Year = year
        };
        context.UserActivityLogs.Add(userActivityLog);
        context.SaveChanges();
        context.ChangeTracker.Clear();
    }

}
