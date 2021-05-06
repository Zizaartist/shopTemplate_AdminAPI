using ShopAdminAPI.Models.EnumModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopAdminAPI.Models
{
    public class OrganizationInfo
    {
        public string OrganizationName { get; set; }
        public string Avatar { get; set; }
        public string ActualAddress { get; set; }
        public string Email { get; set; }
        public string Phones { get; set; } //тупо хуярь все в одну строку с разделителями $
        public string Inn { get; set; }
        public string Ogrnip { get; set; }
        public string LegalAddress { get; set; }
        public string DeliveryTerms { get; set; }
        public decimal DeliveryPrice { get; set; }
        public int MaxPoints { get; set; }
        public string PaymentMethods { get; set; } //012 - каждый char это int значение enum модели

        [NotMapped]
        public string[] ActualPhones
        {
            get => Phones.Split('$');
            set
            {
                string phonesList = null;
                foreach (var phone in value)
                    phonesList += $"{phone}$";
                Phones = phonesList;
            }
        }
        [NotMapped]
        public List<PaymentMethod> ActualPaymentMethods
        {
            get
            {
                var pmList = new List<PaymentMethod>();
                foreach (var character in PaymentMethods)
                    pmList.Add((PaymentMethod)int.Parse(character.ToString()));
                return pmList;
            }
            set
            {
                string pms = null;
                foreach (var pm in value)
                    pms += (int)pm;
                PaymentMethods = pms;
            }
        }
    }
}
