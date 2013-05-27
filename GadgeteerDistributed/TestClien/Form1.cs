using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace TestClien
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            UploadThisFile("c:\\temp\\aa.bmp");


        }

        private void UploadThisFile(string fileToLoad)
        {
            string reqHOST = "192.168.2.157";
            reqHOST = "spider.hv.internal"; //debug 
            reqHOST="localhost";
            string reaURI = "http://" + reqHOST + ":1046/Service1/transfer/1/2/3";
            //reaURI = "http://" + reqHOST + ":80/gadg/Service1/transfer/1/2/3";

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(reaURI);
            textBox2.Text = reaURI; 


            loadFileIntoPOSTdata(req, fileToLoad);
            textBox1.Text = "";
            try
            {
                req.GetResponse();

            }
            catch (Exception xxx)
            {
                this.Text = "BADE: " + DateTime.Now.ToLongTimeString();
                textBox1.Text = xxx.ToString();

            }
        }

        private static void loadFileIntoPOSTdata(HttpWebRequest req, string fileToLoad)
        {
            req.Method = "POST";
            req.ContentType = "image/bmp";

            var filStrm = new FileStream(fileToLoad, FileMode.Open);
            var reqstr = req.GetRequestStream();
            var memStrm = new MemoryStream();
            filStrm.CopyTo(memStrm);
            filStrm.Close();

            byte[] fileToSend = memStrm.GetBuffer();
            reqstr.Write(fileToSend, 0, fileToSend.Length);
            reqstr.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            UploadThisFile("c:\\temp\\bb.bmp");
        }
    }
}
