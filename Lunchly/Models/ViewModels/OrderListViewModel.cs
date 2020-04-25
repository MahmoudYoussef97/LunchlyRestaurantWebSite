using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lunchly.Models.ViewModels
{
    public class OrderListViewModel
    {
        public IList<OrderHeadersAndDetailsViewModel> Orders { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}