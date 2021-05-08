using ApiClick.Models;
using ApiClick.Models.EnumModels;
using ApiClick.StaticValues;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopAdminAPI.Controllers.FrequentlyUsed;
using ShopAdminAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopAdminAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : Controller
    {
        private readonly ShopContext _context;

        public AnalyticsController(ShopContext _context)
        {
            this._context = _context;
        }

        /// <summary>
        /// Возвращает отчеты за определенный период
        /// </summary>
        /// <param name="datePeriod">Временной период (неделя, месяц, год, все время)</param>
        // GET: api/Analytics/30/3
        [HttpGet("{datePeriod}/{_page}")]
        public ActionResult<IEnumerable<Report>> Get(DatePeriod datePeriod, int _page)
        {
            //Вычитаем период для получения дня отчета. Получаем результаты от startingDay по текущий
            var startingDay = DateTime.UtcNow.Date.AddDays((int)datePeriod * -1);

            var reports = _context.Report.Include(rep => rep.ProductOfDay)
                                            .Where(rep => ((rep.CreatedDate >= startingDay) || datePeriod == DatePeriod.allTime));

            reports = Functions.GetPageRange(reports, _page, PageLengths.REPORT_LENGTH);

            if (!reports.Any())
            {
                return NotFound();
            }

            var result = reports.ToList();

            return result;
        }

        /// <summary>
        /// Возвращает статистические показатели за определенный период
        /// </summary>
        // GET: api/Analytics/Stats/7
        [Route("Stats/{datePeriod}")]
        [HttpGet]
        public ActionResult<string> GetPeriodStats(DatePeriod datePeriod)
        {
            //Вычитаем период для получения дня отчета. Получаем результаты от startingDay по текущий
            var startingDay = DateTime.UtcNow.Date.AddDays((int)datePeriod * -1);

            var registeredUsersCount = _context.User.Where(user => user.CreatedDate >= startingDay).Count();


            var installationsCount = _context.TokenRecord.FromSqlRaw($"SELECT * FROM dbo.TokenRecord WHERE CreatedDate >= {startingDay}").Count();
            var appLaunchCount = _context.SessionRecord.FromSqlRaw($"SELECT * FROM dbo.SessionRecord WHERE CreatedDate >= {startingDay}").Count();


            var ordersCount = _context.Order.Where(order => order.CreatedDate >= startingDay).Count();
            var sumRevenue = _context.Order.Include(order => order.OrderDetails)
                                            .DefaultIfEmpty() //Возвращает default если коллекция пуста
                                            .Sum(order => order != default ? PointsController.CalculateSum(order) : 0);

            var result = new 
            {
                Registered = registeredUsersCount,
                Installations = installationsCount,
                Launches = appLaunchCount,
                OrdersCount = ordersCount,
                OrdersSum = sumRevenue
            };

            return Json(result);
        }

        /// <summary>
        /// Генерирует отчеты в зависимости от времени прошедшего с генерации последнего отчета
        /// Метод вызывается при входе в административное приложение
        /// Если последний отчет был сгенерирован 
        /// </summary>
        // POST: api/Analytics/GenerateReports
        [Route("GenerateReports")]
        [HttpPost]
        public ActionResult GenerateReports() 
        {
            var lastReport = _context.Report.OrderBy(report => report.CreatedDate).First();

            var yesterday = DateTime.UtcNow.Date.AddDays(-1); //Последний отчет всегда должен быть вчерашним

            //Генерация первого отчета
            if (lastReport == null)
            {
                AddingReports(yesterday);
                _context.SaveChanges();
                return Ok();
            }

            var lastReportDate = lastReport.CreatedDate.Date;

            if (lastReportDate < yesterday) 
            {
                int daysSinceLastReport = (int)yesterday.Subtract(lastReportDate).TotalDays;

                //Узнав количество прошедших дней - генерируем отчеты
                for (; daysSinceLastReport > 0; daysSinceLastReport--) 
                {
                    var newReportDate = lastReportDate.AddDays(1);

                    AddingReports(newReportDate);
                }

                _context.SaveChanges();
            }

            return Ok();
        }

        private void AddingReports(DateTime _day)
        {
            var ordersFromDay = _context.Order.Where(order => order.CreatedDate.Date == _day).ToList();

            var totatSum = ordersFromDay?.Sum(order => PointsController.CalculateSum(order)) ?? 0;

            Report result = new Report()
            {
                OrderCount = ordersFromDay?.Count ?? 0,
                Sum = totatSum,
                CreatedDate = _day
            };

            if (ordersFromDay.Any())
            {
                //Выравниваем, создавая большой список orderDetails и группируем по критерию id продукции
                var productsByGroup = ordersFromDay.SelectMany(e => e.OrderDetails)
                                                    .Where(e => e.ProductId != null)
                                                    .GroupBy(e => e.ProductId);

                //Группирует по виду продукции и хранит суммарное количество
                var countsPerProductType = productsByGroup.Select(e => new { ProductId = e.Key, Count = e.Sum(x => x.Count) });

                //Находит группу с наибольшим count
                var productOfDay = countsPerProductType.Aggregate((max, next) => max.Count > next.Count ? max : next);

                result.ProductOfDayId = productOfDay.ProductId;
                result.ProductOfDayCount = productOfDay.Count;
                //Находим группу по id и из первого элемента вытаскиваем цену
                result.ProductOfDaySum = productOfDay.Count * productsByGroup.First(e => e.Key == productOfDay.ProductId).First().Price; //пока без проверок безопасности
            }

            _context.Report.Add(result);
        }
    }
}
