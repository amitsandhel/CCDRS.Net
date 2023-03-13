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
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CCDRSManager;

/// <summary>
/// Class to provides access to the persistent storage?
/// </summary>
public partial class CCDRSManagerModelRepository
{
    private readonly CCDRSContext _context;
    private readonly ObservableCollection<RegionModel> _regionsModel;
    private readonly ObservableCollection<VehicleCountTypeModel> _vehiclesModel;

    // Initialize the CCDRS class
    public CCDRSManagerModelRepository(CCDRSContext context)
    {
        _context = context;
        _regionsModel = new ObservableCollection<RegionModel>(_context.Regions.Select(r => new RegionModel(r)));
        _vehiclesModel = new ObservableCollection<VehicleCountTypeModel>(_context.VehicleCountTypes.Select(r => new VehicleCountTypeModel(r)));
    }

    /// <summary>
    /// Property to get a list of all Regions that exist in the database.
    /// </summary>
    public ReadOnlyObservableCollection<RegionModel> Regions
    {
        get => new(_regionsModel);
    }

    public ObservableCollection<VehicleCountTypeModel> VehicleCountTypeModels
    {
        get => new(_vehiclesModel);
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
                ).AsNoTracking().Any();
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
        // Save the context to the database.
        Save();
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
        // Save the context to the database.
        Save();
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
            // Save the context to the database.
            Save();
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
            int lineNumber = 0;

            // loop through the line
            while ((line = readFile.ReadLine()) is not null)
            {
                lineNumber++;
                try
                {
                    row = line.Split(';');
                    // add station data
                    AddStationIfNotExists(regionId, row[0], row[1]);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Corrupt data in file {stationFileName} on line {lineNumber} \n" + ex);
                }
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
        // Save the context to the database.
        Save();
    }

    /// <summary>
    /// Method to check if vehicle object exists in the database.
    /// </summary>
    /// <param name="vehicleName">Name of vehicle as a string to search against in the database.</param>
    /// <returns></returns>
    public Vehicle? GetVehicleIfExists(string vehicleName)
    {
        return _context.Vehicles.Where(v => v.Name == vehicleName).FirstOrDefault();
    }

    /// <summary>
    /// Checks if a vehicle_count_type object exists based on a given vehicle id and occupancy number.
    /// </summary>
    /// <param name="vehicleId">Primary key of vehicle.</param>
    /// <param name="occupancy">Number of occupants that can sit in vehicle.</param>
    /// <returns>A VehicleCountType object.</returns>
    public VehicleCountType? GetVehicleCountTypeIfExists(int vehicleId, int occupancy)
    {
        return _context.VehicleCountTypes.Where(v => v.VehicleId == vehicleId && v.Occupancy == occupancy).FirstOrDefault();
    }

    /// <summary>
    /// Add a new vehicle into the database.
    /// </summary>
    /// <param name="vehicleName">Name of vehicle object to add.</param>
    /// <returns>a Vehicle object.</returns>
    public Vehicle AddVehicle(string vehicleName)
    {
        Vehicle vehicle = new()
        {
            Name = vehicleName
        };
        _context.Vehicles.Add(vehicle);
        Save();
        return vehicle;
    }

    /// <summary>
    /// Add a new VehicleCountType entry into the database.
    /// </summary>
    /// <param name="newOccupancyNumber">Number of occupants that can sit in new vehicle as an int.</param>
    /// <param name="newVehicleText">Name of new vehicle object e.g. auto1</param>
    /// <param name="vehicleId">Primary key of vehicle object.</param>
    /// <returns>A vehicleCountType object</returns>
    public VehicleCountType AddNewVehicleCountType(int newOccupancyNumber, string newVehicleText, int vehicleId)
    {
        VehicleCountType vehicleCountType = new()
        {
            Occupancy = newOccupancyNumber,
            Description = newVehicleText,
            CountType = 1,
            VehicleId = vehicleId
        };
        _context.VehicleCountTypes.Add(vehicleCountType);
        //_context.SaveChanges();
        Save();
        return vehicleCountType;
    }

