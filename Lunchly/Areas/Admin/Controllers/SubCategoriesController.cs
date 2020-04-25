using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lunchly.Data;
using Lunchly.Models.ViewModels;
using Lunchly.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Lunchly.Utility;

namespace Lunchly.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerUser)]

    [Area("Admin")]
    public class SubCategoriesController : Controller
    {   
        [TempData]
        public string StatusMessage { get; set; }
        private readonly ApplicationDbContext _db;
        public SubCategoriesController(ApplicationDbContext db) => _db = db;
        // GET /Admin/SubCategories
        public async Task<IActionResult> Index()
        {
            var subCategories = await _db.SubCategories.Include(s => s.Category).ToListAsync();
            return View(subCategories);
        }

        // GET /Admin/SubCategories/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new SubCategoryAndCategoryViewModel()
            {
                Categories = await _db.Categories.ToListAsync(),
                SubCategory = new SubCategory(),
                SubCategoriesList = await _db.SubCategories.OrderBy(s => s.Name)
                                                           .Select(s => s.Name)
                                                           .Distinct()
                                                           .ToListAsync(),
            };
            return View(viewModel);
        }

        // POST /Admin/SubCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var subCategoriesExistWithSameCateogry = _db.SubCategories
                                                       .Include(s => s.Category)
                                                       .Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);

                if (subCategoriesExistWithSameCateogry.Count() > 0)
                {
                    // Error Message
                    StatusMessage = "Error : Sub Category exists under + "
                                    + subCategoriesExistWithSameCateogry.First().Category.Name
                                    + " cateogry. Please use another name.";
                }
                else
                {
                    _db.SubCategories.Add(model.SubCategory);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            var viewModel = new SubCategoryAndCategoryViewModel()
            {
                Categories = await _db.Categories.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoriesList = await _db.SubCategories.OrderBy(s => s.Name).Select(s => s.Name).ToListAsync(),
                StatusMessage = StatusMessage
            };
            return View(viewModel);
        }
        // GET /Admin/SubCategories/Edit/Id
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var subCategory = await _db.SubCategories.FindAsync(id);
            if (subCategory == null)
                return NotFound();

            var viewModel = new SubCategoryAndCategoryViewModel()
            {
                Categories = await _db.Categories.ToListAsync(),
                SubCategory = subCategory,
                SubCategoriesList = await _db.SubCategories.OrderBy(s => s.Name).Select(s => s.Name).ToListAsync()
            };

            return View(viewModel);
        }
        // POST /Admin/SubCategories/Edit/Id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var subCategoriesExistWithSameCateogry = _db.SubCategories
                                                       .Include(s => s.Category)
                                                       .Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);

                if (subCategoriesExistWithSameCateogry.Count() > 0)
                {
                    // Error Message
                    StatusMessage = "Error : Sub Category exists under + "
                                    + subCategoriesExistWithSameCateogry.First().Category.Name
                                    + " cateogry. Please use another name.";
                }
                else
                {
                    var subCategoryInDb = await _db.SubCategories.FindAsync(model.SubCategory.Id);
                    subCategoryInDb.Name = model.SubCategory.Name;
                    await _db.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }
            var viewModel = new SubCategoryAndCategoryViewModel()
            {
                Categories = await _db.Categories.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoriesList = await _db.SubCategories.OrderBy(s => s.Name).Select(s => s.Name).ToListAsync(),
                StatusMessage = StatusMessage
            };
            return View(viewModel);
        }

        // GET /Admin/SubCategories/Details/Id
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();
            
            var subCategory = await _db.SubCategories
                              .Include(s => s.Category)
                              .SingleOrDefaultAsync(s => s.Id == id);
            if (subCategory == null)
                return NotFound();

            return View(subCategory);
        }   

        // GET /Admin/SubCategories/Delete/Id
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var subCategory = await _db.SubCategories
                              .Include(s => s.Category)
                              .SingleOrDefaultAsync(s => s.Id == id);
            if (subCategory == null)
                return NotFound();

            return View(subCategory);
        }

        // POST /Admin/SubCategories/Delete/Id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
                return NotFound();

            var subCategory = await _db.SubCategories.FindAsync(id);
            if (subCategory == null)
                return NotFound();

            _db.SubCategories.Remove(subCategory);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetSubCategories(int id)
        {
            var subCategories = await _db.SubCategories.Where(s => s.CategoryId == id).ToListAsync();
            return Json(new SelectList(subCategories, "Id", "Name"));
        }
    }
}