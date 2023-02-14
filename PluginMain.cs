using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Advanced_Combat_Tracker;
using Eorzea;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace Amatsumi
{
    /// <summary>
    /// プラグインのメインクラス
    /// </summary>
    internal class PluginMain : ConfigForm, IActPluginV1
    {

        private readonly string settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config\\Amatsumi.config.xml");
        SettingsSerializer xmlSettings;
        Label lblStatus;    // The status label that appears in ACT's Plugin tab
        ListBox log; // for log display
        OverlayForm overlay; // overlay window
        readonly Eorzea.Clock eorzeaClock;
        readonly Eorzea.Weather eorzeaWeather;
        Label infoBox;
        System.Timers.Timer timer;

        /// <summary>
        /// Constructor
        /// </summary>
        public PluginMain()
        {
            eorzeaClock = new Eorzea.Clock();
            eorzeaWeather = new Eorzea.Weather();

            // Overlay Mini Window
            overlay = new OverlayForm();

            infoBox = overlay.InfoBox;
        }


        #region IActPluginV1 Members

        /// <summary>
        /// プラグインが有効化されたときに呼び出される
        /// </summary>
        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            lblStatus = pluginStatusText;   // Hand the status label's reference to our local var

            pluginScreenSpace.Text = "Amatsumi";
            pluginScreenSpace.Controls.Add(this);   // Add this UserControl to the tab ACT provides

            xmlSettings = new SettingsSerializer(this); // Create a new settings serializer and pass it this instance
            LoadSettings();

            // List box for log display
            log = new ListBox
            {
                Dock = DockStyle.Bottom,
                Height = 250
            };

            overlay.Controls.Add(log);
            overlay.Show();

            ActGlobals.oFormActMain.BeforeLogLineRead += OnBeforeLogLineRead;

            timer = new System.Timers.Timer
            {
                Interval = 1000
            };
            timer.Elapsed += (o, e) =>
            {
                try
                {
                    DisplayUpdate();
                }
                catch (Exception ex)
                {
                    Log("Error: Update: {0}", ex.ToString());
                }
            };
            timer.Start();

            Log("Plugin Started.");

            // 起動時のゾーンを得る
            string currentET = eorzeaClock.GetCurrentET(ActGlobals.oFormActMain.LastKnownTime);
            // 起動時の天気を得る
            string currentWeather = eorzeaWeather.GetWeather(ActGlobals.oFormActMain.CurrentZone, ActGlobals.oFormActMain.LastKnownTime);
            
            Log(ActGlobals.oFormActMain.CurrentZone + currentET + " -- " + currentWeather);

            lblStatus.Text = "Plugin Started";
        }

        /// <summary>
        /// プラグインが無効化されたときに呼び出される
        /// </summary>
        public void DeInitPlugin()
        {
            SaveSettings();
            overlay.Close();
            timer.Stop();

            // Unsubscribe
            ActGlobals.oFormActMain.BeforeLogLineRead -= OnBeforeLogLineRead;

            lblStatus.Text = "Plugin Exited";
        }
        #endregion

        /// <summary>
        /// プラグイン設定をロードする
        /// </summary>
        private void LoadSettings()
        {
            // Add any controls you want to save the state of
            xmlSettings.AddControlSetting(textBox1.Name, textBox1);

            if (File.Exists(settingsFile))
            {
                FileStream fs = new FileStream(settingsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                XmlTextReader xReader = new XmlTextReader(fs);

                try
                {
                    while (xReader.Read())
                    {
                        if (xReader.NodeType == XmlNodeType.Element)
                        {
                            if (xReader.LocalName == "SettingsSerializer")
                            {
                                xmlSettings.ImportFromXml(xReader);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "Error loading settings: " + ex.Message;
                }
                xReader.Close();
            }
        }

        /// <summary>
        /// プラグイン設定を保存する
        /// </summary>
        private void SaveSettings()
        {
            FileStream fs = new FileStream(settingsFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            XmlTextWriter xWriter = new XmlTextWriter(fs, Encoding.UTF8);
            xWriter.Formatting = Formatting.Indented;
            xWriter.Indentation = 1;
            xWriter.IndentChar = '\t';
            xWriter.WriteStartDocument(true);
            xWriter.WriteStartElement("Config");    // <Config>
            xWriter.WriteStartElement("SettingsSerializer");    // <Config><SettingsSerializer>
            xmlSettings.ExportToXml(xWriter);   // Fill the SettingsSerializer XML
            xWriter.WriteEndElement();  // </SettingsSerializer>
            xWriter.WriteEndElement();  // </Config>
            xWriter.WriteEndDocument(); // Tie up loose ends (shouldn't be any)
            xWriter.Flush();    // Flush the file buffer to disk
            xWriter.Close();
        }

        /// <summary>
        /// ACTログが読み込まれるときに実行されるイベントハンドラ
        /// </summary>
        /// <param name="isImport"></param>
        /// <param name="logInfo"></param>
        private void OnBeforeLogLineRead(bool isImport, LogLineEventArgs logInfo)
        {
            
            string log = logInfo.logLine; // The full log line
            string zone = logInfo.detectedZone;
            int type = logInfo.detectedType;
            DateTime time = logInfo.detectedTime;

            
            // ログの種類見るために羅列
            int[] excludes = { 0, 1, 2, 3, 4, 11, 12, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 33, 34, 35, 36, 37, 38, 39, 41, 42, 251, 256, 257, 260 };

            if (excludes.Contains(type)) return;

            string currentET = eorzeaClock.GetCurrentET(time);

            // 天気を得る
            string currentWeather = eorzeaWeather.GetWeather(zone, time);

            // 40:ChangeMap 
            if (type == 40)
            {
                Log("[zone] " + zone + currentET + " -- " + currentWeather);

            } else
            {
                Log(log + " --[zone] " + zone + " --[type]: " + type + " --[time]: " + time);
            }
        }

        /// <summary>
        /// Runs every second from timer, updating the InfoBox.
        /// </summary>
        private void DisplayUpdate()
        {
            string zone = ActGlobals.oFormActMain.CurrentZone;
            DateTime time = ActGlobals.oFormActMain.LastKnownTime;
            string currentET = eorzeaClock.GetCurrentET(time);
            string currentWeather = eorzeaWeather.GetWeather(zone, time);

            Info("[zone] " + zone + currentET + " -- " + currentWeather);
        }

        /// <summary>
        /// Add a new item to the ListBox for Log
        /// </summary>
        private void Log(string format, params object[] args)
        {
            log.Items.Add(DateTime.Now.ToString() + " | " + string.Format(format, args));
        }

        /// <summary>
        /// Set the Text property of the InfoBox.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        private void Info(string format, params object[] args)
        {
            infoBox.Text = string.Format(format, args);
        }
    }

}
