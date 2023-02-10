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

using CCDRSManager.Data;
using CCDRSManager.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CCDRSManager;

/// <summary>
/// Class to provides access to the persistent storage?
/// </summary>
public partial class CCDRSManagerModelRepository
{
    private readonly CCDRSContext _context;
    private readonly ObservableCollection<RegionModel> _regionsModel;

    // Initialize the CCDRS class
    public CCDRSManagerModelRepository(CCDRSContext context)
    {
        _context = context;
        _regionsModel = new ObservableCollection<RegionModel>(_context.Regions.Select(r => new RegionModel(r)));
    }

    /// <summary>
    /// Property to get a list of all Regions that exist in the database.
    /// </summary>
    public ReadOnlyObservableCollection<RegionModel> Regions
    {
        get => new(_regionsModel);
    }

    /// <summary>
    /// Checks if the survey exists or not.
    /// </summary>
    public bool CheckSurveyExists(int regionId, int surveyYear)
    {
        // find one instance of the survey data
        return (from surveys in _context.Surveys
                join regions in _context.Regions on surveys.RegionId equals regions.Id
                where
                   regions.Id == regionId
                   && surveys.Year == surveyYear
                select
                  surveys
                ).Any();
    }

    /// <summary>
    /// Delete the survey data from a given survey region and year. 
    /// This deletes the data from the survey, surveystation and stationcountobservation tables.
    /// </summary>
    /// <param name="regionId">The serial primary key regionId.</param>
    /// <param name="surveyYear">The year of the survey.</param>
    public void DeleteSurveyData(int regionId, int surveyYear)
    {
        var dataList = (from surveys in _context.Surveys
                        join regions in _context.Regions on surveys.RegionId equals regions.Id
                        where
                            regions.Id == regionId
                            && surveys.Year == surveyYear
                        select
                            surveys).ToList();
        _context.Surveys.RemoveRange(dataList);
        _context.SaveChanges();
    }

    /// <summary>
    /// Add survey data to the database.
    /// </summary>
    /// <param name="regionId">Primary serial key of regionId as an integer.</param>
    /// <param name="surveyYear">Year of survey e.g. 2022.</param>
    public void AddSurveyData(int regionId, int surveyYear)
    {
        // Create a new survey object.
        Survey survey = new()
        {
            RegionId = regionId,
            Year = surveyYear
        };
        _context.Surveys.Add(survey);
        _context.SaveChanges();
    }

    /// <summary>
    /// Check if station exists in the database and only add new stations to the survey.
    /// </summary>
    /// <param name="regionId">Primary serial key of the region.</param>
    /// <param name="stationCode">StationCode number used to check if station exists in the database.</param>
    /// <param name="stationDescription">Description of station provided by agency.</param>
    public void AddStationIfNotExists(int regionId, string stationCode, string stationDescription)
    {
        // Check if stationCode exists in database.
        var stationExists = (from stations in _context.Stations
                             join regions in _context.Regions on stations.RegionId equals regions.Id
                             where
                               regions.Id == regionId
                               && stations.StationCode == stationCode.Trim()
                             select
                               stations).Any();

        // Add new stationCode to the stations context if stationExists return False.
        if (stationExists == false)
        {
            // Add new station code to the Stations context.
            Station newStation = new()
            {
                StationCode = stationCode,
                Description = stationDescription
            };
            _context.Stations.Add(newStation);
            _context.SaveChanges();
        }
    }

    /// <summary>
    /// Add station data to the database
    /// </summary>
    /// <param name="stationFileName">String path to station csv file.</param>
    /// <param name="regionId">Primary serial key of the region.</param>
    public void AddStationData(string stationFileName, int regionId)
    {
        // Loop through the station csv file
        using (var readFile = new StreamReader(stationFileName))
        {
            string? line;
            string[] row;
            readFile.ReadLine();
            // loop through the line
            while ((line = readFile.ReadLine()) is not null)
            {
                row = line.Split(';');
                // add station data
                AddStationIfNotExists(regionId, row[0], row[1]);
            }
        };
    }

