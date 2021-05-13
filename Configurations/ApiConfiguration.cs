using ShopAdminAPI.Controllers.FrequentlyUsed;
using ShopHubAPI.StaticValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopAdminAPI.Configurations
{
    //Конфигурация api единицы
    public class ApiConfiguration
    {
        public static string SHOP_ID;
        public static string TOKEN_AUDIENCE => Functions.GetHashFromString(SHOP_ID + AuthOptions.SHOP_KEY);
        public static string SHOP_HUB_API;
    }
}
