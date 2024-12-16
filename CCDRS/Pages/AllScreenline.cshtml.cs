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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;

namespace CCDRS.Pages
{
    /// <summary>
    /// Class to display the data for the All Screenline page.
    /// </summary>
    public class AllScreenlineModel : PageModel
    {
        private readonly CCDRS.Data.CCDRSContext _context;

        public AllScreenlineModel(CCDRS.Data.CCDRSContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Initialize dropdown list of directions used to store user selected directions.
        /// </summary>
        [BindProperty] public IList<Direction> DirectionList { get; set; } = null!;

        /// <summary>
        /// Initialize list of vehicle count types used to store user selected vehicle count options.
        /// </summary>
        [BindProperty]
        public IList<IndividualCategory> VehicleCountTypeList { get; set; } = default!;

        /// <summary>
        /// Initialize list of person count types used to store user selected person count options.
        /// </summary>
        [BindProperty]
        public IList<IndividualCategory> PersonCountTypeList { get; set; } = default!;

        /// <summary>
        /// Initialize list of technologies available used to store user selected technologies
        /// </summary>
        [BindProperty]
        public IList<IndividualCategory> IndividualCategoriesList { get; set; } = default!;

        /// <summary>
        /// Set a global default variable to save the variable id this reads the value from the url
        /// The primary key of the selected region
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public int RegionId { get; set; }

        /// <summary>
        /// The user selected survey id read from the url.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public int SelectedSurveyId { get; set; }


        /// <summary>
        /// Display the data on page load
        /// </summary>
        /// <returns>The html page with the populated data</returns>
        public void OnGet()
        {
            // configure session 
            HttpContext.Session.SetString("Username", User.Identity.Name);

            // Query region table to display the region name to the front end.
            var regionName = _context.Regions
                              .Where(r => r.Id == RegionId)
                              .SingleOrDefault();

            // Bind the local variable to the ViewData to display to the front-end
            ViewData["RegionName"] = regionName?.Name;

            // Query survey table to display the survey year to the front end.
            var surveyYear = _context.Surveys
                              .Where(s => s.Id == SelectedSurveyId)
                              .SingleOrDefault();

            // Bind the local variable to the ViewData to display to the front-end
            ViewData["SurveyYear"] = surveyYear?.Year;

            // Bind the survey notes to the viewdata to display the notes to the front-end page.
            ViewData["SurveyNotes"] = surveyYear?.Notes;

            // Query directions table to return direction radiobutton options.
            if (_context.Directions != null)
            {
                DirectionList = Utility.Directions;
            }

            // Query individual_categories table to return various radiobutton options.
            if (_context.IndividualCategories != null)
            {
                // List all the unique technologies available for a given survey.
                IList<IndividualCategory> technologies = IndividualCategory.GetIndividualCategoriesBasedOnSurvey(SelectedSurveyId, _context);

                // List all total vehicle count options
                VehicleCountTypeList = IndividualCategory.GetTotalVehicleCounts(technologies);

                //// List all total person count options
                PersonCountTypeList = IndividualCategory.GetTotalPersonCounts(technologies);

                // List all technologies options
                IndividualCategoriesList = IndividualCategory.GetTechnologyCounts(technologies);
            }
        }

        /// <summary>
        /// Post method that executes database query to generate plain/txt format output
        /// </summary>
        /// <returns>Redirects user to a text file page with the results from executed query.</returns>
        public IActionResult OnPostSubmit(char[] directionCountSelect,
            int startTime, int endTime,
            int trafficVolumeRadioButtonSelect,
            int[] individualCategorySelect, IList<IndividualCategory> individualCategoriesList
            )
        {
            // run the query to get a region name and survey year to display in the content header file
            // Query region table to display the region name to the front end.
            var regionName = _context.Regions
                              .Where(r => r.Id == RegionId)
                              .SingleOrDefault();

            // Query survey table to display the survey year to the front end.
            var surveyYear = _context.Surveys
                              .Where(s => s.Id == SelectedSurveyId)
                              .SingleOrDefault();

            var builder = new StringBuilder();


            // Access session data
            string username = HttpContext.Session.GetString("Username");

            // User selects total volume.
            if (trafficVolumeRadioButtonSelect == 1)
            {
                // Build the header of the content file
                builder.Append(regionName?.Name ?? "Unknown Region");
                builder.Append(' ');
                builder.Append(surveyYear?.Year);
                builder.AppendLine();

                // User selects Total Volume
                builder.Append(GetTotalVolume(directionCountSelect,
                    startTime, endTime,
                 individualCategorySelect, individualCategoriesList
                 ));

                // log the information to the data and system.
                Utility.WriteToUserActivityLog(_context, username, 2, 1, regionName.Name, surveyYear.Year);

                return Content(builder.ToString());
            }
            else
            {
                // Build the header of the content file
                builder.Append(regionName?.Name ?? "Unknown Region");
                builder.Append(' ');
                builder.Append(surveyYear?.Year);
                builder.AppendLine();

                // User selects fifteen minute interval.
                builder.Append(GetFifteenMinuteInterval(directionCountSelect, 
                    startTime,endTime, 
                 individualCategorySelect, individualCategoriesList
                 ));

                // log the information to the data and system.
                Utility.WriteToUserActivityLog(_context, username, 2, 2, regionName.Name, surveyYear.Year);

                return Content(builder.ToString());
            }
        }

        /// <summary>
        /// Method to calculate the Fifteen Minute Interval results.
        /// </summary>
        /// <param name="startTime">Starting time user requested which is the start of the range</param>
        /// <param name="endTime">Ending time user requested which is the end of the range</param>
        /// <param name="individualCategorySelect">List of all categories selected by user</param>
        /// <param name="individualCategoriesList">Default list of all categories available for selected survey</param>
        /// <returns>String representation of the results</returns>
        internal string GetFifteenMinuteInterval(char[] directionCountSelect,
            int startTime, int endTime,
            int[] individualCategorySelect, IList<IndividualCategory> individualCategoriesList
        )
        {
            // Dictionary of station_count records with a key of the tuple of screenline name, time, direction and an array of observations. 
            Dictionary<(string screenLineName, int time, char direction), int[]> newlist = new();

            // Executes query and returns all station count data
            var dataList = (from stationcount in _context.StationCountObservations
                            join surveystation in _context.SurveyStations on stationcount.SurveyStationId equals surveystation.Id
                            join vehicleCountType in _context.VehicleCountTypes on stationcount.VehicleCountTypeId equals vehicleCountType.Id
                            join vehicle in _context.Vehicles on vehicleCountType.VehicleId equals vehicle.Id
                            join station in _context.Stations on surveystation.StationId equals station.Id
                            join survey in _context.Surveys on surveystation.SurveyId equals survey.Id
                            join screenlinestation in _context.ScreenlineStations on station.Id equals screenlinestation.StationId
                            join screenline in _context.Screenlines on screenlinestation.ScreenlineId equals screenline.Id
                            where individualCategorySelect.Contains(vehicleCountType.Id)
                               && stationcount.Time >= startTime & stationcount.Time <= endTime
                               && directionCountSelect.Contains((char)station.Direction!)
                               && screenline.RegionId == RegionId
                               && survey.Id == SelectedSurveyId
                            group new { stationcount, screenline, station, vehicleCountType } by new { stationcount.Time, screenline.SlineCode, station.Direction, vehicleCountType.Id }
                             into newgrp
                            select new
                            {
                                SlineCode = newgrp.Key.SlineCode,
                                Time = newgrp.Key.Time,
                                Direction = newgrp.Key.Direction,
                                Observations = newgrp.Sum(x => x.stationcount.Observation),
                                VehicleCountTypeId = newgrp.Key.Id,
                            }
                          ).OrderBy(x => x.Direction).ToList();

            //Get the number of stations associated with a screenline grouped by the station direction.
            var checkSums = (from regions in _context.Regions
                             join surveys in _context.Surveys on regions.Id equals surveys.RegionId
                             join stations in _context.Stations on regions.Id equals stations.RegionId
                             join screenlines in _context.Screenlines on regions.Id equals screenlines.RegionId

                             join screenlineStations in _context.ScreenlineStations on
                             new { ScreenlineId = screenlines.Id, StationId = stations.Id }
                             equals new { ScreenlineId = screenlineStations.ScreenlineId, StationId = screenlineStations.StationId }
                             join surveyStations in _context.SurveyStations on new { StationId = stations.Id, SurveyId = surveys.Id }
                             equals new { surveyStations.StationId, surveyStations.SurveyId }
                             where regions.Id == RegionId
                                  && surveys.Id == SelectedSurveyId
                             group new { regions, surveys, stations, screenlines, screenlineStations, surveyStations }
                             by new { screenlines.SlineCode, stations.Direction }
                       into newgrp
                             select new
                             {
                                 Sline = newgrp.Key.SlineCode,
                                 Direction = newgrp.Key.Direction,
                                 SumRecords = newgrp.Count()
                             }
                ).ToDictionary(entry => (entry.Sline, entry.Direction), entry => entry.SumRecords);

            foreach (var item in dataList)
            {
                if (!newlist.TryGetValue((item.SlineCode, item.Time, item.Direction), out var counts))
                {
                    newlist[(item.SlineCode, item.Time, item.Direction)] = counts = new int[individualCategorySelect.Length];
                }
                counts[Array.IndexOf(individualCategorySelect, item.VehicleCountTypeId)] += item.Observations;
            }

            var builder = new StringBuilder();
            // Build the header
            builder.Append("Sline,Direction,StationCount,SumOfRecords,StartTime,EndTime");
            foreach (var item in individualCategorySelect)
            {
                var category = Utility.TechnologyNames.First(c => c.id == item);
                builder.Append("," + category.name);
            }
            builder.AppendLine();

            foreach (var item in from x in newlist.Keys
                                 orderby x.screenLineName, x.time
                                 select x
                                     )
            {
                //Calculate the start time
                int starttime = Utility.CalculateStartTime(item.time);
                // Get the sum records the number of fifteen minute intervals between the min and max time.
                var sumRecord = ((Utility.FromDMGTimeToMinutes(item.time) - Utility.FromDMGTimeToMinutes(starttime)) / 15) + 1;
                var row = newlist[item];
                builder.Append(item.screenLineName);
                builder.Append(',');
                builder.Append(item.direction);
                builder.Append(',');
                var count = Utility.GetStationCount(_context, RegionId, SelectedSurveyId, item.screenLineName, item.direction);
                builder.Append(count);
                builder.Append(',');
                builder.Append(checkSums[(item.screenLineName, item.direction)] * sumRecord);
                builder.Append(",");
                builder.Append(starttime);
                builder.Append(',');
                builder.Append(item.time);
                foreach (var x in row)
                {
                    builder.Append(',');
                    builder.Append(x);
                }
                builder.AppendLine();
            }
            return builder.ToString();
        }

        /// <summary>
        /// Method to calculate the Total Volume results.
        /// </summary>
        /// <param name="startTime">Starting time user requested which is the start of the range</param>
        /// <param name="endTime">Ending time user requested which is the end of the range</param>
        /// <param name="individualCategorySelect">List of all categories selected by user</param>
        /// <param name="IndividualCategoriesList">Default list of all categories available for selected survey</param>
        /// <returns>String representation of the results</returns>
        internal string GetTotalVolume(char[] directionCountSelect,
            int startTime, int endTime,
            int[] individualCategorySelect, IList<IndividualCategory> individualCategoriesList
            )
        {
            // Dictionary of station_count records with a key of the tuple of screenline and direction and an array of observations. 
            Dictionary<(string screenlinename, char direction), int[]> newlist = new();

            //outputs a list of all stationcount data for all technologies
            var datalist = (from stationcount in _context.StationCountObservations
                            join stationsurvey in _context.SurveyStations on stationcount.SurveyStationId equals stationsurvey.Id
                            join vehiclecount in _context.VehicleCountTypes on stationcount.VehicleCountTypeId equals vehiclecount.Id
                            join vehicle in _context.Vehicles on vehiclecount.VehicleId equals vehicle.Id
                            join survey in _context.Surveys on stationsurvey.SurveyId equals survey.Id
                            join station in _context.Stations on stationsurvey.StationId equals station.Id
                            join screenlinestation in _context.ScreenlineStations on station.Id equals screenlinestation.StationId
                            join screenline in _context.Screenlines on screenlinestation.ScreenlineId equals screenline.Id
                            where
                                screenline.RegionId == RegionId
                                && survey.Id == SelectedSurveyId
                                && directionCountSelect.Contains((char)station.Direction!)
                                && stationcount.Time >= startTime & stationcount.Time <= endTime
                                && individualCategorySelect.Contains(vehiclecount.Id)
                            group new { screenline, stationcount, vehicle, vehiclecount, station }
                            by new { screenline.SlineCode, vehicle.Name, vehiclecount.Occupancy, 
                                vehiclecount.Id, station.Direction, stationcount.Time, station.StationCode }
                            into grp
                            select new
                            {
                                SlineCode = grp.Key.SlineCode,
                                Observations = grp.Sum(x => x.stationcount.Observation),
                                VehicleCountTypeId = grp.Key.Id,
                                Direction = grp.Key.Direction,
                                Time = grp.Key.Time,
                                stationCode = grp.Key.StationCode
                            }
                      ).ToList();

            // Get the minimum and maximum timestamps from the selected dataset.
            var minimumStartTime = Utility.CalculateStartTime(datalist.Min(x => x.Time));
            var maximumEndTime = datalist.Max(x => x.Time);
            // Get the sum records the number of fifteen minute intervals between the min and max time.
            var sumRecord = ((Utility.FromDMGTimeToMinutes(maximumEndTime) - Utility.FromDMGTimeToMinutes(minimumStartTime)) / 15) + 1;
            //Get the number of stations associated with a screenline grouped by the station direction.
            var checkSums = (from regions in _context.Regions
                       join surveys in _context.Surveys on regions.Id equals surveys.RegionId
                       join stations in _context.Stations on regions.Id equals stations.RegionId
                       join screenlines in _context.Screenlines on regions.Id equals screenlines.RegionId
                       
                       join screenlineStations in _context.ScreenlineStations on 
                       new { ScreenlineId = screenlines.Id, StationId = stations.Id } 
                       equals new { ScreenlineId = screenlineStations.ScreenlineId, StationId = screenlineStations.StationId }
                       join surveyStations in _context.SurveyStations on new { StationId = stations.Id, SurveyId = surveys.Id }
                       equals new { surveyStations.StationId, surveyStations.SurveyId }
                       where regions.Id == RegionId
                            && surveys.Id == SelectedSurveyId
                       group new { regions, surveys, stations, screenlines, screenlineStations, surveyStations }
                       by new { screenlines.SlineCode, stations.Direction }
                       into newgrp
                       select new
                       {
                           Sline = newgrp.Key.SlineCode,
                           Direction = newgrp.Key.Direction,
                           SumRecords = newgrp.Count()
                       }
                ).ToDictionary(entry => (entry.Sline, entry.Direction), entry => entry.SumRecords);

            foreach (var item in datalist)
            {
                if (!newlist.TryGetValue((item.SlineCode, item.Direction), out var counts))
                {
                    newlist[(item.SlineCode, item.Direction)] = counts = new int[individualCategorySelect.Length];
                }
                counts[Array.IndexOf(individualCategorySelect, item.VehicleCountTypeId)] += item.Observations;
            }

            var builder = new StringBuilder();
            // Build the header
            builder.Append("SLine,Direction,StationCount,SumOfRecords,StartTime,EndTime");
            foreach (var item in individualCategorySelect)
            {
                var category = Utility.TechnologyNames.First(c => c.id == item);
                builder.Append("," + category.name);
            }
            builder.AppendLine();

            foreach (var item in from x in newlist.Keys
                                 orderby x.screenlinename
                                 select x
                                 )
            {
                var row = newlist[item];
                builder.Append(item.screenlinename);
                builder.Append(',');
                builder.Append(item.direction);
                builder.Append(',');
                var count = Utility.GetStationCount(_context, RegionId, SelectedSurveyId, item.screenlinename, item.direction);
                builder.Append(count);
                builder.Append(',');
                builder.Append(checkSums[(item.screenlinename, item.direction)] * sumRecord);
                builder.Append(",");
                builder.Append(minimumStartTime);
                builder.Append(',');
                builder.Append(maximumEndTime);
                foreach (var x in row)
                {
                    builder.Append(',');
                    builder.Append(x);
                }
                builder.AppendLine();
            }
            return builder.ToString();
        }
    }
}
