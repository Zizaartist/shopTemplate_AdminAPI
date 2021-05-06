using Newtonsoft.Json;
using ShopAdminAPI.Models;
using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace ApiClick.Models
{
    public partial class Report
    {
        [JsonIgnore]
        public int ReportId { get; set; }
        public int OrderCount { get; set; }
        public decimal Sum { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ProductOfDayId { get; set; }
        public int? ProductOfDayCount { get; set; }
        public decimal? ProductOfDaySum { get; set; }

        public virtual Product ProductOfDay { get; set; }
    }
}
