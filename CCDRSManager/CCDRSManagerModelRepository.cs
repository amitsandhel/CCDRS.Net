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
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CCDRSManager;

/// <summary>
/// Class to provides access to the persistent storage?
/// </summary>
public class CCDRSManagerModelRepository
{
    private readonly CCDRSContext _context;
    private ObservableCollection<RegionModel> _regionsModel;

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
        get => new ReadOnlyObservableCollection<RegionModel>(_regionsModel);
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
    /// Save the changes to the context to the database when all context changes have finished.
    /// </summary>
    /// <returns></returns>
    public async Task SaveData()
    {
        _context.SaveChangesAsync();
    }

    /// <summary>
    /// Add survey data to the database.
    /// </summary>
    /// <param name="regionId">Primary serial key of regionId as an integer.</param>
    /// <param name="surveyYear">Year of survey e.g. 2022.</param>
    public void AddSurveyData(int regionId, int surveyYear)
    {
        // Create a new survey object.
        Survey survey = new Survey();
        survey.RegionId = regionId;
        survey.Year = surveyYear;
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
                                && stations.StationCode == stationCode
                              select
                                stations).Any();

        // Add new stationCode to the stations context if stationExists return False.
        if (stationExists == false)
        {
            // Add new station code to the Stations context.
            Station newStation = new Station();
            newStation.StationCode = stationCode;
            newStation.Description = stationDescription;
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
        string[] data;
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
    /// Add the surveysstation data of new survey to the database.
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
                        ).FirstOrDefault();

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
            SurveyStation ss = new SurveyStation();
            ss.StationId = station.Id;
            ss.SurveyId = surveyID.Id;
            _context.SurveyStations.Add(ss);
        }
        //save the context to the database.
        _context.SaveChanges();
    }
}
