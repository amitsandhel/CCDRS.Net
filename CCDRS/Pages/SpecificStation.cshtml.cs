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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;
using CCDRS.Model;
using Microsoft.IdentityModel.Tokens;

namespace CCDRS.Pages
{
    public class SpecificStationModel : PageModel
    {
        private readonly CCDRS.Data.CCDRSContext _context;

        public SpecificStationModel(CCDRS.Data.CCDRSContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Initialize dropdown list of directions.
        /// </summary>
        [BindProperty]
        public IList<Direction> DirectionList { get; set; }
        
        /// <summary>
        /// Initalize list of vehicle count types.
        /// </summary>
        [BindProperty]
        public IList<IndividualCategory> VehicleCountTypeList { get; set; } = default!;

        /// <summary>
        /// Initialize list of person Count types
        /// </summary>
        [BindProperty]
        public IList<IndividualCategory> PersonCountTypeList { get; set; } = default!;

        /// <summary>
        /// Initialize list of technologies available.
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
        /// The selected survey.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public int SelectedSurveyId { get; set; }

        /// <summary>
        /// Constant enumerated value for total vehicle counts used to filter the individual
        /// category table for the radio buttons
        /// </summary>
        private const int VehicleCounts = 2;

        /// <summary>
        /// Constant enumerated value for person counts used to filter the individual
        /// category table for the radio buttons
        /// </summary>
        private const int PersonCounts = 3;

        /// <summary>
        /// Constant enumerated value for technologies used to filter the individual
        /// category table for the radio buttons
        /// </summary>
        private const int TechnologyCounts = 1;

        /// <summary>
        /// Display the data on page load
        /// </summary>
        /// <returns>the html page with the populated data</returns>
        public async Task OnGetAsync()
        {
            // local variable to query region name
            var regionName = _context.Regions
                              .Where(r => r.Id == RegionId)
                              .SingleOrDefault();
            // bind the local variable to the ViewData to display to the front-end
            ViewData["RegionName"] = regionName.Name;

            // local variable to query survey for year
            var ddlYearName = _context.Surveys
                              .Where(s => s.Id == SelectedSurveyId)
                              .SingleOrDefault();

            // bind the local variable to the ViewData to display to the front-end
            ViewData["DdlYearId"] = ddlYearName.Year;

            // Query a list of all stations associated with a given region.
            if (_context.Stations is not null)
            {
                StationsSelectList = new SelectList((from st in _context.Stations
                                                     where st.RegionId == RegionId
                                                     orderby st.StationCode
                                                     select new
                                                     {
                                                         st.Id,
                                                         Title = st.StationCode + " " + st.Description
                                                     }), "Id", "Title");
            }

            // Query a list of all Directions.
            if (_context.Directions != null)
            {
                DirectionList = Utility.DirectionUtilityList;
            }

            if (_context.IndividualCategories != null)
            {
                // List all vehicle count list
                VehicleCountTypeList = Utility.IndividualCategoriesUtilityList.
                    Where(s => s.RegionId == RegionId &
                               s.SurveyId == SelectedSurveyId &
                               s.CountType == VehicleCounts 
                            ).ToList();
                
                // List of all person count list
                PersonCountTypeList = Utility.IndividualCategoriesUtilityList.
                    Where(s => s.RegionId == RegionId &
                               s.SurveyId == SelectedSurveyId &
                               s.CountType == PersonCounts
                            ).ToList();

                // List of all technologies
                IndividualCategoriesList = Utility.IndividualCategoriesUtilityList.
                    Where(s => s.RegionId == RegionId &
                               s.SurveyId == SelectedSurveyId &
                               s.CountType == TechnologyCounts
                            ).ToList();
            }
        }

        /// <summary>
        /// post method that runs query and generates plain/txt format output
        /// </summary>
        /// <returns>Redirects user to a text file page with the results</returns>
        public IActionResult OnPostSubmit(
            int startTime, int endTime, int DdlStationId,
            int trafficVolumeRadioButtonSelect,
            int[] individualCategorySelect, IList<IndividualCategory> IndividualCategoriesList
            )
        {
            // If user selects total volume.
            if (trafficVolumeRadioButtonSelect == 1)
            {
                string x = GetTotalVolume(startTime,
                   endTime, DdlStationId,
                 individualCategorySelect, IndividualCategoriesList
                 );
                return Content(x);
            }
            else
            {
                // user selects fifteen minute interval
                string x = GetFifteenMinuteInterval(startTime,
                    endTime, DdlStationId,
                 individualCategorySelect, IndividualCategoriesList
                 );
                return Content(x);
            }
        }

        /// <summary>
        /// Method to calculate the Fifteen Minute Interval
        /// </summary>
        /// <param name="startTime">Starting time user requested which is the start of the range</param>
        /// <param name="endTime">Ending time user requested which is the end of the range</param>
        /// <param name="DdlStationId">Id of the selected station</param>
        /// <param name="individualCategorySelect">List of all categories selected by user</param>
        /// <param name="IndividualCategoriesList">Default list of all categories available for selected survey</param>
        /// <returns>String representation of the results</returns>
        internal string GetFifteenMinuteInterval(int startTime,
            int endTime, int DdlStationId,
            int[] individualCategorySelect, IList<IndividualCategory> IndividualCategoriesList
        )
        {
            //a list of dictionaries
            Dictionary<(string stationName, int time), int[]> newlist = new();

            //outputs a list of all stationcount data for all technologies
            var dataList = (from sc in _context.StationCountObservations
                            join ss in _context.SurveyStations on sc.SurveyStationId equals ss.Id
                            join np in _context.VehicleCountTypes on sc.VehicleCountTypeId equals np.Id
                            join s in _context.Surveys on ss.SurveyId equals s.Id
                            join st in _context.Stations on ss.StationId equals st.Id
                            join reg in _context.Regions on st.RegionId equals reg.Id
                            join tech in _context.Vehicles on np.VehicleId equals tech.Id
                            where
                                reg.Id == RegionId &
                                st.Id == DdlStationId &
                                s.Id == SelectedSurveyId &
                                sc.Time > startTime & sc.Time <= endTime &
                                individualCategorySelect.Contains(np.Id)
                            select new
                            {
                                st.StationCode,
                                Time = sc.Time,
                                Observations = sc.Observation,
                                npFKId = np.Id
                            }
                      );
            foreach (var item in dataList)
            {
                if (!newlist.TryGetValue((item.StationCode, item.Time), out var counts))
                {
                    newlist[(item.StationCode, item.Time)] = counts = new int[individualCategorySelect.Length];
                }
                counts[Array.IndexOf(individualCategorySelect, item.npFKId)] += item.Observations;
            }

            var builder = new StringBuilder();
            //building the header with technologies
            builder.Append("Station,Time");
            foreach (var item in individualCategorySelect)
            {
                var category = Utility.NumOfPersonTechIdList.First(c => c.id == item);
                builder.Append("," + category.name);
            }
            builder.AppendLine();

            foreach (var item in from x in newlist.Keys
                                 orderby x.stationName, x.time
                                 select x
                                 )
            {
                var row = newlist[item];
                builder.Append(item.stationName);
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
        /// Method to calculate the Total Volume
        /// </summary>
        /// <param name="startTime">Starting time user requested which is the start of the range</param>
        /// <param name="endTime">Ending time user requested which is the end of the range</param>
        /// <param name="DdlStationId">Id of the selected station</param>
        /// <param name="individualCategorySelect">List of all categories selected by user</param>
        /// <param name="IndividualCategoriesList">Default list of all categories available for selected survey</param>
        /// <returns>String representation of the results</returns>
        internal string GetTotalVolume(int startTime, int endTime,
            int DdlStationId,
            int[] individualCategorySelect,
             IList<IndividualCategory> IndividualCategoriesList
            )
        {
            //a list of dictionaries
            Dictionary<string, int[]> newlist = new();

            //outputs a list of all stationcount data for all technologies
            var dataList = (from sc in _context.StationCountObservations
                            join ss in _context.SurveyStations on sc.SurveyStationId equals ss.Id
                            join np in _context.VehicleCountTypes on sc.VehicleCountTypeId equals np.Id
                            join s in _context.Surveys on ss.SurveyId equals s.Id
                            join st in _context.Stations on ss.StationId equals st.Id
                            join reg in _context.Regions on st.RegionId equals reg.Id
                            join tech in _context.Vehicles on np.VehicleId equals tech.Id
                            where
                                reg.Id == RegionId &
                                st.Id == DdlStationId &
                                s.Id == SelectedSurveyId &
                                sc.Time > startTime & sc.Time <= endTime &
                                individualCategorySelect.Contains(np.Id)
                            group new { sc, np, st, tech } by new { st.StationCode, tech.Name, np.Occupancy, np.Id }
                            into newgrp
                            select new
                            {
                                Station = newgrp.Key.StationCode,
                                Observations = newgrp.Sum(x => x.sc.Observation),
                                npFKId = newgrp.Key.Id,
                                newgrp.Key.Occupancy
                            }
                      );
            foreach (var item in dataList)
            {
                if (!newlist.TryGetValue(item.Station, out var counts))
                {
                    newlist[item.Station] = counts = new int[individualCategorySelect.Length];
                }
                counts[Array.IndexOf(individualCategorySelect, item.npFKId)] += item.Observations;
            }

            var builder = new StringBuilder();
            //building the header with technologies
            builder.Append("Station,startTime,endtime");
            foreach (var item in individualCategorySelect)
            {
                var category = Utility.NumOfPersonTechIdList.First(c => c.id == item);
                builder.Append("," + category.name);
            }
            builder.AppendLine();

            foreach (var item in newlist.Keys.OrderBy(x => x))
            {
                var row = newlist[item];
                builder.Append(item);
                builder.Append(',');
                builder.Append(startTime);
                builder.Append(',');
                builder.Append(endTime);
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
