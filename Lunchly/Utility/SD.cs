using Lunchly.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lunchly.Utility
{
    public class SD
    {
        public const string DefaultImage = "default_food.png";

        public const string ManagerUser = "Manager";
        public const string KichenUser = "Kitchen";
        public const string FrontDeskUser = "FrontDesk";
        public const string CustomerEndUser = "Customer";
		public const string ssCouponCode = "ssCouponCode";

		public const string StatusSubmitted = "Sumbitted";
		public const string StatusInProcess = "Being Prepared";
		public const string StatusReady = "Ready For Pickup";
		public const string StatusCompleted = "Completed";
		public const string StatusCancelled = "Cancelled";
		
		public const string PaymentStatusPending = "Pending";
		public const string PaymentStatusApproved = "Approved";
		public const string PaymentStatusRejected = "Rejected";
		public static string ConvertToRawHtml(string source)
		{
			char[] array = new char[source.Length];
			int arrayIndex = 0;
			bool inside = false;

			for (int i = 0; i < source.Length; i++)
			{
				char let = source[i];
				if (let == '<')
				{
					inside = true;
					continue;
				}
				if (let == '>')
				{
					inside = false;
					continue;
				}
				if (!inside)
				{
					array[arrayIndex] = let;
					arrayIndex++;
				}
			}
			return new string(array, 0, arrayIndex);
		}

		public static double DiscountedPrice(Coupon coupon, double originalTotalPrice)
		{
			if (coupon == null)
				return originalTotalPrice;

			if (coupon.MinimumAmount > originalTotalPrice)
				return originalTotalPrice;

			if (Convert.ToInt32(coupon.CouponType) == (int)Coupon.ECouponType.EGP)
				return Math.Round(originalTotalPrice - coupon.Discount, 2);
			
			if (Convert.ToInt32(coupon.CouponType) == (int)Coupon.ECouponType.Percent)
				return Math.Round(originalTotalPrice - (originalTotalPrice * coupon.Discount / 100), 2);

			return originalTotalPrice;
		}

	}
}
