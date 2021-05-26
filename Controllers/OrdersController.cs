using ApiClick.StaticValues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
    [Authorize]
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

        /// <summary>
        /// Возвращает заказы со статусом "Получено"
        /// </summary>
        // GET: api/Orders/New
        [Route("New")]
        [HttpGet]
        public ActionResult<IEnumerable<Order>> GetNewOrders()
        {
            IQueryable<Order> orders = _context.Order.Include(order => order.OrderInfo)
                                        .Where(order => order.OrderStatus == OrderStatus.sent)
                                        .OrderByDescending(order => order.CreatedDate);

            if (!orders.Any())
            {
                return NotFound();
            }

            var result = orders.ToList();

            return result;
        }

        /// <summary>
        /// Возвращает заказы со статусом "Принято" и выше, не включая последний
        /// </summary>
        // GET: api/Orders/Unfinished
        [Route("Unfinished")]
        [HttpGet]
        public ActionResult<IEnumerable<Order>> GetUnfinishedOrders() 
        {
            IQueryable<Order> orders = _context.Order.Include(order => order.OrderInfo)
                                        .Where(order => order.OrderStatus != OrderStatus.delivered && //Последний статус
                                                        order.OrderStatus != OrderStatus.sent) //Первый статус
                                        .OrderByDescending(order => order.CreatedDate);

            if (!orders.Any()) 
            {
                return NotFound();
            }

            var result = orders.ToList();

            return result;
        }

        /// <summary>
        /// Возвращает историю заказов постранично
        /// </summary>
        // GET: api/Orders/History/3
        [Route("History/{_page}")]
        [HttpGet]
        public ActionResult<IEnumerable<Order>> GetHistory(int _page)
        {
            IQueryable<Order> orders = _context.Order.Include(order => order.OrderInfo)
                                        .Where(order => order.OrderStatus >= OrderStatus.delivered)
                                        .OrderByDescending(order => order.CreatedDate);

            orders = Functions.GetPageRange(orders, _page, PageLengths.ORDER_LENGTH);

            if (!orders.Any())
            {
                return NotFound();
            }

            var result = orders.ToList();

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
            if (order.OrderStatus >= OrderStatus.delivered)
            {
                return BadRequest("Заказ уже выполнен/отменен, смена статуса невозможна");
            }

            order.OrderStatus = _status;

            PointsController pointsController = new PointsController(_context);
            //Завершаем транзакцию баллов и производим cashback
            if (order.OrderStatus == OrderStatus.delivered)
            {
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
            //Производим отмену транзакции если таковая существует
            else if (order.OrderStatus == OrderStatus.canceled) 
            {
                if (order.PointRegister != null)
                {
                    if (!pointsController.CancelTransaction(order.PointRegister))
                    {
                        return BadRequest("Не удалось произвести возврат средств");
                    }
                }
            }

            _context.SaveChanges();

            return Ok();
        }

        // DELETE: api/Orders/RefuseOrder/2
        [Route("RefuseOrder/{id}")]
        [HttpDelete]
        public ActionResult RefuseOrder(int id)
        {
            var order = _context.Order.Include(order => order.OrderDetails)
                                            .ThenInclude(detail => detail.Product)
                                        .Include(order => order.PointRegisters)
                                        .FirstOrDefault(order => order.OrderId == id);

            if (order == null || order.OrderStatus != OrderStatus.sent)
            {
                return BadRequest("Заказ не существует или уже утвержден на исполнение");
            }

            //Если есть pointRegister - вернуть средства
            if (order.PointsUsed && order.PointRegister != null)
            {
                PointsController pointsController = new PointsController(_context);
                if (!pointsController.CancelTransaction(order.PointRegister))
                {
                    return BadRequest("Ошибка при попытке возврата баллов");
                }
            }

            //Возвращаем продукты на место
            foreach (var detail in order.OrderDetails)
            {
                detail.Product.InStorage += detail.Count;
            }

            //"Отсоединяем" связанные сущности, чтобы контексту было наплевать на отсутсвие каскада
            //В статье говорится что отсоединение затрагивает только указанные сущности, поэтому
            //по идее продукты должны измениться
            //https://docs.microsoft.com/ru-ru/dotnet/api/system.data.objects.objectcontext.detach?view=netframework-4.8#-----------
            var detachedOrderDetails = _context.ChangeTracker.Entries<OrderDetail>().ToList();

            foreach (var detail in detachedOrderDetails)
            {
                detail.State = EntityState.Detached;
            }

            //Удаляем без сомнений и позволяем триггеру делать свою работу
            _context.Order.Remove(order);
            _context.SaveChanges();

            return Ok();
        }

        // GET: api/Orders/Details/{id}
        [Route("Details/{id}")]
        [HttpGet]
        public ActionResult<IEnumerable<OrderDetail>> GetOrder(int id)
        {
            var order = _context.Order.Include(order => order.OrderDetails)
                                            .ThenInclude(detail => detail.Product)
                                        .FirstOrDefault(order => order.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return order.OrderDetails.ToList();
        }
    }
}
