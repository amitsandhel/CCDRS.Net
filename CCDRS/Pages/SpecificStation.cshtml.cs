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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CCDRS.Pages
{
    /// <summary>
    /// Class to display the data for the Specific Station page.
    /// </summary>
    public class SpecificStationModel : PageModel
    {
        private readonly CCDRS.Data.CCDRSContext _context;

        public SpecificStationModel(CCDRS.Data.CCDRSContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Initialize dropdown list of directions used to store user selected directions.
        /// </summary>
        [BindProperty]
        public IList<Direction> DirectionList { get; set; } = null!;

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
        /// Initialize a select list of all stations available for a given region and year.
        /// </summary>
        public IEnumerable<SelectListItem> StationsSelectList { get; set; } = default!;

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

            // Query station table to return a list of all stations associated with a given region.
            if (_context.Stations is not null)
            {
                StationsSelectList = (from st in _context.Stations
                                                     join surveyStations in _context.SurveyStations on st.Id equals surveyStations.StationId
                                                     join surveys in _context.Surveys on surveyStations.SurveyId equals surveys.Id
                                                     join stationcounts in _context.StationCountObservations on surveyStations.Id equals stationcounts.SurveyStationId
                                                     where st.RegionId == RegionId
                                                     && surveys.Id == SelectedSurveyId
                                                     select new
                                                     {
                                                         st.Id,
                                                         st.StationCode,
                                                         st.Description,
                                                         st.StationNum
                                                     })
                                                     .Distinct()
                                                     .OrderBy(st => st.StationNum)
                                                     .Select(station => new SelectListItem(station.StationCode + " " + station.Description, station.Id.ToString()));
            }

            // Query directions table to return direction radiobutton options.
            if (_context.Directions != null)
            {
                DirectionList = Utility.Directions;
            }

            // Query individual_categories table to return various radiobutton options.
            if (_context.IndividualCategories != null)
            {
                // List all total vehicle count options
                VehicleCountTypeList = Utility.GetTotalVehicleCounts(RegionId, SelectedSurveyId);

                // List all total person count options
                PersonCountTypeList = Utility.GetTotalPersonCounts(RegionId, SelectedSurveyId);

                // List all technologies options
                IndividualCategoriesList =
                    Utility.GetTechnologyCounts(RegionId, SelectedSurveyId);
            }
        }

        /// <summary>
        /// Post method that executes database query to generate plain/txt format output
        /// </summary>
        /// <returns>Redirects user to a text file page with the results from executed query.</returns>
        public IActionResult OnPostSubmit(
            int startTime, int endTime, int[] SelectedStations,
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
            // Check to see if user wants to calculate total volume or 15 minute intervals
            if (trafficVolumeRadioButtonSelect == 1)
            {
                // Build the header of the content file
                builder.Append(regionName?.Name ?? "Unknown Region");
                builder.Append(' ');
                builder.Append(surveyYear?.Year);
                builder.AppendLine();
                builder.Append("Station,Direction,StartTime,EndTime");
                foreach (var item in individualCategorySelect)
                {
                    var category = Utility.TechnologyNames.First(c => c.id == item);
                    builder.Append("," + category.name);
                }
                builder.AppendLine();
                // Loop through selected stations list and run them
                foreach (int SelectedStationId in SelectedStations)
                {
                    builder.Append(GetTotalVolume(startTime,
                      endTime, SelectedStationId,
                    individualCategorySelect, individualCategoriesList));
                }
                return Content(builder.ToString());
            }
            else
            {
                // Build the header of the content file
                builder.Append(regionName?.Name ?? "Unknown Region");
                builder.Append(' ');
                builder.Append(surveyYear?.Year);
                builder.AppendLine();
                builder.Append("Station,Direction,StartTime,EndTime");
                foreach (var item in individualCategorySelect)
                {
                    var category = Utility.TechnologyNames.First(c => c.id == item);
                    builder.Append("," + category.name);
                }
                builder.AppendLine();
                //  Loop through selected stations list and run them
                foreach (int SelectedStationId in SelectedStations)
                {
                    // User selects fifteen minute interval.
                    builder.Append(GetFifteenMinuteInterval(startTime,
                        endTime, SelectedStationId,
                     individualCategorySelect, individualCategoriesList));
                } 
                return Content(builder.ToString());
            }
        }

        /// <summary>
        /// Method to calculate the Fifteen Minute Interval results.
        /// </summary>
        /// <param name="startTime">Starting time user requested which is the start of the range</param>
        /// <param name="endTime">Ending time user requested which is the end of the range</param>
        /// <param name="SelectedStationId">User selected Station</param>
        /// <param name="individualCategorySelect">List of all categories selected by user</param>
        /// <param name="individualCategoriesList">Default list of all categories available for selected survey</param>
        /// <returns>String representation of the results</returns>
        internal string GetFifteenMinuteInterval(int startTime,
            int endTime, int SelectedStationId,
            int[] individualCategorySelect, IList<IndividualCategory> individualCategoriesList)
        {
            // Dictionary of station_count records with a key of the tuple of station code and time and an array of observations. 
            Dictionary<(string stationName, int time, char direction), int[]> newlist = new();

            // Executes query and returns all station count data 
            var dataList = (from stationCounts in _context.StationCountObservations
                            join surveyStations in _context.SurveyStations on stationCounts.SurveyStationId equals surveyStations.Id
                            join vehicleCountTypes in _context.VehicleCountTypes on stationCounts.VehicleCountTypeId equals vehicleCountTypes.Id
                            join surveys in _context.Surveys on surveyStations.SurveyId equals surveys.Id
                            join stations in _context.Stations on surveyStations.StationId equals stations.Id
                            join vehicles in _context.Vehicles on vehicleCountTypes.VehicleId equals vehicles.Id
                            where
                                stations.RegionId == RegionId &
                                stations.Id == SelectedStationId &
                                surveys.Id == SelectedSurveyId &
                                stationCounts.Time > startTime & stationCounts.Time <= endTime &
                                individualCategorySelect.Contains(vehicleCountTypes.Id)
                            select new
                            {
                                stations.StationCode,
                                Time = stationCounts.Time,
                                Observations = stationCounts.Observation,
                                VehicleCountTypeId = vehicleCountTypes.Id,
                                Direction = stations.Direction
                            }
                      );
            foreach (var item in dataList)
            {
                if (!newlist.TryGetValue((item.StationCode, item.Time, item.Direction), out var counts))
                {
                    newlist[(item.StationCode, item.Time, item.Direction)] = counts = new int[individualCategorySelect.Length];
                }
                counts[Array.IndexOf(individualCategorySelect, item.VehicleCountTypeId)] += item.Observations;
            }

            var builder = new StringBuilder();
            foreach (var item in from x in newlist.Keys
                                 orderby x.stationName, x.time
                                 select x
                                 )
            {
                //Calculate the start time
                int starttime = Utility.CalculateStartTime(item.time);
                var row = newlist[item];
                builder.Append(item.stationName);
                builder.Append(',');
                builder.Append(item.direction);
                builder.Append(',');
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
        /// <param name="selectedStationId">Id of the selected station</param>
        /// <param name="individualCategorySelect">List of all categories selected by user</param>
        /// <param name="IndividualCategoriesList">Default list of all categories available for selected survey</param>
        /// <returns>String representation of the results</returns>
        internal string GetTotalVolume(int startTime, int endTime,
            int selectedStationId,
            int[] individualCategorySelect,
             IList<IndividualCategory> IndividualCategoriesList
            )
        {
            // Dictionary of station_count records with a key of station code and an array of observations. 
            Dictionary<(string station, char direction), int[]> newlist = new();

            // Executes query and returns all station count data
            var dataList = (from stationCounts in _context.StationCountObservations
                            join surveyStation in _context.SurveyStations on stationCounts.SurveyStationId equals surveyStation.Id
                            join vehicleCountTypes in _context.VehicleCountTypes on stationCounts.VehicleCountTypeId equals vehicleCountTypes.Id
                            join surveys in _context.Surveys on surveyStation.SurveyId equals surveys.Id
                            join stations in _context.Stations on surveyStation.StationId equals stations.Id
                            join vehicles in _context.Vehicles on vehicleCountTypes.VehicleId equals vehicles.Id
                            where
                                stations.RegionId == RegionId &
                                stations.Id == selectedStationId &
                                surveys.Id == SelectedSurveyId &
                                stationCounts.Time >= startTime & stationCounts.Time <= endTime &
                                individualCategorySelect.Contains(vehicleCountTypes.Id)
                            group new { stationCounts, vehicleCountTypes, stations, vehicles } 
                            by new { stations.StationCode, vehicles.Name, vehicleCountTypes.Occupancy, 
                                vehicleCountTypes.Id, stationCounts.Time, stations.Direction }
                            into groupedBystationCountObservations
                            select new
                            {
                                Station = groupedBystationCountObservations.Key.StationCode,
                                Observations = groupedBystationCountObservations.Sum(x => x.stationCounts.Observation),
                                VehicleCountTypeId = groupedBystationCountObservations.Key.Id,
                                groupedBystationCountObservations.Key.Occupancy,
                                Time = groupedBystationCountObservations.Key.Time,
                                Direction = groupedBystationCountObservations.Key.Direction
                            }
                      );

            // Get the minimum and maximum timestamps from the selected dataset.
            var minimumStartTime = Utility.CalculateStartTime(dataList.Min(x => x.Time));
            var maximumEndTime = dataList.Max(x => x.Time);

            foreach (var item in dataList)
            {
                if (!newlist.TryGetValue((item.Station, item.Direction), out var counts))
                {
                    newlist[(item.Station, item.Direction)] = counts = new int[individualCategorySelect.Length];
                }
                counts[Array.IndexOf(individualCategorySelect, item.VehicleCountTypeId)] += item.Observations;
            }

            var builder = new StringBuilder();
            foreach (var item in newlist.Keys.OrderBy(x => x))
            {
                var row = newlist[item];
                builder.Append(item.station);
                builder.Append(',');
                builder.Append(item.direction);
                builder.Append(',');
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
