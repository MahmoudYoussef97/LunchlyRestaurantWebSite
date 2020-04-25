using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lunchly.Data;
using Lunchly.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lunchly.Utility;
namespace Lunchly.Areas.Admin.Controllers
{   
    [Authorize(Roles = SD.ManagerUser)]
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoriesController(ApplicationDbContext db)
        {
            _db = db;
        }
        // GET /Admin/Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            return View(categories);
        }
        // GET /Admin/Categories/Create
        public IActionResult Create()
        {
            return View();
        }
        // POST /Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // GET /Admin/Cateogries/Edit/Id
        public async Task<IActionResult> Edit(int? id)
        {   
            if (id == null)
                return NotFound();

            var category = await _db.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }
        // POST /Admin/Categories/Edit/Id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            _db.Update(category);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // GET /Admin/Categories/Delete/Id
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var category = await _db.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }
        // POST /Admin/Categories/Delete/Id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {   
            var category = await _db.Categories.FindAsync(id);
            if (category == null)
                return View();

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var category = await _db.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

    }
}