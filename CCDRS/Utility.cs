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
using Microsoft.AspNetCore.Builder;
using System.Drawing;

namespace CCDRS
{
    /// <summary>
    /// Contains cached data. Call Initialize before usage.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Private method to generate a list of vehicles and associated occupancy. 
        /// </summary>
        /// <param name="context">The CCDRS context service</param>
        /// <returns>A list of the query with the id and name and occupancy</returns>
        private static List<(int id, string name)> GenerateNumOfPersonTechIdList(CCDRS.Data.CCDRSContext context)
        {
            return (from t in context.Vehicles
                    join np in context.VehicleCountTypes on t.Id equals np.VehicleId
                    select new
                    {
                        Id = np.Id,
                        Name = t.Name + np.Occupancy
                    })
                   // The first ToList finishes the logic for EF,
                   // then we select a value tuple from that since EF can't use ValueTuples
                   .ToList()
                   .Select(x => (x.Id, x.Name))
                   .ToList();
        }

        /// <summary>
        /// list of tuple containing the id and new name of technology with vehicle and occupancy number
        /// </summary>
        public static List<(int id, string name)> NumOfPersonTechIdList;

        /// <summary>
        /// List of Directions user may select from. Accessible by other pages
        /// </summary>
        public static List<Direction> DirectionUtilityList;

        /// <summary>
        /// Function to run the database query to return a list of all directions avaiable
        /// </summary>
        /// <param name="context">The CCDRSContet service</param>
        /// <returns>A list of the Direction objects</returns>
        private static List<Direction> GenerateDirectionList(CCDRS.Data.CCDRSContext context)
        {
            return context.Directions.ToList();
        }

        /// <summary>
        /// List of IndividualCategory objects
        /// </summary>
        public static IList<IndividualCategory> IndividualCategoriesUtilityList;

        /// <summary>
        /// Method to call the database and run the query and return a list of all rows of individual categories
        /// </summary>
        /// <param name="context">The CCDRSContet service</param>
        /// <returns>A list of IndividualCategory Objects</returns>
        private static List<IndividualCategory> GenerateIndividualCategorylist(CCDRS.Data.CCDRSContext context)
        {
            return context.IndividualCategories.ToList();
        }

        /// <summary>
        /// Invoke this method to initialize cached data.
        /// </summary>
        /// <param name="context">The CCDRS context service</param>
        public static void Initialize(CCDRS.Data.CCDRSContext context)
        {
            NumOfPersonTechIdList = GenerateNumOfPersonTechIdList(context);
            DirectionUtilityList = GenerateDirectionList(context);
            IndividualCategoriesUtilityList = GenerateIndividualCategorylist(context);
        }
    }
}
