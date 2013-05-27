using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace WcfRestService3
{
    public partial class DisplayImgs : System.Web.UI.Page
    {
        string upFolder;
        DirectoryInfo dir;
        protected void Page_Load(object sender, EventArgs e)
        {
            upFolder = MapPath("~/pics/");
            dir =  new DirectoryInfo(upFolder);
            DataList1.DataSource = dir.GetFiles("*.bmp");
            DataList1.DataBind(); 

        }

        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            foreach (var item in dir.GetFiles("*.bmp"))
            {
                item.Delete();
            }
            LinkButton2_Click(sender, e);
        }

        protected void LinkButton2_Click(object sender, EventArgs e)
        {

            Response.Redirect(Request.RawUrl);
        }
    }
}