using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopBy_Aisle.Data;
using ShopBy_Aisle.Models;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Identity;

namespace ShopByAisle.Controllers
{
    public class MasterItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MasterItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MasterItems
        
        public async Task<IActionResult> Index(string pg)
        {
                string CurrentUser = User.Identity.Name;
                if(pg=="ml")
                {
                    var applicationDbContext = _context.MasterItems.Where(c => c.UserName == CurrentUser && c.MasterList == true).Include(m => m.Category).Include(m => m.PreferredStore).Include(m => m.Aisle);
                    ViewData["CategoryID"] = new SelectList(_context.Categories, "ID", "Name");
                    ViewData["StoreID"] = new SelectList(_context.Stores, "ID", "Alias");
                    ViewData["Item"] = new MasterItem();
                    ViewData["pg"] = "ml";
                    ViewData["ml_hidden"] = "hidden";
                    ViewData["sl_hidden"] = "";
                    ViewData["ml_checked"] = "checked";
                    ViewData["sl_checked"] = "";
                return View(await applicationDbContext.ToListAsync());

                }
                 else
                {
                    var applicationDbContext = _context.MasterItems.Where(c => c.UserName == CurrentUser && c.AddToShoppingList==true).Include(m => m.Category).Include(m => m.PreferredStore).Include(m => m.Aisle);
                    ViewData["CategoryID"] = new SelectList(_context.Categories, "ID", "Name");
                    ViewData["StoreID"] = new SelectList(_context.Stores, "ID", "Alias");
                    ViewData["Item"] = new MasterItem();
                    ViewData["pg"] = "sl";
                    ViewData["ml_hidden"] = "";
                    ViewData["sl_hidden"] = "hidden";
                    ViewData["ml_checked"] = "";
                    ViewData["sl_checked"] = "checked";
                return View(await applicationDbContext.ToListAsync());
                }                
        }

        
        public async Task<IActionResult> FindItem(string searchTerm, string pg)
        {
            string CurrentUser = User.Identity.Name;
            if (searchTerm == null)
            {
                //var applicationDbContext = _context.MasterItems.Include(m => m.Category).Include(m => m.PreferredStore).Include(m => m.Aisle);
                TempData["Error"] = " Please Enter an item to search.";
                return Redirect("/MasterItems/Index?pg="+ pg);
            }

            else
            { 
                     var result = _context.MasterItems.Where(i => i.ItemName.Contains(searchTerm) && i.UserName==CurrentUser);
                        if(pg=="ml")
                        {
                            result.Where(i => i.MasterList == true);
                             ViewData["ml_hidden"] = "hidden";
                        }
                        else
                        {
                            result.Where(i => i.AddToShoppingList == true);
                            ViewData["sl_hidden"] = "hidden";
                }
                    ViewData["pg"] = pg;
                    var applicationDbContext = result.Include(m => m.Category).Include(m => m.PreferredStore).Include(m => m.Aisle);
                    ViewBag.Results = true;
                    if (result.Count()==0)
                    {
                       
                        TempData["Error"] = " No results found.";
                        return View("Index", await applicationDbContext.ToListAsync());
                    }
                     else
                    {
                    TempData["Message"] = "Search Results ";
                    return View("Index", await applicationDbContext.ToListAsync());
                    }
            }
        }
        // GET: MasterItems/Details/5
        /*public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var masterItem = await _context.MyMasterLists
                .Include(m => m.Category)
                .Include(m => m.PreferredStore)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (masterItem == null)
            {
                return NotFound();
            }

            return View(masterItem);
        }*/

        // GET: MasterItems/Create============================================================================
        /*public IActionResult Create()
        {
            MasterItem newMasterItem = new MasterItem();
            ViewData["CategoryID"] = new SelectList(_context.Categories, "ID", "Name");
            ViewData["StoreID"] = new SelectList(_context.Stores, "ID", "Alias");
            return View(newMasterItem);
        }*/

        // POST: MasterItems/Create   ============================================================================================================
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Create([Bind("ID,ItemName,Category,Store")] MasterItem masterItem)
        public async Task<IActionResult> Create(MasterItem masterItem)
        {

            /*Store store = _context.Stores.Single(s => s.ID == masterItem.StoreID);
            Category category = _context.Categories.Single(s => s.ID == masterItem.CategoryID);
            String CurrentUserID = User.Identity.Name;
            try
            {
                Aisle aisle = _context.Aisles.Single(a => a.Category == category && a.Store == store);
                masterItem.AisleID = aisle.ID;
            }
            catch (Exception)
            {
                Aisle aisle = new Aisle();
                aisle.Name = masterItem.Aisle.Name;
                aisle.StoreID = masterItem.StoreID;
                //newAisle.ID = _context.Aisles.Add(newAisle);
                aisle.CategoryID = masterItem.CategoryID;
                _context.Aisles.Add(aisle);
                await _context.SaveChangesAsync();
                masterItem.AisleID = aisle.ID;

            }*/
            Store store = null;
            Category category = null;
                String CurrentUserID = User.Identity.Name;
            if (masterItem.StoreID != 0 && masterItem.CategoryID != 0)
            {
                store = _context.Stores.Single(s => s.ID == masterItem.StoreID);
                category = _context.Categories.Single(s => s.ID == masterItem.CategoryID);
                try
                {
                    Aisle aisle = _context.Aisles.Single(a => a.Category == category && a.Store == store);
                    masterItem.AisleID = aisle.ID;
                }
                catch
                {
                    Aisle aisle = new Aisle();
                    aisle.Name = masterItem.Aisle.Name;
                    aisle.StoreID = masterItem.StoreID;
                    //newAisle.ID = _context.Aisles.Add(newAisle);
                    aisle.CategoryID = masterItem.CategoryID;
                    _context.Aisles.Add(aisle);
                    await _context.SaveChangesAsync();
                    masterItem.AisleID = aisle.ID;
                };
                if (ModelState.IsValid)
                {
                    masterItem.Category = category;
                    masterItem.PreferredStore = store;
                    masterItem.Aisle = _context.Aisles.Single(a => a.ID == masterItem.AisleID);
                    masterItem.UserName = CurrentUserID;
                    //masterItem.AddToShoppingList = true;
                    _context.MasterItems.Add(masterItem);
                    await _context.SaveChangesAsync();
                    return Redirect("/MasterItems/Index");
                };
            }
            else
            {
                masterItem.UserName = CurrentUserID;
                _context.MasterItems.Add(masterItem);
                await _context.SaveChangesAsync();
                return Redirect("/MasterItems/Index");
            }
            ViewData["CategoryID"] = new SelectList(_context.Categories, "ID", "Name", masterItem.ID);
            ViewData["StoreID"] = new SelectList(_context.Stores, "ID", "Alias", masterItem.StoreID);
            //ViewData["AisleID"] = new SelectList(_context.Aisles, "ID", "Name", masterItem.AisleID);
            ViewData["Aisle"] = _context.Aisles.Single(a => a.ID == masterItem.AisleID);
            return View(masterItem);
        }