    /// <summary>
    /// Ensures if the vehicle type exists in the database
    /// </summary>
    /// <param name="header">Name of vehicle type as a string.</param>
    /// <param name="vehicleResult">A VehicleCountType object.</param>
    /// <returns>true if a VehicleCountType object is found else false.</returns>
    public VehicleCountType GetVehicleCountTypeObject(string header)
    {
        Vehicle? vehicle;
        VehicleCountType? vehicleCountTypeResult;

        // Regex to split the technology at the number if applicable.
        Regex re = MyRegex();
        Match regexValue = re.Match(header);

        // Regex match found so do two queries
        if (regexValue.Success)
        {
            string vehicleName = regexValue.Groups[1].Value;
            int vehicleNumber = int.Parse(regexValue.Groups[2].Value);

            // Check to see if vehicle object exists.
            vehicle = GetVehicleIfExists(vehicleName);

            // If the vehicle doesn't exist in the database create a new vehicle and associated
            // vehicle_count_type entry in the database otherwise check if the vehicle_count_type exists 
            // in the database and if it doesn't create a new vehicle_count_type entry or return the entry
            // in the database.
            if (vehicle is null)
            {
                // New vehicle found so add the new vehicle to the database.
                Vehicle newVehicle = AddVehicle(header);

                // Create a new vehicle_count_type entry in the database associated with the new vehicle.
                return AddNewVehicleCountType(vehicleNumber, header, newVehicle.Id);
            }
            else
            {
                // Vehicle already exists in the database check if an associated
                // vehicle_count_type object with given occupancy number exists in the database.
                vehicleCountTypeResult = GetVehicleCountTypeIfExists(vehicle.Id, vehicleNumber);

                return vehicleCountTypeResult ?? AddNewVehicleCountType(vehicleNumber, header, vehicle.Id);
            }
        }
        else
        {
            // No regex match found so query the vehicle table to see if vehicle exists.
            vehicle = GetVehicleIfExists(header);

            // If vehicle object is null create a new vehicle object and the associated vehicle_count_object
            // otherwise return vehicleCountType object existing in the database.
            if (vehicle is null)
            {
                Vehicle newVehicleObject = AddVehicle(header);
                return AddNewVehicleCountType(1, header, newVehicleObject.Id);
            }
            else
            {
                // successfully returned a vehicle object now return the associated vehicle_count_type object
                // which exists.
                vehicleCountTypeResult = GetVehicleCountTypeIfExists(vehicle.Id, 1);
                return vehicleCountTypeResult ?? AddNewVehicleCountType(1, header, vehicle.Id);
            }
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
            //ignore adding in data that is zero as we don't add zero values to the database.
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
    /// <param name="stationCountObservationFile">String filepath to the stationcount observation csv file</param>
    /// <param name="regionId">Primary serial key of region.</param>
    /// <param name="surveyYear">Year of survey e.g.2016.</param>
    /// <exception cref="Exception"></exception>
    public void AddStationCountObservationData(string stationCountObservationFile, int regionId, int surveyYear)
    {
        // extract the header of the ccdrs file.
        string[] headerLine;

        List<VehicleCountType> vehicleCountTypeList = new();

        // read the station_count_observation ccdrs csv file
        using (var readFile = new StreamReader(stationCountObservationFile))
        {
            int lineNumber = 0;
            try
            {
                string? line;
                string[] observationData;

                // Extract the header of the csv file which contains the columns of vehicle types.
                headerLine = readFile.ReadLine()?.Split(',') ?? throw new Exception("No header file found");

                // Loop through header to check if the vehicle exists in the database or not
                foreach (string technology in headerLine)
                {
                    if (!(technology == "station" || technology == "time"))
                    {
                        // get the VehicleCountTypeObject and add to the list.
                        VehicleCountType result = GetVehicleCountTypeObject(technology);
                        vehicleCountTypeList.Add(result);
                    }
                }

                // Loop through the remaining rows of data and insert the observation data into the database.
                while ((line = readFile.ReadLine()) is not null)
                {

                    lineNumber++;
                    observationData = line.Split(',');
                    // check and add the station data
                    InsertDataIntoStationCountContext(vehicleCountTypeList, observationData, regionId, surveyYear);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Corrupt data in file {stationCountObservationFile} on line {lineNumber} \n" + ex);
            }

        }
        // Update the individaul_categories table to display the correct technologies on webpage.
        UpdateIndividualCategoriesTable(regionId, surveyYear, vehicleCountTypeList);

        // Save the context to the database.
        Save();
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

        // Save the context to the database.
        Save();

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
    /// Delete all screenline and ScreenlineStation data for specified regionId.
    /// </summary>
    /// <param name="regionId">Primary serial key of region.</param>
    public void DeleteScreenline(int regionId)
    {
        List<Screenline> dataList = _context.Screenlines.Where(s => s.RegionId == regionId).ToList();
        _context.Screenlines.RemoveRange(dataList);
        // Save the context to the database.
        Save();
    }

    /// <summary>
    /// Insert data into the screenline table.
    /// </summary>
    /// <param name="regionId">Primary serial key of region.</param>
    /// <param name="screenlineData">List of data of a screenline containing screenline code, description and associated station.</param>
    public void InsertDataIntoScreenline(int regionId, string slineCode, string slineDescription)
    {
        if (!_context.Screenlines.Any(s => s.RegionId == regionId && s.SlineCode == slineCode))
        {
            // Make a new screenline object
            Screenline newScreenline = new()
            {
                RegionId = regionId,
                SlineCode = slineCode,
                Note = slineDescription
            };
            _context.Screenlines.Add(newScreenline);
            // Save the context to the database.
            Save();
        }
    }

    /// <summary>
    /// Insert data into the ScreenlineStation table.
    /// </summary>
    /// <param name="regionId">Primary serial key of region.</param>
    /// <param name="screenlineData">List of data of a screenline containing screenline code, description and associated station.</param>
    public void AddScreenlineStationData(int regionId, string slineCode, string stationCode)
    {
        // find the Screenline object.
        Screenline? screenline = _context.Screenlines.FirstOrDefault(s => s.RegionId == regionId && s.SlineCode == slineCode);

        // Find the station object in the data.
        Station? station = _context.Stations.FirstOrDefault(s => s.StationCode == stationCode && s.RegionId == regionId);

        if (station is not null)
        {
            // Create a new ScreenlineStation object.
            ScreenlineStation newScreenlineStation = new()
            {
                ScreenlineId = screenline.Id,
                StationId = station.Id
            };
            _context.ScreenlineStations.Add(newScreenlineStation);
            // Save the context to the database.
            Save();
        }
    }

    /// <summary>
    /// Parse screenline csv file and insert data into the Screenline and ScreenlineStation tables.
    /// </summary>
    /// <param name="regionId">Primary serial key of region.</param>
    /// <param name="screenlineFileName">Filepath to the location of the screenline csv file.</param>
    public void AddScreenlineData(int regionId, string screenlineFileName)
    {
        // Delete all the screenline data of provided region.
        DeleteScreenline(regionId);

        // read the screenline csv file
        using var readFile = new StreamReader(screenlineFileName);
        string? line;
        string[] observationData;

        // Loop through the remaining rows of data and insert the screenline data into the database.
        while ((line = readFile.ReadLine()) is not null)
        {
            observationData = line.Split(',');
            string slineCode = observationData[0];
            string slineDescription = observationData[1];
            string stationCode = observationData[2];
            // insert data into Screenline table.
            InsertDataIntoScreenline(regionId, slineCode, slineDescription);
            // insert data into the ScreenlineStation table.
            AddScreenlineStationData(regionId, slineCode, stationCode);
        }
    }

    /// <summary>
    /// Property to get a list of all VehicleCountType objects that exist in the database.
    /// </summary>
    public ObservableCollection<VehicleCountTypeModel> Vehicles
    {
        get => new(_vehiclesModel);
    }

    /// <summary>
    /// Get the VehicleCount Type object based on user specified id.
    /// </summary>
    /// <param name="vehicleCountTypeId"></param>
    /// <returns></returns>
    public VehicleCountType GetVehicleCountTypeData(int vehicleCountTypeId)
    {
        return _context.VehicleCountTypes.Where(v => v.Id == vehicleCountTypeId).First();
    }

    /// <summary>
    /// Get the Vehicle object user specified.
    /// </summary>
    /// <param name="vehicleId"></param>
    /// <returns></returns>
    public Vehicle GetVehicleData(int vehicleId)
    {
        return _context.Vehicles.Where(v => v.Id == vehicleId).First();
    }

    /// <summary>
    /// Update the VehicleCountType object in the datbase with the new user provided values.
    /// </summary>
    /// <param name="selectedVehicleCountType">VehicleCountType object to update.</param>
    /// <param name="selectedVehicle">Vehicle object of selected VehicleCountType object.</param>
    /// <param name="occupancyNumber">New occupancy number.</param>
    /// <param name="countType">New counttype.</param>
    /// <param name="vehicleDescription">New vehicle description.</param>
    /// <param name="vehicleName">New vehicle name.</param>
    public void UpdateVehicleData(VehicleCountType selectedVehicleCountType, Vehicle selectedVehicle, int occupancyNumber, int countType, string vehicleDescription, string vehicleName)
    {
        var item = _context.VehicleCountTypes.Find(selectedVehicleCountType.Id);
        item.GetType().GetProperty("Description").SetValue(item, vehicleDescription);
        item.GetType().GetProperty("Occupancy").SetValue(item, occupancyNumber);
        item.GetType().GetProperty("CountType").SetValue(item, countType);
        _context.SaveChanges();
    }

    /// <summary>
    /// Rebuild the VehicleCountTypeModel list to update the combo box.
    /// </summary>
    /// <returns></returns>
    public ObservableCollection<VehicleCountTypeModel> RebuildComboBox()
    {
        return new ObservableCollection<VehicleCountTypeModel>(_context.VehicleCountTypes.Select(r => new VehicleCountTypeModel(r)));
    }

    /// <summary>
    /// Save the changes to the context to the database when all context changes have finished.
    /// </summary>
    public void Save()
    {
        _context.SaveChanges();
        ClearChangeTracker();
    }

    /// <summary>
    /// Clear all tracked entities since we can't dispose the context at the end of each unit of work.
    /// </summary>
    public void ClearChangeTracker()
    {
        _context.ChangeTracker.Clear();
    }

    [GeneratedRegex("([a-zA-Z]+)(\\d+)")]
    private static partial Regex MyRegex();
}
