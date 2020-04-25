using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lunchly.Data;
using Microsoft.AspNetCore.Mvc;
using Lunchly.Models.ViewModels;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Lunchly.Utility;
using Microsoft.AspNetCore.Http;
using Lunchly.Models;
using Stripe;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authorization;

namespace Lunchly.Areas.Customer.Controllers
{   
    [Authorize]
    [Area("Customer")]
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        [BindProperty]
        public OrderDetailsViewModel OrderDetailsVM { get; set; }
        public CartsController(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }
       
        public async Task<IActionResult> Index()
        {
            OrderDetailsVM = new OrderDetailsViewModel()
            {
                OrderHeader = new Models.OrderHeader()
            };
            OrderDetailsVM.OrderHeader.TotalOrderPrice = 0;
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var carts = await _db.ShoppingCarts.
                        Include(s => s.MenuItem)
                        .Where(u => u.ApplicationUserId == claim.Value).ToListAsync();

            if (carts != null)
                OrderDetailsVM.ShoppingCarts = carts;

            foreach (var cart in OrderDetailsVM.ShoppingCarts)
            {
                OrderDetailsVM.OrderHeader.TotalOrderPrice += (cart.MenuItem.Price * cart.NumberOfItems);
                cart.MenuItem.Description = SD.ConvertToRawHtml(cart.MenuItem.Description);
                if (cart.MenuItem.Description.Length > 100)
                    cart.MenuItem.Description = cart.MenuItem.Description.Substring(0, 99) + "...";
            }
            OrderDetailsVM.OrderHeader.OriginalTotalOrderPrice = Math.Round(OrderDetailsVM.OrderHeader.TotalOrderPrice, 2);
            OrderDetailsVM.OrderHeader.TotalOrderPrice = Math.Round(OrderDetailsVM.OrderHeader.TotalOrderPrice, 2);
            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                OrderDetailsVM.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var coupon = await _db.Coupons.Where(c => c.Name.ToLower() == OrderDetailsVM.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                OrderDetailsVM.OrderHeader.TotalOrderPrice = SD.DiscountedPrice(coupon, OrderDetailsVM.OrderHeader.OriginalTotalOrderPrice);
            }
            return View(OrderDetailsVM);
        }
        public async Task<IActionResult> Summary()
        {
            OrderDetailsVM = new OrderDetailsViewModel()
            {
                OrderHeader = new Models.OrderHeader()
            };
            OrderDetailsVM.OrderHeader.TotalOrderPrice = 0;
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var carts = await _db.ShoppingCarts.
                        Include(s => s.MenuItem)
                        .Where(u => u.ApplicationUserId == claim.Value).ToListAsync();
            var user = await _db.ApplicationUsers.FindAsync(claim.Value);
            if (carts != null)
                OrderDetailsVM.ShoppingCarts = carts;

            foreach (var cart in OrderDetailsVM.ShoppingCarts)
            {
                OrderDetailsVM.OrderHeader.TotalOrderPrice += (cart.MenuItem.Price * cart.NumberOfItems);
                cart.MenuItem.Description = SD.ConvertToRawHtml(cart.MenuItem.Description);
            }
            OrderDetailsVM.OrderHeader.OriginalTotalOrderPrice = Math.Round(OrderDetailsVM.OrderHeader.TotalOrderPrice, 2);
            OrderDetailsVM.OrderHeader.TotalOrderPrice = Math.Round(OrderDetailsVM.OrderHeader.TotalOrderPrice, 2);
            OrderDetailsVM.OrderHeader.PickupName = user.Name;
            OrderDetailsVM.OrderHeader.PhoneNumber = user.PhoneNumber;
            OrderDetailsVM.OrderHeader.PickupTime = DateTime.Now;

            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                OrderDetailsVM.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var coupon = await _db.Coupons.Where(c => c.Name.ToLower() == OrderDetailsVM.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                OrderDetailsVM.OrderHeader.TotalOrderPrice = SD.DiscountedPrice(coupon, OrderDetailsVM.OrderHeader.OriginalTotalOrderPrice);
            }
            return View(OrderDetailsVM);
        }

