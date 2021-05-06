using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopAdminAPI.Models.Statistics
{
    //Запись о получении токена
    //Формируется при первом запуске установленного приложения (учтите, что установок может быть несколько)
    public class TokenRecord
    {
        public DateTime CreatedDate { get; set; }
    }
}
