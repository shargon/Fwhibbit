using ChartResume.Charts;
using ChartResume.Helpers;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace ChartResume
{
    class Program
    {
        const string ConfigFile = "config.json";
        const string DoneFile = "sended.json";

        static void Main(string[] args)
        {
            Config cfg = null;
            try { cfg = Config.LoadFromFile(ConfigFile); }
            catch
            {
                if (cfg == null) cfg = new Config();
                cfg.SaveToFile(ConfigFile);

                Console.WriteLine("Require config");
                return;
            }

            SmtpClient smtp = new SmtpClient(cfg.SmtpServer, cfg.SmtpPort)
            {
                EnableSsl = cfg.SmtpEnableSsl,
                Credentials = new NetworkCredential(cfg.SmtpUser, cfg.SmtpPassword)
            };

            List<int> sended = new List<int>(), errorSend = new List<int>();

            if (File.Exists(DoneFile))
            {
                sended = JsonConvert.DeserializeObject<List<int>>(File.ReadAllText("sended.json"));
                if (sended == null) sended = new List<int>();
            }

            // Connect
            using (MySqlConnection con = new MySqlConnection(new MySqlConnectionStringBuilder()
            {
                Password = cfg.DatabasePassword,
                Database = cfg.Database,
                UserID = cfg.DatabaseUserID,
                Server = cfg.DatabaseServer,
                Port = cfg.DatabasePort
            }
            .ToString()))
            {
                con.Open();

                using (MySqlCommand com = con.CreateCommand())
                {
                    com.Parameters.Clear();

                    // Fetch users & scores
                    using (DataTable dt = SqlHelper.SelectDataTable(com, "SELECT user_id,name,email,user_points from vteams order by user_points DESC"))
                    using (DataTable dtscores = SqlHelper.SelectDataTable(com, "SELECT user_id, ts, points from vscores"))
                    {
                        int maxScore;
                        Score[] globalScore = Chart3.Preload(dtscores, out maxScore);
                        Bitmap bmp_banner = Res.report;

                        foreach (DataRow dr in dt.Rows)
                        {
                            string email = dr["email"].ToString();
                            if (string.IsNullOrEmpty(email)) continue;

                            int userId = Convert.ToInt32(dr["user_id"]);
                            if (sended.Contains(userId)) continue;

                            // Get chart 1

                            string url1 = Chart1.GetChart(userId, com);
                            if (string.IsNullOrEmpty(url1)) continue;

                            // Get chart 2

                            string url2 = Chart2.GetChart(userId, com);
                            if (string.IsNullOrEmpty(url2)) continue;

                            string url3 = Chart3.GetChart(userId, dr["name"].ToString(), com, dtscores, globalScore, maxScore);
                            if (string.IsNullOrEmpty(url3)) continue;

                            // Generate image
                            using (WebClient client = new WebClient())
                            {
                                byte[] d1 = client.DownloadData(url1);
                                byte[] d2 = client.DownloadData(url2);
                                byte[] d3 = client.DownloadData(url3);

                                if (d1 == null || d2 == null || d3 == null)
                                {
                                    errorSend.Add(userId);
                                    continue;
                                }

                                using (MemoryStream ms1 = new MemoryStream(d1))
                                using (MemoryStream ms2 = new MemoryStream(d2))
                                using (MemoryStream ms3 = new MemoryStream(d3))
                                using (Image bmp1 = Image.FromStream(ms1))
                                using (Image bmp2 = Image.FromStream(ms2))
                                using (Image bmp3 = Image.FromStream(ms3))
                                {
                                    Bitmap bmpnew = new Bitmap(bmp_banner.Width, bmp_banner.Height);

                                    using (Graphics g = Graphics.FromImage(bmpnew))
                                    {
                                        g.DrawImageUnscaled(bmp_banner, 0, 0);

                                        using (Font f1 = new Font("Arial", 19, FontStyle.Bold))
                                        using (Font f2 = new Font("Arial", 21, FontStyle.Bold))
                                        using (Brush br = new SolidBrush(Color.White))
                                        //using (Brush br = new SolidBrush(Color.FromArgb(174, 34, 34)))
                                        using (StringFormat sf = new StringFormat()
                                        {
                                            Alignment = StringAlignment.Center,
                                            LineAlignment = StringAlignment.Center
                                        })
                                        {
                                            int puesto = dt.Rows.IndexOf(dr) + 1;

                                            g.DrawString(dr["name"].ToString(), f1, br, new Rectangle(0, 265, 350, 80), sf);
                                            g.DrawString(dr["user_points"].ToString(), f2, br, new Rectangle(550, 278, 170, 80), sf);

                                            //g.FillRectangle(br, new Rectangle(720, 265, 200, 80));
                                            g.DrawString((puesto == 1 ? "WINNER!" : puesto.ToString() + "º"), f2, br, new Rectangle(710, 278, 195, 80), sf);
                                        }

                                        g.DrawImageUnscaled(bmp1, 64, 447);
                                        g.DrawImageUnscaled(bmp2, 64, 700);
                                        g.DrawImageUnscaled(bmp3, 64, 1023);
                                    }

                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        bmpnew.Save(ms, ImageFormat.Png);

                                        //File.WriteAllBytes("D:\\report-test.png", ms.ToArray());

                                        try
                                        {
                                            MailMessage msg = new MailMessage()
                                            {
                                                From = new MailAddress(cfg.SmtpUser, "No reply"),
                                                Body = cfg.MailBody,
                                                Subject = cfg.MailSubject,
                                                IsBodyHtml = false,
                                            };

                                            msg.To.Add(email);
                                            //msg.Bcc.Add("xxxxx@xxxxx.com");

                                            ms.Seek(0, SeekOrigin.Begin);
                                            msg.Attachments.Add(new Attachment(ms, "report.png", "image/png"));

                                            smtp.Send(msg);
                                            sended.Add(userId);
                                        }
                                        catch (Exception e)
                                        {
                                            errorSend.Add(userId);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            File.WriteAllText(DoneFile, JsonConvert.SerializeObject(sended));
            if (errorSend.Count > 0) Console.WriteLine(string.Join(",", errorSend));

            Console.WriteLine("END PROCESS, PRESS ENTER TO CONTINUE");
            Console.ReadLine();
        }
    }
}