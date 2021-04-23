using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShopAdminAPI.Controllers.FrequentlyUsed;
using ShopAdminAPI.Models;
using ShopAdminAPI.Models.EnumModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopAdminAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ShopContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(ShopContext _context, ILogger<OrdersController> _logger)
        {
            this._context = _context;
            this._logger = _logger;
        }

        // GET: api/Orders/Unfinished
        [Route("Unfinished")]
        [HttpGet]
        public ActionResult<IEnumerable<Order>> GetUnfinishedOrders() 
        {
            var orders = _context.Order.Include(order => order.OrderDetails)
                                        .Include(order => order.PointRegisters)
                                        .Include(order => order.OrderInfo)
                                        .Where(order => order.OrderStatus != OrderStatus.delivered);

            if (!orders.Any()) 
            {
                return NotFound();
            }

            var result = orders.ToList();

            foreach (var order in result)
            {
                order.Sum = order.OrderDetails.Sum(detail => detail.Count * detail.Price) +
                            (order.DeliveryPrice ?? 0) -
                            (order.PointRegister?.Points ?? 0);
                order.OrderDetails = null;
            }

            return result;
        }

        // PUT: api/Orders/ChangeStatus/4/3
        [Route("ChangeStatus/{id}/{_status}")]
        [HttpPut]
        public ActionResult ChangeStatus(int id, OrderStatus _status)
        {
            var order = _context.Order.Include(order => order.User)
                                        .Include(order => order.PointRegisters)
                                        .Include(order => order.OrderDetails)
                                        .FirstOrDefault(order => order.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            //Если изменить статус, хотя уже минимум стоит "доставлено" или пытаемся установить недоступный для роли статус
            if (order.OrderStatus == OrderStatus.delivered)
            {
                return BadRequest("Заказ уже выполнен, смена статуса невозможна");
            }

            order.OrderStatus = _status;

            if (order.OrderStatus == OrderStatus.delivered)
            {
                PointsController pointsController = new PointsController(_context);

                if (order.PointsUsed)
                {
                    //Завершаем перевод баллов от клиента магазину
                    if (!pointsController.CompleteTransaction(order.PointRegister))
                    {
                        return BadRequest("Не удалось завершить транзакцию");
                    }
                }

                //Переводим кэшбэк
                PointRegister cashbackRegister;
                //Если любой из процессов кэшбэка даст сбой
                if (!pointsController.StartTransaction(pointsController.CalculateCashback(order), order.User, false, order, out cashbackRegister) ||
                    !pointsController.CompleteTransaction(cashbackRegister))
                {
                    return BadRequest("Не удалось произвести кэшбэк");
                }
            }

            _context.SaveChanges();

            return Ok();
        }
    }
}
