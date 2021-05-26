using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopAdminAPI.Configurations;
using ShopAdminAPI.Models;

namespace ShopAdminAPI.Controllers.FrequentlyUsed
{
    public class PointsController
    {
        ShopContext _context;

        public PointsController(ShopContext _context)
        {
            this._context = _context;
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

        public decimal CalculateCashback(Order _order)
        {
            var pointlessSum = CalculatePointless(_order);
            return pointlessSum * ShopConfiguration.Cashback;
        }

        /// <summary>
        /// Рассчитывает часть стоимости заказа, которая не уплачена баллами
        /// </summary>
        public decimal CalculatePointless(Order _order)
        {
            var sum = GetDetailsSum(_order.OrderDetails);
            var points = _order.PointRegisters.Any() ? _order.PointRegister?.Points ?? 0 : 0;
            return sum - points;
        }

        /// <summary>
        /// Вычисляет сумму деталей заказа
        /// </summary>
        public decimal GetDetailsSum(IEnumerable<OrderDetail> _details)
        {
            return _details.Sum(detail =>
            {
                var discountCoef = (100 - (detail.Discount ?? default)) / 100m;
                var newPrice = detail.Price * discountCoef;
                return newPrice * detail.Count;
            });
        }

        /// <summary>
        /// Вычисляет максимальную часть стоимости заказа, которую можно оплатить баллами 
        /// </summary>
        /// <param name="_userPoints">Текущие баллы пользователя</param>
        /// <param name="_order">Заказ, стоимость которого берется в расчет</param>
        /// <returns>Часть стоимости, которая будет оплачена баллами</returns>

        public decimal GetMaxPayment(decimal _userPoints, Order _order)
        {
            var detailsSum = GetDetailsSum(_order.OrderDetails);

            return GetMaxPayment(_userPoints, detailsSum);
        }

        /// <summary>
        /// Вычисляет максимальную часть стоимости заказа, которую можно оплатить баллами 
        /// </summary>
        /// <param name="_userPoints">Текущие баллы пользователя</param>
        /// <param name="_initialSum">Суммарная стоимость заказа</param>
        /// <returns>Часть стоимости, которая будет оплачена баллами</returns>
        public decimal GetMaxPayment(decimal _userPoints, decimal _initialSum)
        {
            decimal costInPoints = _initialSum / 100 * ShopConfiguration.MaxPoints;
            if (_userPoints > costInPoints)
            {
                return costInPoints;
            }
            else
            {
                return _userPoints;
            }
        }
    }
}
