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
using System.Data;

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
            ViewData["sortByAisle"] = new List<SelectListItem> {
                    new SelectListItem{ Text = "Aisle",  Value = "1"},
                    new SelectListItem{ Text = "Category",  Value = "2"}
                };

            if (pg == "ml")
            {
                var applicationDbContext = _context.MasterItems.Where(c => c.UserName == CurrentUser && c.MasterList == true).OrderBy(m => m.ItemName).Include(m => m.Category).Include(m => m.PreferredStore).Include(m => m.Aisle);
                ViewData["CategoryID"] = new SelectList(_context.Categories, "ID", "Name");
                ViewData["StoreID"] = new SelectList(_context.Stores.Where(s=>s.UserName==CurrentUser), "ID", "Alias");
                ViewData["selectedStore"] = 0;
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
                var applicationDbContext = _context.MasterItems.Where(c => c.UserName == CurrentUser && c.AddToShoppingList == true).Include(m => m.Category).Include(m => m.PreferredStore).Include(m => m.Aisle);
                ViewData["CategoryID"] = new SelectList(_context.Categories, "ID", "Name");
                ViewData["selectedStore"] = 0;
                ViewData["StoreID"] = new SelectList(_context.Stores.Where(s => s.UserName == CurrentUser), "ID", "Alias");
                ViewData["Item"] = new MasterItem();
                ViewData["pg"] = "sl";
                ViewData["ml_hidden"] = "";
                ViewData["sl_hidden"] = "hidden";
                ViewData["ml_checked"] = "";
                ViewData["sl_checked"] = "checked";
                return View(await applicationDbContext.ToListAsync());
            }
        }


        // Method for Search Items/Filter by store/Sort by aisle & category/Clear Shopped Items
        public async Task<IActionResult> FindItem(string searchTerm, string pg, int storeId, int sortByColumn,string myaction)
        {
            string CurrentUser = User.Identity.Name;

            // Code to clear Shopped Items from shopping List
            if (myaction== "clearshoppeditem")
            {
                    IEnumerable<MasterItem> shoppedItems = _context.MasterItems.Where(c => c.Shopped == true);
                    foreach (MasterItem item in shoppedItems)
                    {
                        item.Shopped = false;
                        item.AddToShoppingList = false;
                    }
                _context.SaveChanges();
            }

            {
                // To retrieve specific List for purpose of search based on ml/sl:
                IQueryable<MasterItem> result = null;
                result = _context.MasterItems.Where(i => i.UserName == CurrentUser);
                if (pg == "ml")
                {
                    result = result.Where(i => i.MasterList == true);
                    ViewData["ml_hidden"] = "hidden";
                }
                else
                {
                    result = result.Where(i => i.AddToShoppingList == true);
                    ViewData["sl_hidden"] = "hidden";
                }
                
                // Search Items from the List
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    result = result.Where(i => i.ItemName.Contains(searchTerm));
                }
                
                // Filter by Store
                if (storeId != 0)
                {
                    result = result.Where(c => c.StoreID == storeId);
                }
                result = result.Include(m => m.Category).Include(m => m.PreferredStore).Include(m => m.Aisle);
                
                // Sort List
                if (sortByColumn != 0)
                {
                    if (sortByColumn == 1)
                    {
                        result = result.OrderBy(c => c.Aisle.Name);
                    }
                    else if(sortByColumn == 2)
                    {
                        result = result.OrderBy(c => c.Category.Name);
                    }
                }
                
                ViewBag.Results = true;
                ViewData["pg"] = pg;
                ViewData["StoreID"] = new SelectList(_context.Stores.Where(s=>s.UserName==CurrentUser), "ID", "Alias", storeId);
                ViewData["CategoryID"] = new SelectList(_context.Categories, "ID", "Name");
                ViewData["searchTerm"] = searchTerm;

               
                ViewData["sortByAisle"] = new List<SelectListItem> 
                {
                    new SelectListItem{ Text = "Aisle",  Value = "1", Selected = sortByColumn==1? true: false},
                    new SelectListItem{ Text = "Category",  Value = "2", Selected = sortByColumn==2? true: false}
                };
                
                if (result.Count()==0)
                {
                    TempData["Error"] = "No results found";
                    return View("Index", await result.ToListAsync());
                }
                else
                {
                    if (myaction == "clearshoppeditem")
                    {
                    TempData["Message"] = "Shopped Items cleared ";
                    return View("Index", await result.ToListAsync());
                    }
                    else
                    {
                        TempData["Message"] = "Search Results";
                        return View("Index", await result.ToListAsync());
                    }
                }
            }
        }
        
        // POST: MasterItems/

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MasterItem masterItem, string submit,string pg)
        {
            if (submit == "Goback")
            {
                return View("Index");
            }
            else
            {
                String CurrentUserID = User.Identity.Name;
                masterItem.UserName = CurrentUserID;
                Store store;
                Category category;

                // To  Retrieve/add/Update Aisle Information based Store and Category
                if (masterItem.StoreID != null && masterItem.CategoryID != null)
                {
                    store = _context.Stores.Single(s => s.ID == masterItem.StoreID);
                    category = _context.Categories.Single(s => s.ID == masterItem.CategoryID);
                    masterItem.PreferredStore = store;
                    masterItem.Category = category;
                    try
                    {
                        Aisle aisle = _context.Aisles.Single(a => a.Category == masterItem.Category && a.PlaceID == store.PlaceID);
                        masterItem.AisleID = aisle.ID;
                        aisle.Name = masterItem.Aisle.Name;
                    }
                    catch (Exception)
                    {
                        Aisle aisle = new Aisle();
                       try
                        {
                            aisle.Name = masterItem.Aisle.Name.ToUpper();
                        }
                        catch
                        {
                            aisle.Name = masterItem.Aisle.Name;
                        }
                        aisle.StoreID = (int)masterItem.StoreID;
                        aisle.CategoryID = (int)masterItem.CategoryID;
                        aisle.PlaceID = store.PlaceID;
                        _context.Aisles.Add(aisle);
                        await _context.SaveChangesAsync();
                        masterItem.AisleID = aisle.ID;
                    }

                    if (ModelState.IsValid)
                    {
                        masterItem.Aisle = _context.Aisles.Single(a => a.ID == masterItem.AisleID);
                        _context.MasterItems.Add(masterItem);
                        await _context.SaveChangesAsync();
                        return Redirect("/MasterItems/Index?pg=" + pg);
                    }
                }
                else if (masterItem.StoreID == null)
                {
                    ViewData["store"] = masterItem.PreferredStore;
                    masterItem.Aisle = null;
                    _context.MasterItems.Add(masterItem);
                    await _context.SaveChangesAsync();
                    return Redirect("/MasterItems/Index?pg=" + pg);
                }
                ViewData["CategoryID"] = new SelectList(_context.Categories, "ID", "Name", masterItem.ID);
                ViewData["StoreID"] = new SelectList(_context.Stores, "ID", "Alias", masterItem.StoreID);
                return View(masterItem);
            }
        }


        // GET: MasterItems/Edit/5
        public async Task<IActionResult> Edit(int? id, string pg)
        {

            ViewData["pg"] = pg;
            String CurrentUserID = User.Identity.Name;
            var item = _context.MasterItems.Single(m => m.ID == id);
            if (pg == "ml")
            {
                ViewData["ml_hidden"] = "hidden";
                ViewData["sl_hidden"] = "";
            }
            else
            {
                ViewData["ml_hidden"] = "";
                ViewData["sl_hidden"] = "hidden";
            }

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
            ViewData["StoreID"] = new SelectList(_context.Stores.Where(s => s.UserName == CurrentUserID), "ID", "Alias", masterItem.StoreID);
          
            try
            {
                 Aisle aisle = _context.Aisles.Single(a => a.ID == masterItem.AisleID);
            }
            catch
            {
                masterItem.Aisle = null;
            }
            return View(masterItem);
        }

        // POST: MasterItems/Edit/5
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,MasterItem masterItem, string pg)
        {
            String CurrentUserID = User.Identity.Name;
            masterItem.UserName = CurrentUserID;
            Store store = _context.Stores.Single(s => s.ID == masterItem.StoreID);
            Category category = _context.Categories.Single(s => s.ID == masterItem.CategoryID);
            try
            {
                Aisle aisle = _context.Aisles.Single(a => a.Category == category && a.PlaceID == store.PlaceID);
                masterItem.AisleID = aisle.ID;
                aisle.Name = masterItem.Aisle.Name;
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                Aisle aisle = new Aisle();
                aisle.Name = masterItem.Aisle.Name;
                aisle.StoreID = (int)masterItem.StoreID;
                aisle.CategoryID = (int)masterItem.CategoryID;
                aisle.PlaceID = store.PlaceID; 
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
                return Redirect("/MasterItems/Index?pg="+pg);
            }
            ViewData["CategoryID"] = new SelectList(_context.Categories, "ID", "Name", masterItem.CategoryID);
            ViewData["StoreID"] = new SelectList(_context.Stores.Where(s => s.UserName == CurrentUserID), "ID", "Alias", masterItem.StoreID);
            ViewData["Aisle"] = _context.Aisles.Single(a => a.ID == masterItem.AisleID);
            
            if (pg == "ml")
            {
                ViewData["ml_hidden"] = "hidden";
                ViewData["sl_hidden"] = "";
            } else
            {
                ViewData["ml_hidden"] = "";
                ViewData["sl_hidden"] = "hidden";
            }
            ViewData["pg"] = pg;
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


        // GET : Aisle Information that already exists in database based on store and Category info
        [HttpPost]
        public JsonResult GetAisleData(string JsonStr)
        {
            int int_CatID;
            int int_StoreID;
            try
            {
                JObject jsondata = JObject.Parse(JsonStr);
                int_CatID= Convert.ToInt32(jsondata["catID"]);
                int_StoreID= Convert.ToInt32(jsondata["storeID"]);
                Store myStore = _context.Stores.Single(s => s.ID == int_StoreID);
                Aisle aisle = _context.Aisles.Single(x => x.CategoryID == int_CatID && x.PlaceID == myStore.PlaceID);
                return Json(new { success = true, aisleName = aisle.Name, aisleID = aisle.ID });
            }
            catch (Exception)
            {
                return Json(new { success = true, aisleName = "", aisleID = 0 });
            }
        }

        //Method to update Add to Shopping List/Add to Favorites flag when clicked.
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
            else if(name=="shopped")
            {
                m.Shopped = checked_val;
            }
            _context.SaveChanges();
            return Json(new {success=true });
        }
    }
}
