using ShopAdminAPI.Models.EnumModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopAdminAPI.Configurations
{
    //Конфигурация бизнес логики
    public class ShopConfiguration
    {
        public static decimal DeliveryPrice;
        public static int MaxPoints;
        public static List<PaymentMethod> PaymentMethods;
        public static int Version;
    }
}
