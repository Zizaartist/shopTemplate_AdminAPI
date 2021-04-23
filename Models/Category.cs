using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace ShopAdminAPI.Models
{
    public partial class Category
    {
        public Category()
        {
            ChildCategories = new HashSet<Category>();
            Products = new HashSet<Product>();
        }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Image { get; set; }
        public int? ParentCategoryId { get; set; }

        [JsonIgnore]
        public virtual Category ParentCategory { get; set; }
        public virtual ICollection<Category> ChildCategories { get; set; }
        public virtual ICollection<Product> Products { get; set; }

        [NotMapped]
        public bool IsEndpoint { get => !ChildCategories.Any(); }
    }
}