    /// <summary>
    /// Add the surveys_station data of new survey to the database.
    /// </summary>
    /// <param name="regionId">Primary serial key of the region.</param>
    /// <param name="surveyYear">Year of survey e.g. 2022.</param>
    public void AddSurveyStationData(int regionId, int surveyYear)
    {
        //find the survey id 
        var surveyID = (from surveys in _context.Surveys
                        join regions in _context.Regions on surveys.RegionId equals regions.Id
                        where
                            regions.Id == regionId
                            && surveys.Year == surveyYear
                        select
                            surveys
                        ).First();

        //find stations list of all stations of specific region
        var stationList = (from stations in _context.Stations
                           join regions in _context.Regions on stations.RegionId equals regions.Id
                           where
                              regions.Id == regionId
                           select
                              stations).ToList();

        //combine the survey with the stationList to make the new surveystation context.
        foreach (var station in stationList)
        {
            SurveyStation ss = new()
            {
                StationId = station.Id,
                SurveyId = surveyID.Id
            };
            _context.SurveyStations.Add(ss);
        }
        //save the context to the database.
        _context.SaveChanges();
    }

    /// <summary>
    /// Checks if the vehicle type exists in the database
    /// </summary>
    /// <param name="header">Name of vehicle type as a string.</param>
    /// <param name="vehicleResult">A VehicleCountType object.</param>
    /// <returns>true if a VehicleCountType object is found else false.</returns>
    public bool TryGetVehicle(string header, [NotNullWhen(true)] out VehicleCountType? vehicleResult)
    {
        Regex re = MyRegex();
        Match regexValue = re.Match(header);

        // Regex match found so do two queries
        // One against the vehicle table and one against the vehicle_count_type table
        if (regexValue.Success)
        {
            // first check against the 
            string vehicleName = regexValue.Groups[1].Value;
            int vehicleNumber = int.Parse(regexValue.Groups[2].Value);

            // run the query to find the vehicle Id
            var vehicle = _context.Vehicles.Where(v => v.Name == vehicleName).FirstOrDefault();
            if (vehicle is null)
            {
                vehicleResult = null;
                return false;
            }
            vehicleResult = _context.VehicleCountTypes.Where(v => v.VehicleId == vehicle.Id && v.Occupancy == vehicleNumber).FirstOrDefault();
            return vehicleResult is not null;
        }
        else
        {
            // No regex match found so query the vehicle table to see if the vehicle exists.
            var vehicle = _context.Vehicles.Where(v => v.Name == header).FirstOrDefault();
            if (vehicle is null)
            {
                vehicleResult = null;
                return false;
            }
            vehicleResult = _context.VehicleCountTypes.Where(v => v.VehicleId == vehicle.Id && v.Occupancy == 1).FirstOrDefault();
            return vehicleResult is not null;
        }
    }

    /// <summary>
    /// Method which creates a new StationCountObservation object and appends the new items to the context.
    /// </summary>
    /// <param name="vehicleCountTypeList">List of all VehicleCountTypeObjects found in the file.</param>
    /// <param name="observation">The count observed for each VehicleCountType passed in as a list.</param>
    /// <param name="regionId">Primary serial key of region as int.</param>
    /// <param name="surveyYear">Year of survey e.g. 2016.</param>
    public void InsertDataIntoStationCountContext(List<VehicleCountType> vehicleCountTypeList, string[] observation, int regionId, int surveyYear)
    {
        // get the survey station data object
        var surveyStation = (from surveyStations in _context.SurveyStations
                             join stations in _context.Stations on surveyStations.StationId equals stations.Id
                             join region in _context.Regions on stations.RegionId equals region.Id
                             join survey in _context.Surveys on surveyStations.SurveyId equals survey.Id
                             where
                               region.Id == regionId
                               && survey.Year == surveyYear
                               && stations.StationCode == observation[0].Trim()
                             select
                               surveyStations
                      ).First();

        // Iterate over the list of all vehicle types
        foreach (var vehicleCountType in vehicleCountTypeList)
        {
            // Find the index of the vehicle_count_type object
            int ind = vehicleCountTypeList.IndexOf(vehicleCountType);
            if (observation[ind + 2] != "0")
            {
                StationCountObservation stationCountObservation = new()
                {
                    SurveyStationId = surveyStation.Id,
                    Time = Int32.Parse(observation[1]),
                    VehicleCountTypeId = vehicleCountType.Id,
                    Observation = Int32.Parse(observation[ind + 2])
                };
                _context.StationCountObservations.Add(stationCountObservation);
            }
        }

    }

