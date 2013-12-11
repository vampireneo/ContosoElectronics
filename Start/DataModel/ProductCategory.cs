using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContosoElectronics.DataModel
{
    public class ProductCategory
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string ImagePath { get; set; }

        public List<ProductSubCategory> SubCategoryItems { get; set; }
    }
}
