using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopBy_Aisle.Data;
using ShopBy_Aisle.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShopBy_Aisle.Controllers
{
    public class ShoppingListController :Controller
    {
        private readonly ApplicationDbContext _context;
        public ShoppingListController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            List<MasterItem> myShoppingList = _context.MasterItems.Include(c=>c.PreferredStore).Include(c=>c.Aisle).Where(c => c.AddToShoppingList == true).ToList();
            ViewBag.stores = new SelectList(_context.Stores,"ID","Alias");
            return View(myShoppingList);
        }

    }
}
