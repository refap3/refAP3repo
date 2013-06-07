using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;



// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace XivelyClient
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : XivelyClient.Common.LayoutAwarePage
    {
        TileNotification notification;
        TileUpdater updater;
        XmlNodeList tileLines;
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //Tile test ...
            var tileContent = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquareBlock);
             tileLines = tileContent.SelectNodes("tile/visual/binding/text");
            tileLines[0].InnerText = inputBox.Text.Substring(0,2);


            notification = new TileNotification(tileContent);

            updater = TileUpdateManager.CreateTileUpdaterForApplication();
            //updater.EnableNotificationQueue(true);


            MakeRequests(inputBox.Text);
            xivelyOutput.Text = "Requested " + inputBox.Text + " from Xively at " + "" + DateTime.Now.ToString("hh:mm:ss");
        }


        private async void MakeRequests(string meas)
        {


            try
            {
                WebRequest request = WebRequest.Create("http://api.xively.com/v2/feeds/1934589243/datastreams/" + meas + ".csv");
                request.Headers["X-ApiKey"] = @"820f-wFPt2yWqCxSwk4t3gvP4F2SAKw4V2s3TEsycGJhVT0g";
                var response = await request.GetResponseAsync();

                //dump results ...
                char[] responseAsChar = new char[2000];
                int count = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")).Read(responseAsChar, 0, responseAsChar.Length);
                xivelyOutput.Text = new string(responseAsChar, 0, count).Split(',')[1]; // hack out the value !


                tileLines[1].InnerText = xivelyOutput.Text;
                updater.Update(notification);


            }

            catch (Exception xxx)
            {
                xivelyOutput.Text = "BAD! " + xxx.ToString();

            }

        }

    }
}

