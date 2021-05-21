using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopAdminAPI.Models
{
    public class ProductPropertyValue
    {
        public int ProductId { get; set; }
        public int ProductPropertyId { get; set; }
        public string PropertyValue { get; set; }

        [JsonIgnore]
        public virtual Product Product { get; set; }
        [JsonIgnore]
        public virtual ProductProperty ProductProperty { get; set; }
    }
}
