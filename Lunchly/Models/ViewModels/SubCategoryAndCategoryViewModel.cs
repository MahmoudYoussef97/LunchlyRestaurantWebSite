using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lunchly.Models.ViewModels
{
    public class SubCategoryAndCategoryViewModel
    {
        public IEnumerable<Category> Categories { get; set;}
        public SubCategory SubCategory { get; set; }
        public IList<string> SubCategoriesList { get; set; }
        public string StatusMessage { get; set; }
    }
}
