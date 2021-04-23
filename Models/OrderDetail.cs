using Newtonsoft.Json;
using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace ShopAdminAPI.Models
{
    public partial class OrderDetail
    {
        [JsonIgnore]
        public int OrderDetailId { get; set; }
        public int Count { get; set; }
        public int? ProductId { get; set; }
        public decimal Price { get; set; }
        [JsonIgnore]
        public int OrderId { get; set; }

        [JsonIgnore]
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
}
