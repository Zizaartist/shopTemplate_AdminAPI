using Newtonsoft.Json;
using ShopAdminAPI.Models.EnumModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace ShopAdminAPI.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
            PointRegisters = new HashSet<PointRegister>();
        }

        public int OrderId { get; set; }
        public int UserId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public bool PointsUsed { get; set; }
        public decimal? DeliveryPrice { get; set; }
        public DateTime CreatedDate { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }
        public virtual OrderInfo OrderInfo { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        [JsonIgnore]
        public virtual ICollection<PointRegister> PointRegisters { get; set; }
        [NotMapped]
        public bool? Delivery { get; set; } //Получаем от клиента
        [NotMapped]
        [JsonIgnore]
        public PointRegister PointRegister
        {
            get
            {
                return PointRegisters?.FirstOrDefault(pr => pr.UsedOrReceived);
            }
        }
        [NotMapped]
        public decimal Sum { get; set; }
        [NotMapped]
        public string OrdererName { get; set; }
    }
}
