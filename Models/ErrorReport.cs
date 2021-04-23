using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace ShopAdminAPI.Models
{
    public partial class ErrorReport
    {
        public int ErrorReportId { get; set; }
        public string Text { get; set; }
        public int? UserId { get; set; }

        public virtual User User { get; set; }
    }
}
