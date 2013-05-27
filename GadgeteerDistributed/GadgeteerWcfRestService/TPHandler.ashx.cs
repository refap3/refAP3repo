using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.IO;
using System.Diagnostics;

namespace GadgeteerWcfRestService
{
    /// <summary>
    /// Summary description for TPHandler
    /// </summary>
    public class TPHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {

            if (!String.IsNullOrEmpty(context.Request.QueryString["ID"]))

            {
                int id = int.Parse(context.Request.QueryString["ID"]);

                context.Response.ContentType = "image/bmp";

                var db = new TDataEntities();
                var myData = from td in db.TDataItems where td.ID == id select td;
                var pic = myData.First().Transacto;

                // debug code push predefined bit map over 
                //


                //var fileStrm = new FileStream("c:\\temp\\bb.bmp", FileMode.Open);
                //var memStrm = new MemoryStream();
                //fileStrm.CopyTo(memStrm) ;

                //pic = memStrm.GetBuffer();

                
                context.Response.BinaryWrite(pic); 
                


            }

        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}