using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShopAdminAPI.Models.EnumModels
{
    /// <summary>
    /// Модель, отвечающая за хранение методов оплаты
    /// </summary>
    public enum PaymentMethod
    {
        cash,
        card,
        online
    }

    public class PaymentMethodDictionaries
    {
        public static Dictionary<string, PaymentMethod> GetPaymentMethodFromString = new Dictionary<string, PaymentMethod>()
        {
            { "Наличными", PaymentMethod.cash },
            { "Картой курьеру", PaymentMethod.card },
            { "Картой онлайн", PaymentMethod.online }
        };

        public static Dictionary<PaymentMethod, string> GetStringFromPaymentMethod = new Dictionary<PaymentMethod, string>
        {
            { PaymentMethod.cash, "Наличными" },
            { PaymentMethod.card, "Картой курьеру" },
            { PaymentMethod.online, "Картой онлайн" }
        };
    }
}
