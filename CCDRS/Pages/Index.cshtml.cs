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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CCDRS.Pages
{
    public class IndexModel : PageModel
    {
        // read only context variable of all database contexts
        private readonly CCDRS.Data.CCDRSContext _context;

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, CCDRS.Data.CCDRSContext context)
        {
            _logger = logger;
            _context = context;
        }

        // initialize list of regions 
        public IList<Region>? RegionList { get; set; }
        
        //initialize list of all years associated with a region
        public IList<Survey>? SurveyList { get; set; }

        /// <summary>
        /// Get Method which displays the contents on the main CCDRS page
        /// </summary>
        /// <returns></returns>
        public async Task OnGet()
        {
            // configure session 
            Utility.SetUserName(HttpContext);
            //HttpContext.Session.SetString("Username", User.Identity.Name);

            // run the database query to output a list all regions
            if (_context.Regions is not null)
            {
                RegionList = await _context.Regions.OrderBy(r => r.Name).ToListAsync();
            }

            // run the database query to output a list of all survey
            if (_context.Surveys is not null)
            {
                SurveyList = await _context.Surveys.ToListAsync();
            }
        }

        // writeable property for the Post call method to pass the variable to other pages
        // store the id of the selected region. This is the region's primary key.
        [BindProperty(SupportsGet = true)]
        public int regionId { get; set; }

        //writeable property to store the year of the selected region.
        [BindProperty(SupportsGet = true)]
        public int SelectedSurveyId { get; set; }

        // run database to return the list of surveys specifically the years associated 
        //with a given region. 
        public JsonResult OnGetRegionId()
        {
            var DataList = _context.Surveys.
                Where(s => s.RegionId == regionId)
                .OrderBy(s => s.Year).ToList();
            return new JsonResult(DataList);
        }


        // redirect to the AllStation page
        public ActionResult OnPostAllStation()
        {
            return RedirectToPage("AllStation", new { SelectedSurveyId, regionId });  
        }

        // redirect to the AllScreenline page
        public IActionResult OnPostAllScreenlines()
        {
            return RedirectToPage("AllScreenline", new { SelectedSurveyId, regionId });
        }

        // redirect to the Specific Station page
        public IActionResult OnPostSpecificStation()
        {
            return RedirectToPage("SpecificStation", new { SelectedSurveyId, regionId });
        }

        // redirect to the specific screenline page
        public IActionResult OnPostSpecificScreenline()
        {
            return RedirectToPage("SpecificScreenline", new { SelectedSurveyId, regionId });
        }
    }
}