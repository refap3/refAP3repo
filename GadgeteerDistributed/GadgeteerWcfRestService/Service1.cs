using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace GadgeteerWcfRestService
{

    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    // NOTE: If the service is renamed, remember to update the global.asax.cs file
    public class Service1
    {
        // accept 3 sensor readings: 
        // light 
        //humidity 
        //temperature 
        // and the captured image in the POST data
        //
        [WebInvoke(UriTemplate = "transfer/{light}/{hum}/{temp}", Method = "POST")]
        public void onPost(string light, string hum, string temp, Stream fileCont)
        {


            // save POST content as BMP file ...
            string docname = (DateTime.Now.ToLongTimeString() + "." + light + "_" + hum + "_" + temp + ".bmp").Replace(":", "_");
            string strdocPath;

            strdocPath = System.Web.Hosting.HostingEnvironment.MapPath("~/pics/") + docname;

            // Save Stream to local memory stream for later distrbution ...
            var ms = new MemoryStream();
            fileCont.CopyTo(ms);
            ms.Position = 0;


            FileStream fs = new FileStream(strdocPath, FileMode.Create, FileAccess.ReadWrite);
            ms.CopyTo(fs);
            ms.Position = 0;
            fs.Close(); ;

            Byte[] bytes = new Byte[ms.Length];
            ms.Read(bytes, 0, bytes.Length); 



            var objContext = new TDataEntities();
            var newData = new TDataItem();
            newData.lightSENS   = int.Parse(light);
            newData.humSENS = int.Parse(hum);
            newData.tempSENS = decimal.Parse(temp);
            newData.Date = DateTime.Now;

            newData.Transacto = bytes;

            objContext.TDataItems.AddObject(newData);
            objContext.SaveChanges(); 

            
        
        }




    }
}
