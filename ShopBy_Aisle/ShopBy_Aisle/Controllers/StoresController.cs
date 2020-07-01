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
        }

        // POST: Stores/Create
        
        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,string Alias)
        {
            Store store = _context.Stores.Single(s => s.ID == id);
            store.Alias = Alias;
            if (ModelState.IsValid)
            {
                try
                {
                    string currentUser = User.Identity.Name;
                    store.UserName = currentUser;
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
            IEnumerable<MasterItem> masterItems = _context.MasterItems.Where(m => m.StoreID == id);
            foreach(MasterItem m in masterItems)
            {
                _context.MasterItems.Remove(m);
            }
            store.UserName = "";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StoreExists(int id)
        {
            return _context.Stores.Any(e => e.ID == id);
        }
        
        //Method to add store from Google maps using ajax call
        public JsonResult AddStore(string JsonStr)
        {
            string currentUser = User.Identity.Name;
            JObject jsondata = JObject.Parse(JsonStr);
            try
            {
                Store myStore = _context.Stores.Single(s => s.Address == jsondata["address"].ToString() && s.UserName==currentUser);
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
                    myStore.PlaceID = jsondata["place_id"].ToString();
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
