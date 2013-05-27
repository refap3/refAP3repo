using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GadgeteerWcfRestService
{
    public partial class DumpSQLdb : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            var db = new TDataEntities();
            foreach (var item in db.TDataItems)
            {
                db.DeleteObject(item); 
            }
            db.SaveChanges();
            Response.Redirect(Request.RawUrl);
        }
    }
}