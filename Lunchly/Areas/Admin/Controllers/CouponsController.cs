using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lunchly.Data;
using Lunchly.Models;
using Lunchly.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lunchly.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerUser)]

    [Area("Admin")]
    public class CouponsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CouponsController(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            var coupons = await _db.Coupons.ToListAsync();
            return View(coupons);
        }
        // Get /Admin/Coupons/Create
        public IActionResult Create()
        {
            return View();
        }
        // POST /Admin/Coupons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            if (!ModelState.IsValid)
                return View(coupon);

            var files = HttpContext.Request.Form.Files;
            if(files.Count > 0)
            {
                byte[] picture = null;
                using (var fs1 = files[0].OpenReadStream())
                {
                    using (var ms1 = new MemoryStream())
                    {
                        fs1.CopyTo(ms1);
                        picture = ms1.ToArray();
                    }
                }
                coupon.Picture = picture;
            }
            _db.Coupons.Add(coupon);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // GET /Admin/Coupons/Edit/Id
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var coupon = await _db.Coupons.FindAsync(id);
            if (coupon == null)
                return NotFound();

            return View(coupon);
        }
        // POST /Admin/Coupons/Edit/Id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Coupon coupon)
        {
            if (!ModelState.IsValid)
                return View(coupon);

            var couponFromDb = await _db.Coupons.FindAsync(coupon.Id);
            var files = HttpContext.Request.Form.Files;
            if (files.Count > 0)
            {
                // New Coupon Image
                byte[] picture = null;
                using (var fs1 = files[0].OpenReadStream())
                {
                    using (var ms1 = new MemoryStream())
                    {
                        fs1.CopyTo(ms1);
                        picture = ms1.ToArray();
                    }
                }
                couponFromDb.Picture = picture;
            }
            couponFromDb.MinimumAmount = coupon.MinimumAmount;
            couponFromDb.Name = coupon.Name;
            couponFromDb.Discount = coupon.Discount;
            couponFromDb.CouponType = coupon.CouponType;
            couponFromDb.IsActive = coupon.IsActive;

            _db.Coupons.Update(couponFromDb);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        // GET /Admin/Coupons/Details/Id
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var coupon = await _db.Coupons.FindAsync(id);
            if (coupon == null)
                return NotFound();

            return View(coupon);
        }
        // GET /Admin/Coupons/Delete/Id
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var coupon = await _db.Coupons.FindAsync(id);
            if (coupon == null)
                return NotFound();

            return View(coupon);
        }
        // POST /Admin/Coupons/Delete/Id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
                return NotFound();

            var coupon = await _db.Coupons.FindAsync(id);
            if (coupon == null)
                return NotFound();

            _db.Coupons.Remove(coupon);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}