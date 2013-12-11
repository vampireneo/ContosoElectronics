using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ContosoElectronics.Common;
using ContosoElectronics.DataModel;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Streams;

namespace ContosoElectronics.DataSource
{
    class CategoryDataSource
    {
        private static CategoryDataSource _categoryDataSource = new CategoryDataSource();

        private List<ProductCategory> _allCategories = null;


        public List<ProductCategory> AllCategories
        {
            get { return this._allCategories; }
        }


        /// <summary>
        /// This method is used to create dummy category list.
        /// </summary>
        private async Task PrepareDummyData()
        {
            try
            {
                //If job list already loaded, skip generating it again. 
                if (_categoryDataSource.AllCategories != null)
                    return;

                _allCategories = new List<ProductCategory>();
                for (int i = 0; i < 3; i++)
                {
                    ProductCategory productCategory = new ProductCategory()
                    {
                        Id = i.ToString(),
                        Name = "Category " + i.ToString(),
                    };

                    List<ProductSubCategory> productSubCategory = new List<ProductSubCategory>();
                    for (int j = 1; j < 4; j++)
                    {
                        productSubCategory.Add(new ProductSubCategory()
                        {
                            Id = "1" + j.ToString(),
                            ImagePath = "/Data/ProductCategoryImages/placeholder.jpg",
                            Name = "Sub Category " + i.ToString() + j.ToString(),
                            ProductCount = j
                        });
                    }
                    productCategory.SubCategoryItems = productSubCategory;

                    _allCategories.Add(productCategory);

                }
            }
            catch (Exception ex)
            {
                _allCategories = null;
            }
        }


        /// <summary>
        /// Gets the category list.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<ProductCategory>> GetListAsync_Dummy()
        {
            await _categoryDataSource.PrepareDummyData();
            return _categoryDataSource.AllCategories;
        }

        /// <summary>
        /// Gets the Sub category Name.
        /// </summary>
        /// <param name="subCategoryId">The sub category id.</param>
        /// <returns></returns>
        public static async Task<string> GetSubCategoryNameAsync_Dummy(string subCategoryId)
        {
            await _categoryDataSource.PrepareDummyData();
            return
                _categoryDataSource.AllCategories.SelectMany(x => x.SubCategoryItems).FirstOrDefault(x => x.Id == subCategoryId).Name;
        }

        /// <summary>
        /// Gets the category details.
        /// </summary>
        /// <param name="subCategoryId">The category name.</param>
        /// <returns></returns>
        public static async Task<ProductCategory> GetCategoryDetailsAsync_Dummy(string categoryName)
        {
            await _categoryDataSource.PrepareDummyData();
            return _categoryDataSource.AllCategories.Where(x => x.Name.ToUpper().StartsWith(categoryName.ToUpper())).FirstOrDefault();
        }

    }
}
