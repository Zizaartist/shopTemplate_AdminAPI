using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopAdminAPI.Models
{
    /// <summary>
    /// Модель для передачи данных об устройстве при регистрации
    /// </summary>
    public class DeviceRegistration
    {
        public string Platform { get; set; }
        public string Handle { get; set; }
        public string[] Tags { get; set; }
    }
}