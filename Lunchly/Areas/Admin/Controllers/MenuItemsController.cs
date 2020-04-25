using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lunchly.Data;
using Lunchly.Models.ViewModels;
using Lunchly.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lunchly.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerUser)]

    [Area("Admin")]
    public class MenuItemsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _hostingEnvironment;
        [BindProperty]
        public MenuItemViewModel MenuItemViewModel { get; set; }
        public MenuItemsController(ApplicationDbContext db, IWebHostEnvironment hostingEnivronment)
        {
            _db = db;
            _hostingEnvironment = hostingEnivronment;
            MenuItemViewModel = new MenuItemViewModel()
            {
                Categories = _db.Categories,
                MenuItem = new Models.MenuItem()
            };
        }
        public async Task<IActionResult> Index()
        {
            var menuItems = await _db.MenuItems
                            .Include(m => m.Category)
                            .Include(m => m.SubCategory)
                            .ToListAsync();
            return View(menuItems);
        }
        // Get /Admin/MenuItems/Create
        public IActionResult Create()
        {
            return View(MenuItemViewModel);
        }
        // POST /Admin/MenuItems/Create
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST()
        {
            MenuItemViewModel.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (!ModelState.IsValid)
                return View(MenuItemViewModel);

            _db.MenuItems.Add(MenuItemViewModel.MenuItem);
            await _db.SaveChangesAsync();

            var webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            var menuItemFromDb = await _db.MenuItems.FindAsync(MenuItemViewModel.MenuItem.Id);
            
            if (files.Count > 0)
            {
                var uploads = Path.Combine(webRootPath, "Images");
                var extension = Path.GetExtension(files[0].FileName);
                using (var fileStream = new FileStream(Path.Combine(uploads, MenuItemViewModel.MenuItem.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }
                menuItemFromDb.Image = @"\images\" + MenuItemViewModel.MenuItem.Id + extension;
            }
            else
            {
                var uploads = Path.Combine(webRootPath, @"Images\"+SD.DefaultImage);
                System.IO.File.Copy(uploads, webRootPath + @"\images\" + MenuItemViewModel.MenuItem.Id + ".png");
                menuItemFromDb.Image = @"\images\" + MenuItemViewModel.MenuItem.Id + ".png";
            }
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // Get /Admin/MenuItems/Edit/Id
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var menuItem = await  _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m =>m.Id == id);
            var subCategories = await _db.SubCategories.Where(s => s.CategoryId == MenuItemViewModel.MenuItem.CategoryId).ToListAsync();
            MenuItemViewModel.MenuItem = menuItem;
            MenuItemViewModel.SubCategories = subCategories;
            
            if (MenuItemViewModel.MenuItem == null)
                return NotFound();

            return View(MenuItemViewModel);  
        }
        // POST /Admin/MenuItems/Edit/Id
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPOST(int? id)
        {
            if (id == null)
                return NotFound();

            MenuItemViewModel.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            
            if (!ModelState.IsValid)
            {
                var subCategories = await _db.SubCategories.Where(s => s.CategoryId == MenuItemViewModel.MenuItem.CategoryId).ToListAsync();
                MenuItemViewModel.SubCategories = subCategories;
                return View(MenuItemViewModel);
            }

            var webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            var menuItemFromDb = await _db.MenuItems.FindAsync(MenuItemViewModel.MenuItem.Id);

            if (files.Count > 0)
            {   
                // New Image
                var uploads = Path.Combine(webRootPath, "Images");
                var extension = Path.GetExtension(files[0].FileName);
                // Delete original Image
                var imagePath = Path.Combine(webRootPath, menuItemFromDb.Image.TrimStart('\\'));
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
                // Uploading the new file
                using (var fileStream = new FileStream(Path.Combine(uploads, MenuItemViewModel.MenuItem.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }
                menuItemFromDb.Image = @"\images\" + MenuItemViewModel.MenuItem.Id + extension;
            }

            menuItemFromDb.Name = MenuItemViewModel.MenuItem.Name;
            menuItemFromDb.Description = MenuItemViewModel.MenuItem.Description;
            menuItemFromDb.Price = MenuItemViewModel.MenuItem.Price;
            menuItemFromDb.Spicyness = MenuItemViewModel.MenuItem.Spicyness;
            menuItemFromDb.CategoryId = MenuItemViewModel.MenuItem.CategoryId;
            menuItemFromDb.SubCategoryId = MenuItemViewModel.MenuItem.SubCategoryId;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // GET /Admin/MenuItems/Deatils/Id
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var menuItem = await _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            var subCategories = await _db.SubCategories.Where(s => s.CategoryId == MenuItemViewModel.MenuItem.CategoryId).ToListAsync();
            MenuItemViewModel.MenuItem = menuItem;
            MenuItemViewModel.SubCategories = subCategories;

            if (MenuItemViewModel.MenuItem == null)
                return NotFound();

            return View(MenuItemViewModel);
        }
        // GET /Admin/MenuItems/Delete/Id
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            MenuItemViewModel.MenuItem = await _db.MenuItems
                                .Include(m => m.Category)
                                .Include(m => m.SubCategory)
                                .SingleOrDefaultAsync(m => m.Id == id);
            if (MenuItemViewModel.MenuItem == null)
                return NotFound();
            MenuItemViewModel.SubCategories = await _db.SubCategories
                                              .Where(s => s.CategoryId == MenuItemViewModel.MenuItem.CategoryId)
                                              .ToListAsync();
            return View(MenuItemViewModel);
        }
        // POST /Admin/MenuItems/Delete/Id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
                return NotFound();

            var menuItem = await _db.MenuItems.FindAsync(id);
            if (menuItem == null)
                return NotFound();
            // Delete original Image
            var webRootPath = _hostingEnvironment.WebRootPath;
            var imagePath = Path.Combine(webRootPath, menuItem.Image.TrimStart('\\'));
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);
            _db.MenuItems.Remove(menuItem);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}