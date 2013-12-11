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
        /// Populates the categories from store file.
        /// </summary>
        private async Task ReadXmlDataFromLocalStorageAsync()
        {
            try
            {
                //If category list already loaded from XML file, skip reloading it again. 
                if (_categoryDataSource.AllCategories != null)
                    return;

                var dataFolder = await Package.Current.InstalledLocation.GetFolderAsync(Constants.DataFilesFolder);
                StorageFile sessionFile = await dataFolder.GetFileAsync(Constants.CategoryFile);

                using (IRandomAccessStreamWithContentType sessionInputStream = await sessionFile.OpenReadAsync())
                {
                    var sessionSerializer = new DataContractSerializer(typeof(ProductCategory[]));
                    var restoredData = (ProductCategory[])sessionSerializer.ReadObject(sessionInputStream.AsStreamForRead());
                    _allCategories = restoredData.ToList();
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
        public static async Task<List<ProductCategory>> GetListAsync()
        {
            await _categoryDataSource.ReadXmlDataFromLocalStorageAsync();
            return _categoryDataSource.AllCategories;
        }

        /// <summary>
        /// Gets the Sub category Name.
        /// </summary>
        /// <param name="subCategoryId">The sub category id.</param>
        /// <returns></returns>
        public static async Task<string> GetSubCategoryNameAsync(string subCategoryId)
        {
            await _categoryDataSource.ReadXmlDataFromLocalStorageAsync();
            return
                _categoryDataSource.AllCategories.SelectMany(x => x.SubCategoryItems).FirstOrDefault(x => x.Id == subCategoryId).Name;
        }

        /// <summary>
        /// Gets the category details.
        /// </summary>
        /// <param name="subCategoryId">The category name.</param>
        /// <returns></returns>
        public static async Task<ProductCategory> GetCategoryDetailsAsync(string categoryName)
        {
            await _categoryDataSource.ReadXmlDataFromLocalStorageAsync();
            return _categoryDataSource.AllCategories.Where(x => x.Name.ToUpper().StartsWith(categoryName.ToUpper())).FirstOrDefault();
        }
    }
}
