using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopBy_Aisle.Data;
using ShopBy_Aisle.Models;


namespace ShopByAisle.Controllers
{
    public class AislesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AislesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Aisles
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Aisles.Include(a => a.Store).Include(a => a.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Aisles/Create
        public IActionResult Create()
        {
            ViewData["StoreID"] = new SelectList(_context.Stores, "ID", "Alias");
            ViewData["CategoryID"] = new SelectList(_context.Categories, "ID", "Name");
            return View();
        }

        // POST: Aisles/Create
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Aisle aisle)
        {
            Aisle newAisle = new Aisle();
            if (ModelState.IsValid)
            {
                newAisle.Name = aisle.Name;
                newAisle.Store = _context.Stores.Single(s => s.ID == aisle.StoreID);
                newAisle.Category = _context.Categories.Single(c => c.ID == aisle.CategoryID);
                newAisle.Email = aisle.Email;
                _context.Add(newAisle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["StoreID"] = new SelectList(_context.Stores, "ID", "Alias", aisle.StoreID);
            ViewData["CategoryID"] = new SelectList(_context.Categories, "ID", "Name", aisle.CategoryID);
            return View(aisle);
        }

        // GET: Aisles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aisle = await _context.Aisles.FindAsync(id);
            if (aisle == null)
            {
                return NotFound();
            }
            ViewData["StoreID"] = new SelectList(_context.Stores, "ID", "Alias", aisle.StoreID);
            ViewData["CategoryID"] = new SelectList(_context.Categories, "ID", "Name", aisle.CategoryID);
            return View(aisle);
        }

        // POST: Aisles/Edit/5
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,StoreID,CategoryID,UserID")] Aisle aisle)
        {
            if (id != aisle.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(aisle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AisleExists(aisle.ID))
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
            ViewData["StoreID"] = new SelectList(_context.Stores, "ID", "Alias", aisle.StoreID);
            ViewData["CategoryID"] = new SelectList(_context.Categories, "ID", "Name", aisle.CategoryID);
            return View(aisle);
        }

        // GET: Aisles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aisle = await _context.Aisles
                .Include(a => a.Store)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (aisle == null)
            {
                return NotFound();
            }

            return View(aisle);
        }

        // POST: Aisles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aisle = await _context.Aisles.FindAsync(id);
            _context.Aisles.Remove(aisle);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AisleExists(int id)
        {
            return _context.Aisles.Any(e => e.ID == id);
        }
    }
}
