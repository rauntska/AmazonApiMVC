using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace UptimeJob2.Controllers
{
    public class SearchProductController : Controller
    {
        //
        // GET: /SearchProduct/

        public ActionResult Search(string Keyword, double Rate, string SearchIndex)
        {
            Models.DataModels.SearchResult me = Models.BusinessLogic.SearchItems(Keyword, Rate, SearchIndex);           
            var json = JsonConvert.SerializeObject(me, Formatting.Indented);        
            return Content(json);
        }

    }
}
