using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ShopAdminAPI.Configurations;
using ShopAdminAPI.Controllers.FrequentlyUsed;
using ShopAdminAPI.Models.EnumModels;
using ShopHubAPI.StaticValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShopAdminAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<ConfigController> _logger;

        public ConfigController(IHttpClientFactory _clientFactory, ILogger<ConfigController> _logger)
        {
            this._clientFactory = _clientFactory;
            this._logger = _logger;
        }

        // PUT: api/Config/3
        [Route("{ver}")]
        [HttpPut]
        public async Task<ActionResult> UpdateConfiguration(int ver) 
        {
            if (ShopConfiguration.Version == ver) 
            {
                return Ok();
            }

            await GetShopConfig(_clientFactory);

            return Ok();
        }

        // GET: api/Config/Awake
        [Route("Awake")]
        [HttpGet]
        public ActionResult Awake() => Ok();

        public async Task<Action> GetShopConfig(IHttpClientFactory clientFactory)
        {
            var client = clientFactory.CreateClient();
            var response = await client.GetAsync($"{ApiConfiguration.SHOP_HUB_API}/{ApiStrings.CONFIG_GET}{ApiConfiguration.SHOP_ID}"); //пока null, не кэшируем

            if (response.IsSuccessStatusCode)
            {
                var template = new
                {
                    DeliveryPrice = 0m,
                    MaxPoints = 0,
                    PaymentMethods = "",
                    Cashback = 0,
                    MinimalDeliveryPrice = (decimal?)0m,
                    Theme = 0,
                    Version = 0
                };

                string result = await response.Content.ReadAsStringAsync();
                var temp = JsonConvert.DeserializeAnonymousType(result, template);

                ShopConfiguration.DeliveryPrice = temp.DeliveryPrice;
                ShopConfiguration.MaxPoints = temp.MaxPoints;
                ShopConfiguration.Cashback = temp.Cashback;
                ShopConfiguration.MinimalDeliveryPrice = temp.MinimalDeliveryPrice;

                var pms = new List<PaymentMethod>();
                foreach (var character in temp.PaymentMethods)
                    pms.Add((PaymentMethod)int.Parse(character.ToString()));

                ShopConfiguration.PaymentMethods = pms;
                ShopConfiguration.Version = temp.Version;
            }

            return null;
        }
    }
}
