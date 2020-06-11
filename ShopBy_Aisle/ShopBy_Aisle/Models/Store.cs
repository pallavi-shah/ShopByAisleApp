using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShopBy_Aisle.Models
{
    public class Store
    {
        public int ID { get; set; }
        [Display(Name="Store")]
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }

        //public List<Aisle> Aisles { get; set; }

        public List<MasterItem> MasterItems { get; set; }

        public string UserName { get; set; }

    }
}