    /// <summary>
    /// Add StationCountObservation data to the database.
    /// </summary>
    /// <param name="stationCountObservationFile">String filepath to the stationcount observation csv file.</param>
    /// <param name="regionId">Primary serial key of region.</param>
    /// <param name="surveyYear">Year of survey e.g.2016.</param>
    /// <param name="checkIfAdd">Function to check if the vehicle type exist in the database or not.</param>
    /// <exception cref="NotImplementedException"></exception>
    public void AddStationCountObservationData(string stationCountObservationFile, int regionId, int surveyYear, Func<string, bool>? checkIfAdd = null)
    {
        string[] headerLine;

        List<VehicleCountType> vehicleCountTypeList = new();

        // read the station_count_observation ccdrs csv file
        using (var readFile = new StreamReader(stationCountObservationFile))
        {
            string? line;
            string[] observationData;

            // Extract the header of the csv file which contains the columns of vehicle types.
            headerLine = readFile.ReadLine()?.Split(',') ?? throw new Exception("No header file found");
            
            // Loop through header to check if the vehicle exists in the database or not
            foreach (string technology in headerLine)
            {
                // Ignore if the header value is labeled station or time 
                if (technology == "station" || technology == "time")
                {
                    continue;
                }
                else
                {
                    if (TryGetVehicle(technology, out var vehicleCount))
                    {
                        vehicleCountTypeList.Add(vehicleCount);
                    }
                    else
                    {
                        if (checkIfAdd?.Invoke(technology) == true)
                        {
                            // ToDo: Need to implement.
                            // If user selects yes we add the technology to the vehicle_count_type
                        }
                        else
                        {
                            // ToDo: Need to implement
                            // if user selects no, don't add new technologies and gracefully abort.
                        }
                        throw new NotImplementedException();
                    }
                }
            }

            // Loop through the remaining rows of data and insert the observation data into the database.
            while ((line = readFile.ReadLine()) is not null)
            {
                observationData = line.Split(',');
                // check and add the station data
                InsertDataIntoStationCountContext(vehicleCountTypeList, observationData, regionId, surveyYear);
            }
        }
        // Update the individaul_categories table to display the correct technologies on webpage.
        UpdateIndividualCategoriesTable(regionId, surveyYear, vehicleCountTypeList);

        // Save the changes to the database.
        SaveData();
    }

    /// <summary>
    /// Update the individual_categories table with the correct technologies to display to the html 
    /// front end.
    /// </summary>
    /// <param name="regionId">Primary serial key of region as an int.</param>
    /// <param name="surveyYear">Year of survey e.g. 2016</param>
    /// <param name="vehicleCountTypes">List of VehicleCountTypes for which observations were collected for the given 
    /// specific survey.</param>
    public void UpdateIndividualCategoriesTable(int regionId, int surveyYear, List<VehicleCountType> vehicleCountTypes)
    {
        var region = _context.Regions.Where(r => r.Id == regionId).First();
        var survey = _context.Surveys.Where(s => s.Year == surveyYear && s.RegionId == regionId).First();

        // Delete the individual category of specific survey of data that was previously uploaded.
        List<IndividualCategory> individualCategories = (from individualcategories in _context.IndividualCategories
                                                         where
                                                             individualcategories.RegionId == regionId
                                                             && individualcategories.Year == surveyYear
                                                         select
                                                             individualcategories).ToList();
        _context.IndividualCategories.RemoveRange(individualCategories);
        _context.SaveChanges();

        // Iterate over the new vehicle_count_type list and build a new set of individual categories
        foreach (VehicleCountType vehicleCountType in vehicleCountTypes)
        {
            var vehicle = _context.Vehicles.Where(v => v.Id == vehicleCountType.VehicleId).First();
            IndividualCategory iC = new()
            {
                RegionId = region.Id,
                RegionName = region.Name,
                SurveyId = survey.Id,
                Year = survey.Year,
                CountType = vehicleCountType.CountType,
                Occupancy = vehicleCountType.Occupancy,
                Description = vehicleCountType.Description,
                VehicleCountTypeId = vehicleCountType.Id,
                VehicleName = vehicle.Name
            };
            _context.IndividualCategories.Add(iC);
        }
    }

    /// <summary>
    /// Save the changes to the context to the database when all context changes have finished.
    /// </summary>
    /// <returns></returns>
    public Task SaveData()
    {
        return _context.SaveChangesAsync();
    }

    [GeneratedRegex("([a-zA-Z]+)(\\d+)")]
    private static partial Regex MyRegex();
}
