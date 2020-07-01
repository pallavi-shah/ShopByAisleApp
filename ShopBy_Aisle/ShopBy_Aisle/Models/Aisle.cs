using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShopBy_Aisle.Models
{
    public class Aisle
    {
        public int ID { get; set; }
        [Display(Name="Aisle")]
        public string Name { get; set; }

        [Display(Name = "Store")]
        public int StoreID { get; set; }
        public Store Store { get; set; }
        public string PlaceID { get; set; }
        public int CategoryID { get; set; }
        public Category Category { get; set; }
        public string Email { get; set; }


    }
}
