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

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        static bool toggle = false; 
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MakeRequests();
        }

        private void MakeRequests()
        {
            HttpWebResponse response;

            if (Request_try_yaler_net(out response))
            {
                response.Close();
            }
        }

        private bool Request_try_yaler_net(out HttpWebResponse response)
        {
            response = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://try.yaler.net/gsiot-bcjp-yj88/a1");

                request.KeepAlive = true;
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.31 (KHTML, like Gecko) Chrome/26.0.1410.64 Safari/537.31";
                request.Referer = "http://try.yaler.net/gsiot-bcjp-yj88/hello";
                request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip,deflate,sdch");
                request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,de-DE;q=0.8,de;q=0.6,en;q=0.4");
                request.Headers.Set(HttpRequestHeader.AcceptCharset, "ISO-8859-1,utf-8;q=0.7,*;q=0.3");
                request.Headers.Add("DNT", @"1");

                response = (HttpWebResponse)request.GetResponse();
                var buffer = new byte[response.ContentLength];

                response.GetResponseStream().Read(buffer, 0, buffer.Length);
                string content = Encoding.UTF8.GetString(buffer);
                textBox1.Text = content;
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError) response = (HttpWebResponse)e.Response;
                else return false;
            }
            catch (Exception)
            {
                if (response != null) response.Close();
                return false;
            }

            return true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MakeRequests2();
        }



        private void MakeRequests2()
        {
            HttpWebResponse response;

            if (Request_try_yaler_net2(out response))
            {
                response.Close();
            }
        }

        private bool Request_try_yaler_net2(out HttpWebResponse response)
        {
            
            response = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://try.yaler.net/gsiot-bcjp-yj88/d13");

                request.Method = "PUT";


                string body = toggle.ToString().ToLower();
                byte[] postBytes = System.Text.Encoding.UTF8.GetBytes(body);
                request.ContentLength = postBytes.Length;
                Stream stream = request.GetRequestStream();
                stream.Write(postBytes, 0, postBytes.Length);
                stream.Close();

                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError) response = (HttpWebResponse)e.Response;
                else return false;
            }
            catch (Exception)
            {
                if (response != null) response.Close();
                return false;
            }

            return true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            toggle = !toggle;
            if (toggle) MakeRequests3("on");
            else MakeRequests3("off");
        }
        private void MakeRequests3(string onoff)
        {
            HttpWebResponse response;

            if (Request_try_yaler_net3(out response,onoff ))
            {
                response.Close();
            }
        }

        private bool Request_try_yaler_net3(out HttpWebResponse response, string onoff)
        {
            response = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://try.yaler.net/gsiot-bcjp-yj88/" + onoff);

                request.Method = "POST";
                request.ServicePoint.Expect100Continue = false;

                string body = @"";
                byte[] postBytes = System.Text.Encoding.UTF8.GetBytes(body);
                request.ContentLength = postBytes.Length;
                Stream stream = request.GetRequestStream();
                stream.Write(postBytes, 0, postBytes.Length);
                stream.Close();

                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError) response = (HttpWebResponse)e.Response;
                else return false;
            }
            catch (Exception)
            {
                if (response != null) response.Close();
                return false;
            }

            return true;
        }

    }
}
