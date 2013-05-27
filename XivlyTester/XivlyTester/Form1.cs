using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;

namespace XivlyTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            txtStatus.Text = "";
            string result = txtBody.Text; 
            byte[] bytes = Encoding.UTF8.GetBytes(result);


            var request = (HttpWebRequest)WebRequest.Create(txtTransport.Text  + txtFeedId.Text + ".csv");
            request.Method = "PUT";
            request.ContentType = "text/csv";
            request.ContentLength = bytes.Length;
            request.Headers.Add("X-ApiKey", txtApiKey.Text);

            //add content ...
            var stream = request.GetRequestStream();
            stream.Write(bytes, 0, bytes.Length);

            //send 

            using (var response = (HttpWebResponse)request.GetResponse())
            {

                Debug.Print("Status: " + response.StatusCode);
                txtStatus.Text = response.StatusCode + " " + response.StatusDescription; 
            }






        }

        private void button2_Click(object sender, EventArgs e)
        {
            txtStatus.Text = "";
            txtGetBody.Text = "";

            var apiKey=txtApiKey.Text;
            var apiEndpoint = txtTransport.Text + txtFeedId.Text;
            string s=""; 
            var csv = getXivlelyCSV(apiKey, apiEndpoint,ref s);
            txtGetBody.Text = csv;
            txtStatus.Text = s;


            // unhack the data CSV 
            //

            double ligthVal = double.Parse(csv.Split(',')[4].Split('\n')[0].Split('.')[0]);
            Debug.Print(ligthVal.ToString());





        }

        private string  getXivlelyCSV(string apiKey, string apiEndpoint, ref string statusCode)
        {
            var request = (HttpWebRequest)WebRequest.Create(apiEndpoint + ".csv");

            request.Method = "GET";
            request.Headers.Add("X-ApiKey", apiKey);
            //send 

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                Debug.Print("Status: " + response.StatusCode);
                 statusCode = response.StatusCode.ToString();
                var stream = response.GetResponseStream();
                byte[] bytes = new byte[response.ContentLength];
                int toread = bytes.Length;
                while (toread > 0)
                {
                    int read = stream.Read(bytes, bytes.Length - toread, toread);
                    toread = toread - read;
                }
                char[] chars = Encoding.UTF8.GetChars(bytes);
                var csvData = new string(chars);
                return csvData; 
            }
        }
    }
}
