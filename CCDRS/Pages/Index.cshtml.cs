using CCDRS.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

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
        public IList<Region> RegionList { get; set; }
        
        //initialize list of all years associated with a region
        public IList<Survey> SurveyList { get; set; }

        /// <summary>
        /// Get Method which displays the contents on the main CCDRS page
        /// </summary>
        /// <returns></returns>
        public async Task OnGet()
        {
            // run the database query to output a list all regions
            if (_context.Regions is not null)
            {
                RegionList = await _context.Regions.ToListAsync();
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
        public int DdlYear { get; set; }

        // run database to return the list of surveys specifically the years associated 
        //with a given region. 
        public JsonResult OnGetRegionId()
        {
            var DataList = _context.Surveys.
                Where(s => s.RegionId == regionId)
                .ToList();
            return new JsonResult(DataList);
        }

        // redirect to the AllStation page
        public IActionResult OnPostAllStation()
        {
            return RedirectToPage("AllStation", new { DdlYear, regionId });
        }

        // redirect to the AllScreenline page
        public IActionResult OnPostAllScreenlines()
        {
            return RedirectToPage("AllScreenlines", new { DdlYear, regionId });
        }

        // redirect to the Specific Station page
        public IActionResult OnPostSpecificStation()
        {
            return RedirectToPage("SpecificStation", new { DdlYear, regionId });
        }

        // redirect to the specific screenline page
        public IActionResult OnPostSpecificScreenline()
        {
            return RedirectToPage("SpecificScreenline", new { DdlYear, regionId });
        }
    }
}