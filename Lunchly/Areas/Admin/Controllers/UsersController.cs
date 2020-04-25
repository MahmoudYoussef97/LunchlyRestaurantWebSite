using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Lunchly.Data;
using Lunchly.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lunchly.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerUser)]

    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db;
        public UsersController(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var users = await _db.ApplicationUsers.Where(u => u.Id != claim.Value).ToListAsync();
            
            return View(users);
        }
        public async Task<IActionResult> Lock(string id)
        {
            if (id == null)
                return NotFound();

            var applicationUser = await _db.ApplicationUsers.FindAsync(id);
            if (applicationUser == null)
                return NotFound();

            applicationUser.LockoutEnd = DateTime.Now.AddYears(100);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> UnLock(string id)
        {
            if (id == null)
                return NotFound();

            var applicationUser = await _db.ApplicationUsers.FindAsync(id);
            if (applicationUser == null)
                return NotFound();

            applicationUser.LockoutEnd = DateTime.Now;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}