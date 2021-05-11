using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopAdminAPI.Models;

namespace ShopAdminAPI.Controllers.FrequentlyUsed
{
    public class PointsController
    {
        ShopContext _context;
        /// <summary>
        /// Коэффициент, который отражает проценты от суммы заказа, возвращаемые баллами
        /// </summary>
        const decimal pointsCoef = 0.05m;
        /// <summary>
        /// Максимальная сумма, которую можно оплатить баллами
        /// </summary>
        const int percentage = 30;

        public PointsController(ShopContext _context)
        {
            this._context = _context;
        }

        public decimal GetMaxPayment(decimal _userPoints, Order _order) 
        {
            var sumCost = _order.OrderDetails.Sum(e => e.Price * e.Count);
            decimal costInPoints = sumCost / 100 * percentage;
            if (_userPoints > costInPoints)
            {
                return costInPoints;
            }
            else 
            {
                return _userPoints;
            }
        }

        /// <summary>
        /// Выполняет изменение значения текущих баллов
        /// затем документирует изменение в виде регистра
        /// </summary>
        /// <param name="_points">Количество переводимых баллов</param>
        /// <param name="_user">Пользователь, чей счет претерпит изменение</param>
        /// <param name="_usedOrReceived">Параметр, утверждающий как изменится баланс баллов (true = -)(false = +)</param>
        /// <param name="_order">Заказ - источник значения максимальной денежной суммы</param>
        /// <param name="register">Полученый в результате регистр</param>
        /// <returns>Успешность операции</returns>
        public bool StartTransaction(decimal _points, User _user, bool _usedOrReceived, Order _order, out PointRegister register) 
        {
            register = null;

            if (_usedOrReceived) //Трата баллов пользователя
            {
                if (_user.Points < _points)
                {
                    return false; //недостаточно средств для действия
                }

                //Изымаем средства от отправителя
                try 
                {
                    _user.Points -= _points;
                }
                catch
                {
                    return false; //Низкоуровневая проблема, хер его знает
                }
            }
            else //Получение баллов
            {
                try
                {
                    _user.Points += _points;
                }
                catch
                {
                    return false; //Низкоуровневая проблема, хер его знает
                }
            }

            try
            {
                //Документируем
                var newPR = new PointRegister()
                {
                    TransactionCompleted = false,
                    UserId = _user.UserId,
                    UsedOrReceived = _usedOrReceived,
                    Points = _points,
                    CreatedDate = DateTime.UtcNow
                };
                register = newPR;
                _order.PointRegisters.Add(newPR);
            }
            catch 
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Завершает транзакцию, делая возврат средств невозможным
        /// </summary>
        /// <param name="_pointRegister">Транзакция, которую необходимо завершить</param>
        /// <returns>Успешность операции</returns>
        public bool CompleteTransaction(PointRegister _pointRegister)
        {
            try
            {
                if (!_pointRegister.TransactionCompleted)
                {
                    _pointRegister.TransactionCompleted = true;
                }
            }
            catch
            {
                return false; //Низкоуровневая проблема, хер его знает
            }
            return true;
        }

        /// <summary>
        /// Производит возврат средств
        /// </summary>
        /// <returns>Успешность операции</returns>
        public bool CancelTransaction(PointRegister _pointRegister)
        {
            User user;
            try
            {
                user = _context.User.Find(_pointRegister.UserId);
            }
            catch
            {
                return false;
            }

            //Совершаем изменение
            try
            {
                if (_pointRegister.UsedOrReceived)
                {
                    user.Points += _pointRegister.Points;
                }
                else
                {
                    user.Points -= _pointRegister.Points; //Пока дозволим отрицательные значения, но может оказаться уязвимостью
                }
                _context.PointRegister.Remove(_pointRegister);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public decimal CalculateSum(Order _order)
        {
            //Если детали не загружены - подгрузить
            if (!_order.OrderDetails.Any()) _context.Entry(_order).Collection(order => order.OrderDetails).Load();
            //Если деталей нет даже после загрузки - что-то не так
            if (!_order.OrderDetails.Any() || _order.OrderDetails.Any(e => e.Price == default || e.Count == default)) throw new Exception("Ошибка при вычислении кэшбэка");
            return _order.OrderDetails.Sum(e => e.Price * e.Count);
        }

        public decimal CalculateCashback(Order _order) 
        {
            var pointlessSum = CalculatePointless(_order);
            return pointlessSum * pointsCoef;
        }

        public decimal CalculatePointless(Order _order) 
        {
            var sum = CalculateSum(_order);
            try
            {
                var points = _order.PointRegisters.Any() ? _context.PointRegister.Find(_order.PointRegister.PointRegisterId).Points : 0;
                return sum - points;
            }
            catch (Exception _ex) 
            {
                throw _ex;
            }
        }
    }
}
