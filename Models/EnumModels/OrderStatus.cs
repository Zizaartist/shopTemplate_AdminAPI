using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ShopAdminAPI.Models.EnumModels
{
    public enum OrderStatus 
    {
        sent, //Отправлен клиентом
        received, //Принят брендом (автоматически или через агрегатор)
        onWay, //Исполнитель утверждает что уже доставляет товар
        delivered, //Доставка произведена
        canceled //Заказ отменен
    }
}
