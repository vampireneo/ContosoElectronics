using ContosoElectronics.Common;
using ContosoElectronics.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Streams;

namespace ContosoElectronics.DataSource
{
    public sealed class ProductDataSource
    {
        private static ProductDataSource _productDataSource = new ProductDataSource();

        private List<Product> _allProducts = null;

        public List<Product> AllProducts
        {
            get { return this._allProducts; }
        }

        /// <summary>
        /// This method is used to populate all the products from the file Product.xml. 
        /// It reads the xml file and serializes the data into a list of products.
        /// </summary>
        private async Task ReadXmlDataFromLocalStorageAsync()
        {
            try
            {
                //If product list already loaded from XML file, skip reloading it again. 
                if (_productDataSource.AllProducts != null)
                    return;

                var dataFolder = await Package.Current.InstalledLocation.GetFolderAsync("Data");
                StorageFile sessionFile = await dataFolder.GetFileAsync("Product.xml");

                using (IRandomAccessStreamWithContentType sessionInputStream = await sessionFile.OpenReadAsync())
                {
                    var sessionSerializer = new DataContractSerializer(typeof(Product[]));
                    var restoredData = (Product[])sessionSerializer.ReadObject(sessionInputStream.AsStreamForRead());
                    _allProducts = restoredData.ToList();
                }
            }
            catch (Exception ex)
            {
                _allProducts = null;
            }
        }
        
        /// <summary>
        /// Gets the product details.
        /// </summary>
        /// <param name="productId">The product id.</param>
        /// <returns></returns>
        public static async Task<Product> GetDetailsAsync(string productId)
        {
            await _productDataSource.ReadXmlDataFromLocalStorageAsync();
            var matches = _productDataSource.AllProducts.Where((item) => item.Id.Equals(productId));
            if (matches != null && matches.Count() == 1) return matches.First();
            return null;
        }

        /// <summary>
        /// Gets the products by sub category id.
        /// </summary>
        /// <param name="subCategoryId">The sub category id.</param>
        /// <returns></returns>
        public static async Task<List<Product>> GetProductsBySubCategoryIdAsync(string subCategoryId)
        {
            await _productDataSource.ReadXmlDataFromLocalStorageAsync();
            var matches = _productDataSource.AllProducts.Where((item) => item.SubCategory == subCategoryId).ToList();
            return matches;
        }

        /// <summary>
        /// Gets some random products for live tile to flip through.
        /// </summary>
        /// <param name="count">Number of random products</param>
        /// <returns>List of products</returns>
        public static async Task<List<Product>> GetRandomProductsAsync(int count)
        {
            await _productDataSource.ReadXmlDataFromLocalStorageAsync();
            var matches = _productDataSource.AllProducts.OrderBy(item => Guid.NewGuid()).Take(count).ToList();
            return matches;
        }

        /// <summary>
        /// This method searches the products by search text. The search text can be a part of the 
        /// Product name or Product description
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <returns>Search results</returns>
        public static async Task<List<Product>> SearchProductsAsync(string searchText)
        {
            await _productDataSource.ReadXmlDataFromLocalStorageAsync();
            return
                _productDataSource.AllProducts.Where(
                    item =>
                    item.Name.ToUpper().Contains(searchText.ToUpper()) ||
                    item.Description.ToUpper().Contains(searchText.ToUpper())).ToList();
        }
    }
}
