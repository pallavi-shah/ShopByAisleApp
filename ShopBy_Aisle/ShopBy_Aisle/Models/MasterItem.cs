using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ShopBy_Aisle.Models
{
    public class MasterItem
    {
        public int ID { get; set; }
        [Required]
        [Display(Name = "Item")]
        //[RegularExpression("([a-zA-Z])", ErrorMessage ="Invalid Item Name")]
        [StringLength(20, MinimumLength = 3)]
        public string ItemName { get; set; }
        public Category Category { get; set; }
        [Display(Name = "Category")]
        public int CategoryID { get; set; }

        [Display(Name = "Store")]
        public int StoreID { get; set; }
        public Store PreferredStore { get; set; }

        public string Email { get; set; }
        public bool MasterList { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateShopped { get; set; }

        public int AisleID { get; set; }
        public Aisle Aisle { get; set; }
        public bool AddToShoppingList { get; set; }
    }
}
