using ContosoElectronics.Common;
using ContosoElectronics.DataModel;
using ContosoElectronics.DataSource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ContosoElectronics
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class ProductDetailsPage : Page
    {
        private Product product;
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ProductDetailsPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Assign a collection of bindable groups to this.DefaultViewModel["Groups"]
            product = await ProductDataSource.GetDetailsAsync(e.NavigationParameter.ToString());
            this.defaultViewModel["ProductDetails"] = product;
            this.defaultViewModel["ProductName"] = product.Name;
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="ContosoElectronics.Common.NavigationHelper.LoadState"/>
        /// and <see cref="ContosoElectronics.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Call the corresponding method on navigation helper
            navigationHelper.OnNavigatedTo(e);

            //Get instance of DataTransfer Manager and add handler for DataRequested event
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataRequested;            
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //Call the corresponding method on navigation helper
            navigationHelper.OnNavigatedFrom(e);

            //Get instance of DataTransfer Manager and remove handler for DataRequested event
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested -= DataRequested;
        }

        #endregion

        #region Feature - Share Charm
        /// <summary>
        /// This method contains the logic to prepare the data which would be shared.
        /// The Product Details would be prepared in the form of a table which can then be shared through email
        /// </summary>
        /// <param name="request"></param>
        private void DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            if (request != null)
            {
                request.Data.Properties.Title = "Found an interesting product...";
                request.Data.Properties.Description = product.Description;

                //Create table format using the Product details
                StringBuilder htmlcontent = new StringBuilder("<p> <Table style=\"font-family:Segoe UI;\"");
                htmlcontent.Append("<tr> <td> <b> Product Name: </b>" + product.Name + "  </td> </tr> ");
                htmlcontent.Append("<tr> <td> <b> Product Number: </b>" + product.Id + " </td> </tr> ");
                htmlcontent.Append("<tr> <td> <b> Product Price: </b>$" + PriceConverter.Convert(product.Price) + " </td> </tr> ");
                htmlcontent.Append("<tr> <td> </td> </tr> <tr> <td> <b> Description: </b> </td> </tr> ");
                htmlcontent.Append("<tr> <td> " + product.Description + " </td></tr> ");
                if (product.ProductSpecifications != null)
                {
                    htmlcontent.Append("<tr> <td> </td> </tr> <tr> <td> <b> Specifications: </b> </td> </tr> ");
                    foreach (var spec in product.ProductSpecifications)
                    {
                        htmlcontent.Append("<tr> <td> " + spec.Name + ": " + spec.Value + " </td></tr> ");
                    }
                }
                htmlcontent.Append("</table> </p>");
                request.Data.SetHtmlFormat(HtmlFormatHelper.CreateHtmlFormat(htmlcontent.ToString()));
            }

        }

        #endregion

        #region Feature - App Bar
        /// <summary>
        /// Handles the App Bar event - Pin to Start. This event is used to pin the current Product as a secondary tile 
        /// on the Start screen
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void PinToStart_Click(object sender, RoutedEventArgs e)
        {
            await SecondaryTileCreation(sender);
        }

        /// <summary>
        /// Handles the App Bar event - Home. This event is used to take the user back to Grouped category screen. 
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Home_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to job listing page
            this.Frame.Navigate(typeof(CategoryListingPage));
        }

        #endregion

        #region Feature - Secondary Tile

        /// <summary>
        /// This method checks if a secondary tile is already present for this job. If it is already present, 
        /// the product is unpinned, if not the product is pinned.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        private async Task SecondaryTileCreation(object sender)
        {
            string appbarTileId = product.Id;
            if (!SecondaryTile.Exists(appbarTileId))
            {
                await PinSecondaryTile(sender, appbarTileId);
            }
            else
            {
                await UnpinSecondaryTile(sender, appbarTileId);
            }
        }

        /// <summary>
        /// This method pins the secondary tile. The secondary tile is created using the required parameters
        /// and pinned. The user is shown a message informing whether the tile is pinned successfully
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="appbarTileId">The appbar tile id.</param>
        /// <returns></returns>
        private async Task PinSecondaryTile(object sender, string appbarTileId)
        {
            // Prepare package images for use as the Tile Logo in our tile to be pinned
            Uri smallLogo = new Uri("ms-appx://" + product.ImagePath);
            Uri wideLogo = new Uri("ms-appx://" + product.ImagePath);
            //Uri wideLogo = new Uri("ms-appx:///Assets/Tile_310X150.png");

            string tileActivationArguments = appbarTileId;
            // Create a 1x1 Secondary tile
            string subTitle = product.Name;
            SecondaryTile secondaryTile = new SecondaryTile(appbarTileId, product.Name, subTitle, tileActivationArguments,
                TileOptions.ShowNameOnLogo | TileOptions.ShowNameOnWideLogo, smallLogo, wideLogo);

            secondaryTile.ForegroundText = ForegroundText.Dark;
            bool isPinned = await secondaryTile.RequestCreateForSelectionAsync(GetElementRect((FrameworkElement)sender), Windows.UI.Popups.Placement.Above);

            if (isPinned)
            {
                MessageDialog dialog = new MessageDialog("Product " + product.Name + " successfully pinned.");
                await dialog.ShowAsync();
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Product " + product.Name + " not pinned.");
                await dialog.ShowAsync();
            }

            ToggleAppBarButton(!isPinned, sender as AppBarButton);
        }

        /// <summary>
        /// This method unpins the existing secondary tile. 
        /// The user is shown a message informing whether the tile is unpinned successfully
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="appbarTileId">The appbar tile id.</param>
        /// <returns></returns>
        private async Task UnpinSecondaryTile(object sender, string appbarTileId)
        {
            SecondaryTile secondaryTile = new SecondaryTile(appbarTileId);
            bool isUnpinned = await secondaryTile.RequestDeleteForSelectionAsync(GetElementRect((FrameworkElement)sender),
                Placement.Above);

            if (isUnpinned)
            {
                MessageDialog dialog = new MessageDialog("Product " + product.Name + " successfully unpinned.");
                await dialog.ShowAsync();
            }
            else
            {
                MessageDialog dialog = new MessageDialog("Product " + product.Name + " not unpinned.");
                await dialog.ShowAsync();
            }

            ToggleAppBarButton(isUnpinned, sender as AppBarButton);
        }


        /// <summary>
        /// This method assigns the style to the app bar button.
        /// </summary>
        /// <param name="showPinButton">if set to <c>true</c> [show pin button].</param>
        private void ToggleAppBarButton(bool showPinButton, AppBarButton pinToStart)
        {
            if (pinToStart != null)
            {
                pinToStart.Icon = (showPinButton) ? new SymbolIcon(Symbol.Pin) :
                    new SymbolIcon(Symbol.UnPin);
                pinToStart.Label = (showPinButton) ? "Pin To Start" : "Unpin";
            }
        }

        /// <summary>
        /// This method is called when the PinToStart button has completed loading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PinToStart_Loaded(object sender, RoutedEventArgs e)
        {
            ToggleAppBarButton(!SecondaryTile.Exists(product.Id), sender as AppBarButton);
        }
        
        /// <summary>
        /// This method creates the placeholder for the secondary tile and shows it above the 'Pin' button.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public static Rect GetElementRect(FrameworkElement element)
        {
            GeneralTransform buttonTransform = element.TransformToVisual(null);
            Point point = buttonTransform.TransformPoint(new Point());
            return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
        }

        #endregion
        
    }
}