        [HttpPost, ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SummaryPost(string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            OrderDetailsVM.ShoppingCarts = await _db.ShoppingCarts.Where(s => s.ApplicationUserId == claim.Value).ToListAsync();

            OrderDetailsVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            OrderDetailsVM.OrderHeader.OrderDate = DateTime.Now;
            OrderDetailsVM.OrderHeader.UserId = claim.Value;
            OrderDetailsVM.OrderHeader.Status = SD.PaymentStatusPending;
            OrderDetailsVM.OrderHeader.PickupTime = Convert.ToDateTime(OrderDetailsVM.OrderHeader.PicupDate.ToShortDateString() + " " + OrderDetailsVM.OrderHeader.PickupTime.ToShortTimeString());

            var orderDetailsList = new List<OrderDetails>();
            _db.OrderHeader.Add(OrderDetailsVM.OrderHeader);
            await _db.SaveChangesAsync();

            OrderDetailsVM.OrderHeader.OriginalTotalOrderPrice = 0;

            foreach (var cart in OrderDetailsVM.ShoppingCarts)
            {
                cart.MenuItem = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == cart.MenuItemId);
                var orderDetails = new OrderDetails
                {
                    MenuItemId = cart.MenuItemId,
                    OrderId = OrderDetailsVM.OrderHeader.Id,
                    Description = cart.MenuItem.Description,
                    Name = cart.MenuItem.Name,
                    Price = cart.MenuItem.Price,
                    Count = cart.NumberOfItems
                };
                OrderDetailsVM.OrderHeader.OriginalTotalOrderPrice += orderDetails.Count * orderDetails.Price;
                _db.OrderDetails.Add(orderDetails);
            }


            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                OrderDetailsVM.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var coupon = await _db.Coupons.Where(c => c.Name.ToLower() == OrderDetailsVM.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                OrderDetailsVM.OrderHeader.TotalOrderPrice = SD.DiscountedPrice(coupon, OrderDetailsVM.OrderHeader.OriginalTotalOrderPrice);
            }
            else
            {
                OrderDetailsVM.OrderHeader.TotalOrderPrice = OrderDetailsVM.OrderHeader.OriginalTotalOrderPrice;
            }
            OrderDetailsVM.OrderHeader.CouponCodeDiscount = OrderDetailsVM.OrderHeader.OriginalTotalOrderPrice - OrderDetailsVM.OrderHeader.TotalOrderPrice;
            _db.ShoppingCarts.RemoveRange(OrderDetailsVM.ShoppingCarts);
            HttpContext.Session.SetInt32("ssCartCount", 0);
            await _db.SaveChangesAsync();

            var options = new ChargeCreateOptions
            {
                Amount = Convert.ToInt32(OrderDetailsVM.OrderHeader.TotalOrderPrice * 100),
                Currency = "EGP",
                Description = "Order ID : " + OrderDetailsVM.OrderHeader.Id,
                Source = stripeToken
            };
            var service = new ChargeService();
            Charge charge = service.Create(options);

            if (charge.BalanceTransactionId == null)
            {
                OrderDetailsVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            }
            else
            {
                OrderDetailsVM.OrderHeader.TransactionId = charge.BalanceTransactionId;
            }

            if (charge.Status.ToLower() == "succeeded")
            {
                var userEmail = _db.Users.Where(u => u.Id == claim.Value).FirstOrDefault().Email;
                await _emailSender.SendEmailAsync(userEmail, "Lunchly - Order Created" + OrderDetailsVM.OrderHeader.Id.ToString(), "Order has been submitted successfully...");
                OrderDetailsVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                OrderDetailsVM.OrderHeader.Status = SD.StatusSubmitted;
            }
            else
            {
                OrderDetailsVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Confirm", "Orders", new { id = OrderDetailsVM.OrderHeader.Id});
        }
        public IActionResult AddCoupon()
        {
            if (OrderDetailsVM.OrderHeader.CouponCode == null)
                OrderDetailsVM.OrderHeader.CouponCode = "";

            HttpContext.Session.SetString(SD.ssCouponCode, OrderDetailsVM.OrderHeader.CouponCode);
            return RedirectToAction(nameof(Index));
        }
        public IActionResult RemoveCoupon()
        {
            HttpContext.Session.SetString(SD.ssCouponCode, string.Empty);
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Plus(int cartId)
        {
            var shoppingCart = await _db.ShoppingCarts.Where(s => s.Id == cartId).FirstOrDefaultAsync();
            shoppingCart.NumberOfItems += 1;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Minus(int cartId)
        {
            var shoppingCart = await _db.ShoppingCarts.Where(s => s.Id == cartId).FirstOrDefaultAsync();
            if (shoppingCart.NumberOfItems == 1)
            {
                _db.ShoppingCarts.Remove(shoppingCart);
                await _db.SaveChangesAsync();

                var carts = _db.ShoppingCarts
                            .Where(s => s.ApplicationUserId == shoppingCart.ApplicationUserId)
                            .ToList().Count;
                HttpContext.Session.SetInt32("ssCartCount", carts);
            }
            else
            {
                shoppingCart.NumberOfItems -= 1;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Remove(int cartId)
        {
            var shoppingCart = await _db.ShoppingCarts.FindAsync(cartId);
            _db.ShoppingCarts.Remove(shoppingCart);
            await _db.SaveChangesAsync();

            var carts = _db.ShoppingCarts
                            .Where(s => s.ApplicationUserId == shoppingCart.ApplicationUserId)
                            .ToList().Count;
            HttpContext.Session.SetInt32("ssCartCount", carts);

            return RedirectToAction(nameof(Index));
        }
    }
}
