using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Mail;
using System.Net;
using System.Configuration;
using System.IO;
using System.IO.Ports;

namespace MailCheck
{

    public partial class Form1 : Form
    {
        bool doUpdate;
        public Form1()
        {
            InitializeComponent();
            doUpdate = false;
            button1.Text = "Start";
            Thread oThread = new Thread(new ThreadStart(doThings));
            oThread.Start();
        }

        private void notifyIcon1_DoubleClick(object sender, System.EventArgs e)
        {
            Show();
            
            WindowState = FormWindowState.Normal;
        }


        private static string CheckMail(string USER, string PASS)
        {
            string result = "";
            
            try
            {
                var encoded = TextToBase64(USER + ":" + PASS);
                var myWebRequest = HttpWebRequest.Create("https://gmail.google.com/gmail/feed/atom");
                myWebRequest.Method = "POST";
                myWebRequest.ContentLength = 0;
                myWebRequest.Headers.Add("Authorization", "Basic " + encoded);

                var response = myWebRequest.GetResponse();
                var stream = response.GetResponseStream();

                StreamReader reader = new StreamReader(stream);
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    result += line;
                    result += "\n";
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message);
            }
            return result;
        }

        public static string TextToBase64(string sAscii)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] bytes = encoding.GetBytes(sAscii);
            return System.Convert.ToBase64String(bytes, 0, bytes.Length);
        }

        public void doThings()
        {
            while (true)
            {
                if (doUpdate)
                {
                    string user = "nevo.himm@gmail.com";
                    string pass = "19981998n";
                    try
                    {
                        while (true)
                        {
                            if (doUpdate)
                            {
                                SerialPort port = new SerialPort("COM5", 9600, Parity.None, 8, StopBits.One);
                                port.Open();
                                string dt = "";
                                if (DateTime.Now.ToString("HH:") == "22:" || DateTime.Now.ToString("HH:") == "23:")
                                    dt = DateTime.Now.ToString("yyyy-MM-dd") + "T" + DateTime.Now.AddHours(-3).ToString("HH:mm");
                                if (DateTime.Now.ToString("HH:") == "00:")
                                    dt = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd") + "T" + DateTime.Now.AddHours(-3).ToString("HH:mm");
                                else
                                    dt = DateTime.Now.ToString("yyyy-MM-dd") + "T" + DateTime.Now.AddHours(-3).ToString("HH:mm");
                                string content = CheckMail(user, pass);
                                string sender = content;

                                sender = sender.Remove(content.IndexOf("</email>"), sender.Length - content.IndexOf("</email>"));
                                sender = sender.Remove(0, content.IndexOf("<email>") + 7);
                                if (content.IndexOf("<entry>") > 0)
                                    content = content.Remove(0, content.IndexOf("<entry>"));
                                else
                                    content = "";

                                if (content.IndexOf(dt) > 0 && sender == user)
                                {
                                    content = content.Remove(content.IndexOf("</title>"), content.Length - content.IndexOf("</title>"));
                                    if (content.IndexOf("Get ") > 0)
                                        content = content.Remove(0, content.IndexOf("Get "));
                                    else
                                    {
                                        if (content.IndexOf("Dir ") > 0)
                                            content = content.Remove(0, content.IndexOf("Dir "));

                                        else
                                            content = "Not found";
                                    }
                                    string inBox = "";
                                    System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage("nevo.himm@gmail.com", "nevo.himm@gmail.com");
                                    mail.Subject = "";
                                    string str = content.Remove(0, 4);

                                    if (content.IndexOf("Get") == 0 || content.IndexOf("Dir") == 0)
                                    {
                                        content = content.Remove(3, content.Length - 3);

                                        if (content == "Get")
                                        {
                                            if (File.Exists(str))
                                            {
                                                label1.Text += "\n" + ("Getting " + str + " Time: " + DateTime.Now.ToString("HH:mm:ss"));
                                                Attachment attachment = new Attachment(@str);
                                                mail.Attachments.Add(attachment);
                                                mail.Subject = str;
                                                inBox = "There you go!";
                                            }
                                            else
                                            {
                                                inBox = "Please try again";
                                                mail.Subject = "Could not find file";
                                            }
                                        }

                                        else
                                        {
                                            if (Directory.Exists(str))
                                            {
                                                label1.Text += "\n" + ("Directing " + str + " Time: " + DateTime.Now.ToString("HH:mm:ss"));
                                                mail.Subject = "the folders' content you asked for";
                                                inBox = "The files inside that folder are:" + "\n";
                                                string[] filePaths = Directory.GetFiles(@str);
                                                foreach (string name in filePaths)
                                                {
                                                    inBox += (name + "\n");
                                                }
                                            }
                                            else
                                            {
                                                mail.Subject = "Please try again";
                                                inBox = "Could not find path";
                                            }
                                        }
                                    }
                                    mail.Body = inBox;
                                    mail.IsBodyHtml = true;
                                    SmtpClient client = new SmtpClient("smtp.gmail.com");
                                    NetworkCredential cred = new NetworkCredential("nevo.himm@gmail.com", "19981998n");
                                    client.EnableSsl = true;
                                    client.Credentials = cred;
                                    if (inBox.Length > 0)
                                    {
                                        try
                                        {
                                            client.Send(mail);

                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e);
                                        }
                                    }
                                }
                                port.Close();
                                //System.Threading.Thread.Sleep(20000);
                            }
                        }
                    }
                    catch (Exception ee)
                    {
                        Console.WriteLine(ee.Message);
                    }
                }
            }
        }
    
            private void Form1_Resize(object sender, System.EventArgs e)
        {
            
            if (FormWindowState.Minimized == WindowState)
                Hide();
            }

        private void econtrol_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (doUpdate)
                button1.Text = "Start";
            else
                button1.Text = "Stop";
            doUpdate = !doUpdate;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
