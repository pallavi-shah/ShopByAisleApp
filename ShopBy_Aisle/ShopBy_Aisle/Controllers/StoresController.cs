using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopBy_Aisle.Data;
using ShopBy_Aisle.Models;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Identity;

namespace ShopByAisle.Controllers
{
    public class StoresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Stores
        public async Task<IActionResult> Index()
        {
            string currentUser = User.Identity.Name;
            List<Store> Stores = await _context.Stores.Where(s=>s.UserName==currentUser).ToListAsync();
            return View(Stores);
            //return View();
        }

        public async Task<IActionResult> Maps()
        {
            return View(await _context.Stores.ToListAsync());
            //return View();
        }

        // GET: Stores/Details/5
        /*public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var store = await _context.Stores
                .FirstOrDefaultAsync(m => m.ID == id);
            if (store == null)
            {
                return NotFound();
            }

            return View(store);
        }*/

        // GET: Stores/Create
        public IActionResult Create()
        {

            return View();
        }

        // POST: Stores/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("ID,Name,Alias,Address")] Store store)
        public async Task<IActionResult> Create([Bind("ID,Name,Alias,Address")] Store store)
        {
            if (ModelState.IsValid)
            {
                string currentUser = User.Identity.Name;
                store.UserName = currentUser;
                _context.Add(store);
                await _context.SaveChangesAsync();
                return Redirect("Index");
            }

            return View(store);
        }

        // GET: Stores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var store = await _context.Stores.FindAsync(id);
            if (store == null)
            {
                return NotFound();
            }
            return View(store);
        }

        // POST: Stores/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Alias,Address")] Store store)
        {
            if (id != store.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(store);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StoreExists(store.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(store);
        }

        // GET: Stores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var store = await _context.Stores
                .FirstOrDefaultAsync(m => m.ID == id);
            if (store == null)
            {
                return NotFound();
            }

            return View(store);
        }

        // POST: Stores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var store = await _context.Stores.FindAsync(id);
            _context.Stores.Remove(store);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StoreExists(int id)
        {
            return _context.Stores.Any(e => e.ID == id);
        }

        //Method to add store from Google maps.
        public JsonResult AddStore(string JsonStr)
        {
            JObject jsondata = JObject.Parse(JsonStr);
            try
            {
                Store myStore = _context.Stores.Single(s => s.Address == jsondata["address"].ToString());
                return Json(new { success = true, message = "Store already exists in your favorites." });

            }
            catch (Exception)
            {
                try
                {
                    Store myStore = new Store();
                    myStore.Name = jsondata["name"].ToString(); ;
                    myStore.Address = jsondata["address"].ToString();
                    myStore.Alias = jsondata["alias"].ToString();
                    myStore.UserName = User.Identity.Name;
                    _context.Stores.Add(myStore);
                    _context.SaveChanges();
                    jsondata["id"] = myStore.ID;
                    return Json(new { success = true, message = "Store Added to your favorites.",nsID=myStore.ID, 
                    nsAlias=myStore.Alias, nsAddress = myStore.Address, nsUserName=myStore.UserName, nsName= myStore.Name });

                }
                catch (Exception)
                {
                    return Json(new { success = true, message = "Error, Please Try again." });
                }
            }

        }
    }
}
