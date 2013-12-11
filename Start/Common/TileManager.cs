using ContosoElectronics.DataSource;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace ContosoElectronics.Common
{
    /// <summary>
    /// Encapsulates common functionality to support live tiles in the application
    /// </summary>
    public class TileManager
    {
        readonly List<Tile> _lstTiles = new List<Tile>();
        
        /// <summary>
        /// Create application live tiles based on template included in xml file
        /// </summary>
        /// <returns></returns>
        public async Task CreateLiveTilesAsync()
        {
            GetTilesAsync();
            foreach (Tile tile in _lstTiles)
            {
                XmlDocument tileXml = CreateSquareTile(tile);
                XmlDocument wideTileXml = CreateWideTile(tile);
                XmlDocument largeTileXml = CreateLargeTile(tile);

                // Add the wide tile to the square tile's payload, so they are sibling elements under visual 
                IXmlNode node = tileXml.ImportNode(wideTileXml.GetElementsByTagName("binding").Item(0), true);
                tileXml.GetElementsByTagName("visual").Item(0).AppendChild(node);

                IXmlNode node1 = tileXml.ImportNode(largeTileXml.GetElementsByTagName("binding").Item(0), true);
                tileXml.GetElementsByTagName("visual").Item(0).AppendChild(node1);

                // Create a tile notification that will expire in 1 day and send the live tile update.  
                TileNotification tileNotification = new TileNotification(tileXml);
                TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);

            }
        }

        /// <summary>
        /// Load live tiles configuration data provider. Random products are used to populate live tile. 
        /// </summary>
        /// <returns></returns>
        private async void GetTilesAsync()
        {
            var randomProducts = await ProductDataSource.GetRandomProductsAsync(3);

            foreach (var product in randomProducts)
            {
                var tile = new Tile
                {
                    Text1 = product.Name,
                    Text2 = product.Price.ToString("C"),
                    ImageUrl = product.ImagePath
                };

                _lstTiles.Add(tile);
            }
        }

        private XmlDocument CreateSquareTile (Tile tile)
        {
            // Create a live update for a square tile
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150PeekImageAndText02);

            XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = tile.Text2;
            tileTextAttributes[1].InnerText = tile.Text1;   

            XmlNodeList tileImageAttributes = tileXml.GetElementsByTagName("image");
            ((XmlElement)tileImageAttributes[0]).SetAttribute("src", tile.ImageUrl);
            ((XmlElement)tileImageAttributes[0]).SetAttribute("alt", "Contoso Electronics logo");

            var sqTileBinding = (XmlElement)tileXml.GetElementsByTagName("binding").Item(0);
            sqTileBinding.SetAttribute("branding", "none"); // removes logo from lower left-hand corner of tile

            return tileXml;
        }

        private XmlDocument CreateWideTile (Tile tile)
        {
            // Create a live update for a wide tile
            XmlDocument wideTileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150SmallImageAndText04);

            // Assign text
            XmlNodeList wideTileTextAttributes = wideTileXml.GetElementsByTagName("text");
            wideTileTextAttributes[0].AppendChild(wideTileXml.CreateTextNode(tile.Text2));
            wideTileTextAttributes[1].AppendChild(wideTileXml.CreateTextNode(tile.Text1));

            // Assign Image
            XmlNodeList wideTileImageAttributes = wideTileXml.GetElementsByTagName("image");
            ((XmlElement)wideTileImageAttributes[0]).SetAttribute("src", tile.ImageUrl);
            ((XmlElement)wideTileImageAttributes[0]).SetAttribute("alt", "Contoso Electronics logo");
                     
            return wideTileXml;
        }

        private XmlDocument CreateLargeTile(Tile tile)
        {
            // Create a live update for a wide tile
            XmlDocument largeTileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare310x310ImageAndText01);

            XmlNodeList wideTileTextAttributes = largeTileXml.GetElementsByTagName("text");
            wideTileTextAttributes[0].AppendChild(largeTileXml.CreateTextNode(tile.Text1 + " "+tile.Text2));
            //wideTileTextAttributes[1].AppendChild(largeTileXml.CreateTextNode(tile.Text2));

            XmlNodeList wideTileImageAttributes = largeTileXml.GetElementsByTagName("image");
            ((XmlElement)wideTileImageAttributes[0]).SetAttribute("src", tile.ImageUrl);
            ((XmlElement)wideTileImageAttributes[0]).SetAttribute("alt", "Contoso Electronics logo");

            var wideTileBinding = (XmlElement)largeTileXml.GetElementsByTagName("binding").Item(0);
            wideTileBinding.SetAttribute("branding", "none"); // removes logo from lower left-hand corner of tile

            return largeTileXml;
        }        
    }

    /// <summary>
    /// Strongly typed representation of tile configuration data
    /// </summary>
    public class Tile
    {
        public string ImageUrl { get; set; }
        public string Alt { get; set; }
        public string Text1 { get; set; }
        public string Text2 { get; set; }
        public string Text3 { get; set; }
        public string TileType { get; set; }
        public string CopiedImageContent { get; set; }
    }


}
