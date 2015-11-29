using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UptimeJob2.Models
{
    public class DataModels
    {
        public class SearchResult
        {
            public string Error="";       
            public List<SearchResultsList> _SearchResultsList { get; set; }
            public class SearchResultsList
            {
                public string Title;
                public string ProductGroup;
                public string Author;
                public string Price;
            }

        }
    }
}