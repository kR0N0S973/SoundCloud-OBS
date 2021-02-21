using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoundCloud_OBS
{
    public partial class Form1 : Form
    {
        string html = "";
        string webserver_path = Application.StartupPath + @"\Widget";
        CefSharp.WinForms.ChromiumWebBrowser chromiumWebBrowser1;
        string localWebAdress = @"http://127.0.0.1:12345/index.html";
        ChromiumWebBrowser widgetPreview;
        SimpleHTTPServer myServer;
        string song_author_class = "//a[@class='playbackSoundBadge__titleLink sc-truncate']";
        string song_name_class = "//a[@class='playbackSoundBadge__lightLink sc-link-light sc-truncate']";
        string song_image_class = "//span[@class='sc-artwork sc-artwork-placeholder-6  image__full g-opacity-transition']";

        public Form1()
        {
            this.BackColor = ColorTranslator.FromHtml("#ff5510");
            //ff5510

            Cef.Initialize(new CefSettings { RemoteDebuggingPort = 8088, CachePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\CEF", PersistSessionCookies = true });
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            Directory.CreateDirectory(webserver_path);
            ResourceManager rm = new ResourceManager("items", Assembly.GetExecutingAssembly());

            string str = Ressources.ResourceManager.GetString("index");
            if(str != string.Empty) 
            {
                //Application.StartupPath
                string indexfile = webserver_path + @"\index.html";
                if (!File.Exists(indexfile)) 
                {
                    StreamWriter wr = new StreamWriter(indexfile, false, System.Text.Encoding.UTF8);
                    wr.Write(str);
                    wr.Close();
                }
            }


            // start widget server
            myServer = new SimpleHTTPServer(webserver_path, 12345);
            linkLabel1.Text = localWebAdress;
            //linkLabel1.

            chromiumWebBrowser1 = new CefSharp.WinForms.ChromiumWebBrowser("http://soundcloud.com");
            widgetPreview = new CefSharp.WinForms.ChromiumWebBrowser(localWebAdress);
            panel2.Controls.Add(widgetPreview);
            panel1.Controls.Add(chromiumWebBrowser1);
            chromiumWebBrowser1.FrameLoadEnd += eoOnSiteLoaded;
            
        }

        private void eoOnSiteLoaded(object sender, EventArgs e)
        {
            timer1.Interval = 2000;
            timer1.Enabled = true;
        }


        private async Task<string> GetHtml() 
        {
            return await chromiumWebBrowser1.GetBrowser().MainFrame.GetSourceAsync();
        }


        private void timer1_Tick(object sender, EventArgs e)
        {

            if(!Directory.Exists(Application.StartupPath + @"\\Widget\\")) 
            {
                Directory.CreateDirectory(Application.StartupPath + @"\\Widget\\");
            }

            string f = webserver_path + @"\currentSource.txt";
            string f2 = webserver_path + @"\currentSong.txt";
            chromiumWebBrowser1.GetSourceAsync().ContinueWith(taskHtml =>
            {
                html = taskHtml.Result;
              
                if (File.Exists(f)) 
                {
                    File.Delete(f);
                }

                StreamWriter wr = new StreamWriter(f, false, System.Text.Encoding.UTF8);
                wr.Write(html);
                wr.Close();
            });


            try 
            {
            using (var sr = new StreamReader(f))
            {
                string test = sr.ReadToEnd();


                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(test);

                string text = "";

                


                HtmlAgilityPack.HtmlNode ts = doc.DocumentNode.SelectSingleNode(song_author_class);
                if (ts != null)
                {
                    text += ts.Attributes["title"].Value + ";";
                }

                //

                HtmlAgilityPack.HtmlNode ts2 = doc.DocumentNode.SelectSingleNode(song_name_class);
                if (ts2 != null)
                {
                    text += ts2.Attributes["title"].Value + ";";
                }


                    // playbackSoundBadge 

                    HtmlAgilityPack.HtmlNode ts4 = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'playbackSoundBadge')]");
                    if(ts4 != null)
                    {


                    foreach(HtmlAgilityPack.HtmlNode node in ts4.ChildNodes) 
                    {
                        try 
                        {
                                foreach (HtmlAgilityPack.HtmlNode node2 in node.ChildNodes)
                                {
                                    foreach (HtmlAgilityPack.HtmlNode node3 in node2.ChildNodes)
                                    {
                                        if (node3.HasClass("sc-artwork"))
                                        {
                                            string img = node3.Attributes["style"].Value;

                                            int pFrom = img.IndexOf("url(") + "key : ".Length;
                                            int pTo = img.LastIndexOf(")");

                                            string result = img.Substring(pFrom, pTo - pFrom).Replace('"', ' ').Replace("&quot;", "");

                                            text += result + "\n";
                                        }
                                    }
                                }
                        }catch(Exception ex) 
                        {
                        
                        }
                    }

                    }

                //    HtmlAgilityPack.HtmlNode ts3 = doc.DocumentNode.SelectSingleNode(song_image_class);
                //if (ts3 != null)
                //{
                //    string img = ts3.Attributes["style"].Value;

                //    int pFrom = img.IndexOf("url(") + "key : ".Length;
                //    int pTo = img.LastIndexOf(")");

                //    string result = img.Substring(pFrom, pTo - pFrom).Replace('"', ' ').Replace("&quot;", "");

                //    text += result + "\n";
                //}

                if (File.Exists(f2))
                {
                    File.Delete(f2);
                }

                StreamWriter wr = new StreamWriter(f2, false, System.Text.Encoding.UTF8);
                wr.Write(text);
                wr.Close();
                }
            }catch(Exception ex) 
            {
            
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //chromiumWebBrowser1.Load("http://soundcloud.com");

        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void chromiumWebBrowser2_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            myServer.Stop();
            myServer = new SimpleHTTPServer(webserver_path, 12345);

            widgetPreview.Reload(true);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabel1.Text);
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/kevinwiede/SoundCloud-OBS/");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(200);
        }
    }
}
