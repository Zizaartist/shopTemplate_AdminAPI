using Newtonsoft.Json;
using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace ShopAdminAPI.Models
{
    public partial class Product
    {
        public Product()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public string ProductName { get; set; }
        public int CategoryId { get; set; }
        public int? Discount { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        [JsonIgnore]
        public DateTime CreatedDate { get; set; }

        [JsonIgnore]
        public virtual Category Category { get; set; }
        [JsonIgnore]
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
