using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;

namespace XivelyConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {

            // before anything else set the locale to en_US ...

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", false);
            var feedId = 651218364;

            var xively = new XivelyClient(apiKey: @"vIWXkBR4a8fYMKW2FBxeZ1sTrMhE8p2vz3WbTuJ23vkFjtZc");

            xively.SetFeedLocation(feedId, lat: 41.65, lon: -83.57);

            var dcommand = DateTime.Now.Hour.ToString();
            var ddata = DateTime.Now.Minute.ToString();
            var dstatus = DateTime.Now.Second.ToString();
            var diplist = DateTime.Now.ToString();

            xively.WriteToFeed(feedId, values: new[] { 
                new XivelyDataPoint { StreamId = "command", CurrentValue = dcommand },
                new XivelyDataPoint { StreamId = "data", CurrentValue = ddata },
                new XivelyDataPoint { StreamId = "iplist", CurrentValue = diplist },
                new XivelyDataPoint { StreamId = "status", CurrentValue = dstatus }
            });

        }
    }
}
