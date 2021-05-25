using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShopAdminAPI.Configurations;
using ShopAdminAPI.Controllers.FrequentlyUsed;
using ShopAdminAPI.Models.EnumModels;
using ShopHubAPI.StaticValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopAdminAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        // PUT: api/Config/3
        [Route("{ver}")]
        [HttpPut]
        public async Task<ActionResult> UpdateConfiguration(int ver) 
        {
            if (ShopConfiguration.Version == ver) 
            {
                return Ok();
            }

            await GetShopConfig();

            return Ok();
        }

        // GET: api/Config/Awake
        [Route("Awake")]
        [HttpGet]
        public ActionResult Awake() => Ok();

        public static async Task GetShopConfig()
        {
            try
            {
                var client = HttpClientSingleton.HttpClient;
                var response = await client.GetAsync($"{ApiConfiguration.SHOP_HUB_API}{ApiStrings.CONFIG_GET}{ApiConfiguration.SHOP_ID}/{null}"); //пока null, не кэшируем

                if (response.IsSuccessStatusCode)
                {
                    var template = new
                    {
                        DeliveryPrice = 0m,
                        MaxPoints = 0,
                        PaymentMethods = "",
                        Version = 0
                    };

                    string result = await response.Content.ReadAsStringAsync();
                    var temp = JsonConvert.DeserializeAnonymousType(result, template);

                    ShopConfiguration.DeliveryPrice = temp.DeliveryPrice;
                    ShopConfiguration.MaxPoints = temp.MaxPoints;

                    var pms = new List<PaymentMethod>();
                    foreach (var character in temp.PaymentMethods)
                        pms.Add((PaymentMethod)int.Parse(character.ToString()));

                    ShopConfiguration.PaymentMethods = pms;
                    ShopConfiguration.Version = temp.Version;
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine("БЕЙ ТРЕВОГУ! ВСЕ К ХУЯМ СЛОМАЛОСЬ!");
            }
        }
    }
}
