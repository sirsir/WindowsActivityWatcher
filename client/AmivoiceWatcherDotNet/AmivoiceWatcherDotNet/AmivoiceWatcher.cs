using System;
using System.Drawing;
using System.Windows.Forms;

//Registry CRUD
using Microsoft.Win32;
using System.Net;
using System.IO;

using System.Globalization;

using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;



namespace AmivoiceWatcher
{
    public class AmivoiceWatcher : Form
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public static FormDummy formDummy = new FormDummy();

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        private Thread threadComputerInfo;
        private Thread threadUserActivity;
        //private Thread threadVdoUploader;
        private Thread threadRabbitMQ;

        [STAThread]
        public static void Main()
        {
            try
            {
                //For datetime format
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");

                //during init of application bind to this event  
                SystemEvents.SessionEnding += new SessionEndingEventHandler(SystemEvents_SessionEnding);

                // Allow only one instant of AmivoiceWatcher
                bool createdNew = true;
                using (Mutex mutex = new Mutex(true, "Amivoice Watcher", out createdNew))
                {
                    if (createdNew)
                    {
                        Application.Run(new AmivoiceWatcher());
                    }
                    else
                    {
                        Process current = Process.GetCurrentProcess();
                        foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                        {
                            if (process.Id != current.Id)
                            {
                                SetForegroundWindow(process.MainWindowHandle);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public AmivoiceWatcher()
        {
            try
            {
#if DEBUG
                Console.WriteLine("Mode=Debug");
                //DebugHelper();
#endif

                Globals.CreateAllDirectoryAndFiles();

                Configuration_SetFile();                

                InitializeGUI();

                //~ Start thread
                ComputerInfo.SubmitComputerLog(ComputerInfo.SubmitComputerLogMode.Startup);

                //~ Start UserActivityThread
                threadUserActivity = new Thread(new ThreadStart(UserActivityThread.ThreadMain));
                threadUserActivity.Start();

                threadComputerInfo = new Thread(new ThreadStart(ComputerInfo.ThreadMain));
                threadComputerInfo.Start();

                threadRabbitMQ = new Thread(new ThreadStart(RabbitMQWrapper.ThreadMain));
                threadRabbitMQ.Start();

#if DEBUG
                Globals.Notifications.PopupWelcomeMessage();
#endif




                //DONT DELETE
                //StartCaptureScreenRecord();                

            }
            catch (Exception e)
            {
                Globals.log.Error(e.ToString());
            }
        }

        public class Notifications {

            private static Form getFormByName(string nameIn)
            {
                Form formOut = null;

                foreach (Form frm in formDummy.OwnedForms)
                {
                    if (frm.Name == nameIn)
                    {
                        formOut = frm;

                    }


                }

                return formOut;
            }


            public static void updateLongNotification(object[] param)
            {
                var frmLong = (FormLongNotification)getFormByName("FormLongNotification");

               
                //frmLong.webBrowser1.Document.InvokeScript("watchercliFn.addNewNotification", param);

                frmLong.webBrowser1.Document.InvokeScript("externalFx_addNewNotification", param);
            }

            public delegate void PopupCallback(string jsonMsg, int duration, FormAnimator.AnimationMethod animationMethod, FormAnimator.AnimationDirection animationDirection);

            

            public static void Popup(string jsonMsg, int duration, FormAnimator.AnimationMethod animationMethod, FormAnimator.AnimationDirection animationDirection)
            {


                // InvokeRequired required compares the thread ID of the
                // calling thread to the thread ID of the creating thread.
                // If these threads are different, it returns true.
                if (formDummy.InvokeRequired)
                {
                    Notifications.PopupCallback d = new Notifications.PopupCallback(Popup);
                    //formDummy.Invoke(d, new object[] { dictMsg, duration, animationMethod, animationDirection });
                    formDummy.Invoke(d, new object[] { jsonMsg, duration, animationMethod, animationDirection });
                }
                else
                {
                    var dictMsg =  Globals.functions.Json_toDictionary(jsonMsg);


                    if (! dictMsg.ContainsKey("title"))
                    {
                        dictMsg["title"] = "";
                    }

                    if (!dictMsg.ContainsKey("content"))
                    {
                        dictMsg["content"] = "";
                    }

                    if (!dictMsg.ContainsKey("timestamp"))
                    {
                        dictMsg["timestamp"] = "";
                    }

                    if (!dictMsg.ContainsKey("level"))
                    {
                        dictMsg["level"] = "";
                    }

                    dictMsg["body"] = dictMsg["content"];

                    if (dictMsg["level"] == "")
                    {
                        var title = dictMsg["title"];
                        var body = dictMsg["body"];

                        int duration2use;
                        if (dictMsg.ContainsKey("timeout"))
                        {
                            Int32.TryParse(dictMsg["timeout"], out duration2use);
                            //duration2use = dictMsg["timeout"];
                        }
                        else
                        {
                            duration2use = duration;
                        }

                        int width = 0,
                            height = 0;
               

                        if (dictMsg.ContainsKey("width"))
                        {
                            Int32.TryParse(dictMsg["width"], out width);
                        }

                        if (dictMsg.ContainsKey("height"))
                        {
                            Int32.TryParse(dictMsg["height"], out height);
                        }


                        Notification toastNotification = new Notification(title, body, duration2use, animationMethod, animationDirection, width, height);
                        toastNotification.Show(formDummy);

                        var htmlString = Globals.htmlStringTemplatePure;
                        htmlString = Globals.functions.HtmlWithAbsolutePaths("./ReplaceWithAbsolutePath", htmlString);
                        htmlString = htmlString.Replace("[[body]]", body);
                        var webBrowser1 = toastNotification.Controls["webBrowser1"] as WebBrowser;
                        webBrowser1.DocumentText = htmlString;

                        object[] param = new object[1];
                        Dictionary<string, string> dictMsgFormatted = new Dictionary<String, String>();

                        string[] key2copy = { "title", "body", "timestamp" };

                        foreach (string key in key2copy)
                        {
                            dictMsgFormatted.Add(key, dictMsg[key]); ;
                        }

                      
                        param[0] = Globals.functions.Json_toJson(dictMsgFormatted);




                        Notifications.updateLongNotification(param);
                    }
                    else
                    {
                        //this.textBox1.Text = text;
                        //FormDummy df = new FormDummy();
                        //df.Text = title;
                        //df.Show(formDummy);
                        var title = dictMsg["title"];


                        //var body = data;
                        var body = dictMsg["body"];
                        var datetime = "";
                        if (dictMsg.ContainsKey("timestamp"))
                        {
                            datetime = dictMsg["timestamp"];
                            var datesplit = datetime.Split(' ');
                            if (datesplit[0] == DateTime.Now.ToString("yyyy-MM-dd"))
                            {
                                var datetimeArray = datesplit[1].Split(':');
                                datetime = datetimeArray[0] + ":" + datetimeArray[1];
                            }
                        }
                        else
                        {
                            datetime = "";
                        }

                        int duration2use;
                        if (dictMsg.ContainsKey("timeout"))
                        {
                            Int32.TryParse(dictMsg["timeout"], out duration2use);
                            //duration2use = dictMsg["timeout"];
                        }
                        else
                        {
                            duration2use = duration;
                        }


                        Notification toastNotification = new Notification(title, body, duration2use, animationMethod, animationDirection);


                        if (Globals.notification_dialog_isTransparent)
                        {
                            toastNotification.Opacity = Globals.notification_dialog_opacity;
                        }
                        else
                        {
                            toastNotification.Opacity = 1;
                        }
                        

                        toastNotification.Show(formDummy);

                        var htmlString = Globals.htmlStringTemplate;

#if DEBUG
                        //htmlString = File.ReadAllText(@"\\VBOXSVR\SharedLocal\AMIVOICE\projects\AmivoiceWatcher\amivoice_watcher\AmiVoiceWatcher2\AmivoiceWatcherDotNet\AmivoiceWatcherDotNet\_Src\Template\NotificationTemplate.html");
#endif

                        htmlString = Globals.functions.HtmlWithAbsolutePaths("./ReplaceWithAbsolutePath/", htmlString);

                        htmlString = htmlString.Replace("[[messageJson]]", Globals.functions.Json_toJson(jsonMsg));

                        htmlString = htmlString.Replace("[[title]]", title);
                        htmlString = htmlString.Replace("[[body]]", body);
                        htmlString = htmlString.Replace("[[datetime]]", datetime);

                        var backgroundColor = "#e3f7fc";
                        var borderColor = "#8ed9f6";

                        switch (dictMsg["level"])
                        {
                            case "notice":
                                backgroundColor = "#e3f7fc";
                                borderColor = "#8ed9f6";
                                //dictMsg["image"] = Globals.notification_image.notice;
                                break;
                            case "error":
                                backgroundColor = "#ffecec";
                                borderColor = "#f5aca6";
                                //dictMsg["image"] = Globals.notification_image.error;
                                break;
                            case "success":
                                backgroundColor = "#e9ffd9";
                                borderColor = "#a6ca8a";
                                //dictMsg["image"] = Globals.notification_image.success;
                                break;
                            case "warning":
                                backgroundColor = "#fff8c4";
                                borderColor = "#f2c779";
                                //dictMsg["image"] = Globals.notification_image.warning;
                                break;
                            case "question":
                                backgroundColor = "#fff8c4";
                                borderColor = "#f2c779";
                                //dictMsg["image"] = Globals.notification_image.question;
                                break;
                            case "custom":
                                backgroundColor = Globals.functions.Color2HexConverter(Globals.notification_dialog_backgroundColor);
                                //borderColor =  Globals.functions.Color2HexConverter(ControlPaint.Dark(Globals.notification_dialog_backgroundColor));
                                borderColor = Globals.functions.Color2HexConverter(Globals.functions.DarkerColor(Globals.notification_dialog_backgroundColor, 90f));
                                break;
                                //default:
                                //    htmlString = htmlString.Replace("[[title]]", title);
                        }




                        htmlString = htmlString.Replace("[[background-color]]", backgroundColor);
                        htmlString = htmlString.Replace("[[border-color]]", borderColor);



                        string foregroundColor;

                        if (dictMsg["level"] == "custom")
                        {
                            foregroundColor = Globals.functions.Color2HexConverter(Globals.functions.CalculateForegroundColor(Globals.notification_dialog_backgroundColor));
                        }
                        else
                        {
                            foregroundColor = "#000000";
                        }

                        htmlString = htmlString.Replace("[[foreground-color]]", foregroundColor);

                        var strImage = "default";

                        if (dictMsg.ContainsKey("image"))
                        {
                            strImage = dictMsg["image"];
                        }
                        else if (dictMsg.ContainsKey("icon"))
                        {
                            strImage = dictMsg["icon"];
                        }



                        if (Globals.notification_show_icon)
                        {
                            if (strImage == "default")
                            {
                                switch (dictMsg["level"])
                                {
                                    case "notice":
                                        strImage = Globals.notification_image_file.notice;
                                        break;
                                    case "error":
                                        strImage = Globals.notification_image_file.error;
                                        break;
                                    case "success":
                                        strImage = Globals.notification_image_file.success;
                                        break;
                                    case "warning":
                                        strImage = Globals.notification_image_file.warning;
                                        break;
                                    case "question":
                                        strImage = Globals.notification_image_file.question;
                                        break;

                                        //default:
                                        //    htmlString = htmlString.Replace("[[title]]", title);
                                }
                                if (!String.IsNullOrEmpty(strImage))
                                {
                                    htmlString = htmlString.Replace("[[image_src]]", strImage);
                                }
                            }
                            else
                            {
                                if (!strImage.StartsWith("data:"))
                                {
                                    strImage = Path.Combine(Globals.PathLocalAppData, "Template", "image", strImage);
                                }
                                htmlString = htmlString.Replace("[[image_src]]", strImage);
                            }


                        }



                        if (dictMsg.ContainsKey("answers"))
                        {


                            htmlString = htmlString.Replace("[[answers]]", dictMsg["answers"]);

                        }
                        else
                        {
                            htmlString = htmlString.Replace("[[answers]]", "");
                        }

                        if (dictMsg.ContainsKey("link"))
                        {
                            var splitTemp = dictMsg["link"].Split('>');

                            htmlString = htmlString.Replace("[[linkCaption]]", splitTemp[0]);
                            htmlString = htmlString.Replace("[[link]]", "javascript:window.external.openLinkInDefaultBrowser('" + splitTemp[1] + "')");
                        }

                        if (dictMsg.ContainsKey("links"))
                        {


                            htmlString = htmlString.Replace("[[links]]", dictMsg["links"]);

                        }
                        var webBrowser1 = toastNotification.Controls["webBrowser1"] as WebBrowser;
                        webBrowser1.DocumentText = htmlString;
                    }

                    

                   
                    //webBrowser1.Navigating += new WebBrowserNavigatingEventHandler(toastNotification.webBrowser1_Navigating);


                    //toastNotification.Height = webBrowser1.Size.Height*2;


                    //webBrowser1.Height = toastNotification.ClientSize.Height;

                    //toastNotification.Height = webBrowser1.Height;
                    //toastNotification.Height = 500;
                    //webBrowser1.Document.Body.ScrollRectangle.Width

                    //Globals.log.Debug("webBrowser1.Height");
                    //Globals.log.Debug(webBrowser1.Height);

                    //webBrowser1.BringToFront();

                    //toastNotification.TransparencyKey = System.Drawing.ColorTranslator.FromHtml(backgroundColor);
                    //toastNotification.Opacity = Globals.notification_dialog_opacity;

                    Globals.notification_client_numberNow++;
                }
            }



        }

        public static void StartCaptureScreenRecord()
        {
            if (Globals.EnabledCaptureSceen)
            {
                try
                {
                    //~ DONT DELETE
                    //Thread threadCaptureScreen = new Thread(new ThreadStart(CaptureScreenThread.ThreadMain));
                    //threadCaptureScreen.Start();
                }
                catch (Exception e)
                {
                    Globals.log.Error(e.ToString());
                }

            }
        }

        private void Configuration_SetFile()
        {
            try
            {
                Globals.log.Debug("Set configuration....");
                //---------- Download config file
                string str_content = "";

                string path4configReal = "";

                // prepare path for (eg C:\Users\UserName\AppData\Local\AmivoiceWatcher\configuration.txt)
                //string path4config = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                //path4config = Path.Combine(path4config, "AmivoiceWatcher");



                string path4config = Path.Combine(Globals.PathLocalAppData, @"configuration.txt");

                Globals.Initilize();
                // get url from registry
                // then load file content
                Globals.log.Debug("Setup configuration value ...");
                string ConfigurationURL = "";
                try
                {
                    Globals.log.Debug("Trying to get ConfigurationURL in the registry " + Globals.reg4ConfigurationURL + "/" + Globals.regkey4ConfigurationURL);
                    ConfigurationURL = (string)Registry.GetValue(Globals.reg4ConfigurationURL, Globals.regkey4ConfigurationURL, null);
                }
                catch (Exception e)
                {
                    Globals.log.Warn("Can NOT get value from Registry");
                    Globals.log.Warn(e.ToString());

                }

                if (String.IsNullOrEmpty(ConfigurationURL))
                {

                    try
                    {
                        Globals.log.Debug("ConfigurationURL is String.Empty . Trying to get the value by windows64bit method...");
                        ConfigurationURL = RegistryWOW6432.GetRegKey64(RegHive.HKEY_LOCAL_MACHINE, Globals.reg4ConfigurationURL.Replace(@"HKEY_LOCAL_MACHINE\", ""), Globals.regkey4ConfigurationURL);

                        if (String.IsNullOrEmpty(ConfigurationURL))
                        {
                            ConfigurationURL = RegistryWOW6432.GetRegKey32(RegHive.HKEY_LOCAL_MACHINE, Globals.reg4ConfigurationURL.Replace(@"HKEY_LOCAL_MACHINE\", ""), Globals.regkey4ConfigurationURL);
                        }
                    }
                    catch (Exception e)
                    {
                        Globals.log.Debug("Error in getting the value by windows64bit method ");
                        Globals.log.Error(e.ToString());
                    }


                    if (String.IsNullOrEmpty(ConfigurationURL))
                    {
                        Globals.log.Debug("ConfigurationURL is still String.Empty");
                    }

                }

                if (!String.IsNullOrEmpty(ConfigurationURL))
                {
                    Globals.log.Debug("Start downloading config file from ConfigurationURL(registry) = " + ConfigurationURL);
                    try
                    {

                        HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(ConfigurationURL);
                        HttpWebResponse webResp = (HttpWebResponse)webRequest.GetResponse();

                        if (webResp == null || webResp.StatusCode != HttpStatusCode.OK)
                        {
                            //if error str_content = "", so move code to the end of this method
                        }
                        else
                        {
                            using (Stream stream = webResp.GetResponseStream())
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                str_content = reader.ReadToEnd();
                            }




                            // Append text to an existing file named "WriteLines.txt".
                            //using (StreamWriter outputFile = new StreamWriter(path4config, true))
                            //{
                            //    outputFile.WriteLine(str_content);
                            //}

                            System.IO.File.WriteAllText(path4config, str_content);


                            Globals.log.Debug("Complete uploading file to " + path4config);

                            //Console.WriteLine(path4config);


                            //log.Debug("Load file from internet completed");
                        }


                    }
                    catch (Exception e)
                    {
                        Globals.log.Warn("Can not load config file from server! The recorded one(if exists) will be used.");
                        Globals.log.Warn(e.ToString());
                    }

                }


                //---------- Set which config. file will be used.
                if (File.Exists(path4config))
                {
                    path4configReal = path4config;
                }
                else
                {
                    //To get the location the assembly normally resides on disk or the install directory
                    //string path4configSameAsExe = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                    string path4configSameAsExe = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    path4configSameAsExe = Path.Combine(path4configSameAsExe, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".ini");

                    if (File.Exists(path4configSameAsExe))
                    {
                        path4configReal = path4configSameAsExe;
                    }

                }


                if (String.IsNullOrEmpty(path4configReal))
                {
                    Globals.log.Warn("There is no configuration file! The default code-embed one will be used.");
                }
                else
                {
                    Globals.log.Debug("Setup configuration from file: " + path4configReal);
                    try
                    {


                        Globals.iniData = Globals.iniParser.ReadFile(path4configReal);


                        foreach (IniParser.Model.KeyData key in Globals.iniData.Sections.GetSectionData("watcher").Keys)
                        {
                            Globals.configuration[key.KeyName] = key.Value;
                        }

                    }
                    catch (Exception e)
                    {
                        Globals.log.Warn(String.Format("Can not parse ini file: {0}", e.ToString()));
                    }
                }




                //For debug
                foreach (string key in Globals.configuration.Keys)
                {
                    Globals.log.Debug(String.Format("Globals.configuration[{0}]={1}", key, Globals.configuration[key]));
                }

                //Post set configuration value 
                Globals.Configuration_SetValueFromSetFile();

                Globals.log.Debug("Set configuration....OK");
            }
            catch (Exception e)
            {
                Globals.log.Error(e.ToString());
            }
            


        }

        private class LongNotification
        {
            public static void _init()
            {

                FormLongNotification fTemp = new FormLongNotification();
                fTemp.Show(formDummy);

                fTemp.Hide();

            }
        }

        private void InitializeGUI()
        {
            try
            {
                Globals.log.Debug("Start InitializeGUI()");
                //---- Setup menu on Tray Icon
                trayMenu = new ContextMenu();

                MenuItem mi_Help = new MenuItem("Help");
                MenuItem mi_Exit = new MenuItem("Exit (&X)", OnExit);

                MenuItem mi_Transparent = new MenuItem("Transparent", TrayMenu.Transparent);

                mi_Transparent.Checked = Globals.notification_dialog_isTransparent;

                mi_Help.MenuItems.Add(new MenuItem("Open log file (&L)", TrayMenu.OpenLogFile));
                mi_Help.MenuItems.Add(new MenuItem("Open configuration file (&C)", TrayMenu.OpenConfigFile));
                mi_Help.MenuItems.Add(new MenuItem("Open log configuration file (&O)", TrayMenu.OpenLogConfigFile));
                mi_Help.MenuItems.Add(new MenuItem("Version dialog (&V)", TrayMenu.OpenVersionDialog));

                // Uncomment to enable
                // some function inside is obsoleted
                //mi_Help.MenuItems.Add(new MenuItem("Config Notification", TrayMenu.OpenConfigNotificationDialog));

                trayMenu.MenuItems.AddRange(new MenuItem[] { mi_Transparent, mi_Help, mi_Exit });
               

                // Create a tray icon
                trayIcon = new NotifyIcon();
                trayIcon.Text = "AmivoiceWatcher";
                trayIcon.Icon = new Icon(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".ico", 40, 40);

                // Add menu to tray icon and show it.
                trayIcon.ContextMenu = trayMenu;
                trayIcon.Visible = true;

                trayIcon.MouseUp += new MouseEventHandler(TrayMenu.trayIcon_MouseLeftClick);

                LongNotification._init();

                Globals.log.Debug("Finish InitializeGUI()");
            }
            catch (Exception e)
            {
                Globals.log.Error(e.ToString());
            }

            
        }

        protected override void OnLoad(EventArgs ev)
        {
            try
            {
                Visible = false; // Hide form window.
                ShowInTaskbar = false; // Remove from taskbar.

                Globals.log.Debug(String.Format("{0}({1}) is login by {2}(ip:{3} , mac_address:{4})", Globals.myComputerInfo.ComputerName, Globals.myComputerInfo.Os, Globals.myComputerInfo.UserName, Globals.myComputerInfo.Ip, Globals.myComputerInfo.MacAddress));

                base.OnLoad(ev);
            }
            catch (Exception e)
            {
                Globals.log.Error(e.ToString());
            }
            
        }


        private void OnExit(object sender, EventArgs ev)
        {
            try
            {                
                Globals.log.Debug("Start OnExit()");

                RabbitMQWrapper.BlnThreadAborted = true;
                
                Globals.blnExitByUser = true;

                ComputerInfo.SubmitComputerLog(ComputerInfo.SubmitComputerLogMode.Logoff);

                
                ComputerInfo.ThreadMainExit();

                threadComputerInfo.Abort();

                UserActivityThread.blnThreadAborted = true;
                threadUserActivity.Abort();

                if (Globals.EnabledCaptureSceen)
                {
                    //~ Auto exit()
                    //No CaptureScreen.blnThreadAborted = true;
                    Globals.log.Debug("Try Exit capture screen");

                    //CaptureScreenThread.Exit();

                }
                    

                
                formDummy.Close();

                

                Globals.log.Debug("Finish OnExit()");
                Application.Exit();
            }
            catch (Exception e)
            {
                Globals.log.Error(e.ToString());
            }

        }

        public class TrayMenu
        {

            public static void ToggleFormLongNotification()
            {
                foreach (Form frm in formDummy.OwnedForms)
                {
                    if (frm.Name == "FormLongNotification")
                    {
                        if (frm.Visible)
                        {
                            frm.Hide();
                        }
                        else
                        {
                            frm.Show(formDummy);
                        }

                    }


                }
            }

            public static void trayIcon_MouseLeftClick(object sender, MouseEventArgs e)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {

                    ToggleFormLongNotification();

                    // code for adding context menu
                }
            }


            public static void Transparent(object sender, EventArgs ev)
            {
                try
                {
                    MenuItem mi = sender as MenuItem;
                    
                    Globals.notification_dialog_isTransparent = !Globals.notification_dialog_isTransparent;

                    if (Globals.notification_dialog_isTransparent)
                    {
                        foreach (Form frm in formDummy.OwnedForms)
                        {
                            frm.Opacity = Globals.notification_dialog_opacity;
                        }
                    }else
                    {
                        foreach (Form frm in formDummy.OwnedForms)
                        {
                            frm.Opacity = 1;
                        }
                    }                                                        

                    mi.Checked = Globals.notification_dialog_isTransparent;
                }
                catch (Exception e)
                {
                    Globals.log.Error(e.ToString());
                }

            }


            public static void OpenLogFile(object sender, EventArgs ev)
            {
                try
                {
                    var path2file = Path.Combine(Globals.PathLocalAppData, @"AmivoiceWatcher.log");

                    Process.Start("notepad.exe", path2file);
                }
                catch (Exception e)
                {
                    Globals.log.Error(e.ToString());
                }

            }

            public static void OpenConfigFile(object sender, EventArgs ev)
            {
                try
                {
                    string fileName = "";

                    string path4config = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    path4config = Path.Combine(path4config, "AmivoiceWatcher");

                    path4config = Path.Combine(path4config, @"configuration.txt");

                    if (File.Exists(path4config))
                    {
                        fileName = path4config;
                    }
                    else
                    {
                        //To get the location the assembly normally resides on disk or the install directory
                        string path4configSameAsExe = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                        path4configSameAsExe = Path.Combine(path4configSameAsExe, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".ini");

                        if (File.Exists(path4configSameAsExe))
                        {
                            fileName = path4configSameAsExe;
                        }

                    }

                    Process.Start("notepad.exe", fileName);
                }
                catch (Exception e)
                {
                    Globals.log.Error(e.ToString());
                }

            }

            public static void OpenLogConfigFile(object sender, EventArgs ev)
            {
                try
                {
                    var fileName = "AmivoiceWatcher.exe.config";
                    Process.Start("notepad.exe", fileName);

                }
                catch (Exception e)
                {
                    Globals.log.Error(e.ToString());
                }

            }



            public static void OpenConfigNotificationDialog(object sender, EventArgs ev)
            {
                try
                {
                    using (FormConfigNotification fTemp = new FormConfigNotification())
                    {
                        fTemp.ShowDialog(formDummy);
                    }
                }
                catch (Exception e)
                {
                    Globals.log.Error(e.ToString());
                }
            }

            public static void OpenVersionDialog(object sender, EventArgs ev)
            {
                try
                {
                    using (FormAbout fAbout = new FormAbout())
                    {
                        fAbout.ShowDialog(formDummy);
                    }
                }
                catch (Exception e)
                {
                    Globals.log.Error(e.ToString());
                }

            }

        }


        // For dealing with shutdown signal
        private static void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs ev)
        {
            try
            {
                Globals.log.Debug("Exit Program because Receive SystemEvents_SessionEnding=" + ev.Reason.ToString());
                if (Environment.HasShutdownStarted || (ev.Reason == SessionEndReasons.SystemShutdown))
                {
                    ev.Cancel = true;
                    //Tackle Shutdown
                    ComputerInfo.SubmitComputerLog(ComputerInfo.SubmitComputerLogMode.Logoff);
                    ev.Cancel = false;
                }
                else if (ev.Reason == SessionEndReasons.Logoff)
                {
                    //Tackle log off
                    ComputerInfo.SubmitComputerLog(ComputerInfo.SubmitComputerLogMode.Logoff);
                }
                else
                {
                    ComputerInfo.SubmitComputerLog(ComputerInfo.SubmitComputerLogMode.Logoff);
                }
            }
            catch (Exception e)
            {
                Globals.log.Error(e.ToString());
            }
        }



        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }


    }


}