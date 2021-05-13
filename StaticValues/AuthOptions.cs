using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopHubAPI.StaticValues
{
    public class AuthOptions
    {
        public static string ISSUER = "404d76cb16f24632c4a19556d93eab8e"; //shopissuer
        const string KEY = "d761e4c9d0dcb25eaafce3ebe84e9cea";   // ключ для шифрации sofuckingrandomimlaughingmyassoff
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
        public static string SHOP_KEY = "blahblahblah"; //ключ для хэширования id магазина somerandomshitphrase
    }
}
