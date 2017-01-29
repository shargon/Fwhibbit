using Newtonsoft.Json;
using System;
using System.IO;

namespace ChartResume
{
    public class Config
    {
        #region Database
        public string DatabasePassword { get; set; }
        public string DatabaseUserID { get; set; }
        public string DatabaseServer { get; set; }
        public uint DatabasePort { get; set; }
        public string Database { get; set; }
        #endregion

        #region Mail
        #region Smtp
        public string SmtpServer { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPassword { get; set; }
        public bool SmtpEnableSsl { get; set; }
        public int SmtpPort { get; set; }
        #endregion
        public string MailBody { get; set; }
        public string MailSubject { get; set; }

        #endregion

        #region Methods
        public static Config LoadFromFile(string file)
        {
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(file));
        }
        public void SaveToFile(string file)
        {
            File.WriteAllText(file, JsonConvert.SerializeObject(this));
        }
        #endregion
    }
}