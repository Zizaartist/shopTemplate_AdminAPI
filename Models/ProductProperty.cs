using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace ShopAdminAPI.Models
{
    public partial class ProductProperty
    {
        public ProductProperty()
        {
            Products = new HashSet<Product>();
            ProductPropertyValues = new HashSet<ProductPropertyValue>();
        }

        [JsonIgnore]
        public int ProductPropertyId { get; set; }
        public string PropertyName { get; set; }
        public int PropertyType { get; set; }

        [JsonIgnore]
        public virtual ICollection<Product> Products { get; set; }
        [JsonIgnore]
        public virtual ICollection<ProductPropertyValue> ProductPropertyValues { get; set; }

        [NotMapped]
        public string PropertyValue { get; set; }
    }
}