        // GET: MasterItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var masterItem = await _context.MasterItems.FindAsync(id);
            if (masterItem == null)
            {
                return NotFound();
            }
            ViewData["CategoryID"] = new SelectList(_context.Categories, "ID", "Name", masterItem.CategoryID);
            ViewData["StoreID"] = new SelectList(_context.Stores, "ID", "Alias", masterItem.StoreID);
            Aisle aisle = _context.Aisles.Single(a => a.ID == masterItem.AisleID);
            
            //ViewBag.aisleName = aisle.Name;
            return View(masterItem);
        }

        // POST: MasterItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("ID,ItemName,CategoryID,StoreID")] MasterItem masterItem)
        public async Task<IActionResult> Edit(int id, MasterItem masterItem)
        {
            if (id != masterItem.ID)
            {
                return NotFound();
            }
            Store store = _context.Stores.Single(s => s.ID == masterItem.StoreID);
            Category category = _context.Categories.Single(s => s.ID == masterItem.CategoryID);
            try
            {
                Aisle aisle = _context.Aisles.Single(a => a.Category == category && a.Store == store);
                masterItem.AisleID = aisle.ID;
            }
            catch (Exception)
            {

                Aisle aisle = new Aisle();
                aisle.Name = masterItem.Aisle.Name;
                aisle.StoreID = masterItem.StoreID;
                //newAisle.ID = _context.Aisles.Add(newAisle);
                aisle.CategoryID = masterItem.CategoryID;
                _context.Aisles.Add(aisle);
                await _context.SaveChangesAsync();
                masterItem.AisleID = aisle.ID;
            }

            if (ModelState.IsValid)
            {
                masterItem.Category = category;
                masterItem.PreferredStore = store;
                masterItem.Aisle = _context.Aisles.Single(a => a.ID == masterItem.AisleID);
                _context.MasterItems.Update(masterItem);
                await _context.SaveChangesAsync();
                return Redirect("/MasterItems/Index");
            }

            /*if (ModelState.IsValid)
            {
               try
                {
                    _context.Update(masterItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MasterItemExists(masterItem.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }*/

            //            return RedirectToAction(nameof(Index));
            //          }
            ViewData["CategoryID"] = new SelectList(_context.Categories, "ID", "Name", masterItem.CategoryID);
            ViewData["StoreID"] = new SelectList(_context.Stores, "ID", "Alias", masterItem.StoreID);
            ViewData["Aisle"] = _context.Aisles.Single(a => a.ID == masterItem.AisleID);
            return View(masterItem);
        }

        // GET: MasterItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var masterItem = await _context.MasterItems
                .Include(m => m.Category)
                .Include(m => m.PreferredStore)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (masterItem == null)
            {
                return NotFound();
            }

            return View(masterItem);
        }

        // POST: MasterItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var masterItem = await _context.MasterItems.FindAsync(id);
            _context.MasterItems.Remove(masterItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MasterItemExists(int id)
        {
            return _context.MasterItems.Any(e => e.ID == id);
        }

        [HttpPost]
        public JsonResult GetAisleData(string JsonStr)
        {
            JObject jsondata = JObject.Parse(JsonStr);
            int int_CatID = Convert.ToInt32(jsondata["catID"]);
            int int_StoreID = Convert.ToInt32(jsondata["storeID"]);
            try
            {
                Aisle aisle = _context.Aisles.Single(x => x.CategoryID == int_CatID && x.StoreID == int_StoreID);
                return Json(new { success = true, aisleName = aisle.Name, aisleID = aisle.ID });
            }
            catch (Exception)
            {
                return Json(new { success = true, aisleName = "", aisleID = 0 });
            }
        }

        public JsonResult UpdateFlag(string JsonStr)
        {
            JObject jsondata = JObject.Parse(JsonStr);
            bool checked_val = Convert.ToBoolean(jsondata["selected"]);
            int item_id= Convert.ToInt32(jsondata["itemID"]);
            string name = Convert.ToString(jsondata["name"]);
            MasterItem m = _context.MasterItems.Single(m => m.ID == item_id);
            if (name == "add_master")
            {
                m.MasterList = checked_val;
            }
            else if (name=="add_shop")
            {
                m.AddToShoppingList = checked_val;
            }
            _context.SaveChanges();
            return Json(new {success=true });
        }
       
    }

}
