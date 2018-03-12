﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IniParser;
using IniParser.Model;

using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Drawing;

namespace AmivoiceWatcher
{
    public class WebClientWithAwesomeTimeouts : WebClient
    {
        public TimeSpan? Timeout { get; set; }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest webRequest = base.GetWebRequest(uri);
            if (this.Timeout.HasValue)
            {
                webRequest.Timeout = (int)this.Timeout.Value.TotalMilliseconds;
            }
            return webRequest;
        }
    }

    class Globals
    {
        public static Dictionary<string, string> configuration;

        public static string reg4ConfigurationURL;
        public static string regkey4ConfigurationURL;

        public static log4net.ILog log;

        private static Random random = new Random();

        public static FileIniDataParser iniParser;

        public static IniData iniData;

        public static ComputerInfo myComputerInfo;
        public static string PathLocalAppData = Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), @"AmivoiceWatcher");
        public static bool EnabledCaptureSceen = false;
        public static bool EnabledCaptureSceenshot = false;

        public static string currentDirectroy = Directory.GetCurrentDirectory();
        
        public static bool blnExitByUser = false;

        //notification
        public static int notification_client_max = 4;
        public static int notification_duration = 20;
        public static int notification_client_numberNow;
        public static Color notification_dialog_backgroundColor = Color.Gray;
        public static double notification_dialog_opacity = 0.8;
        public static bool notification_dialog_isTransparent = true;
        public static Base64ImageFile notification_image_file = new Base64ImageFile();
        public static bool notification_show_icon = true;
        public static string htmlStringTemplate;
        public static string htmlStringTemplatePure;
        public static string htmlStringTemplateSuper;

        public const int NO_OF_SHOTS_TO_RECALCULATE_DIR_SIZE = 200;

#if DEBUG
        private static DateTime dtUploadStart;
        private static double TotalUploadTime = 0;

        private static Stopwatch stopWatch = new Stopwatch();
        private static List<string> stopwatchTimes = new List<string>();

#endif


        static Globals()
        {
            log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        
        public static void Initilize()
        {
            configuration = new Dictionary<string, string>
                {
                    {@"agentactivity.foreground_check.enabled",@"1"},
                    {@"agentactivity.idle_check.enabled",@"1"},
                    {@"agentactivity.idle_check.min_time.sec",@"10"},
                    {@"agentactivity.ie_history_check.enabled",@"1"},
                    {@"agentactivity.ie_history_check.timer.sec",@"30"},
                    {@"agentactivity.localstorage.filename",@"%TEMP%\aa"},
                    {@"agentactivity.senddata.timer.sec",@"20"},
                    {@"agentactivity.upload_fail_recoveryN",@"24"},
                    {@"agentactivity.url",@"http://192.168.1.51/aohs/webapi/agent_activity"},
                    {@"computer_info",@"cti,advw,java,os,mediaplayer,ie"},
                    {@"computer_info.advw",@"C:Program FilesAmiVoiceAudio ViewerAmiAdvw.dll"},
                    {@"computer_info.cti",@"C:Program FilesEventCaptureCTI_TOOLBAR.exe"},
                    {@"computer_info.moss",@"127.0.0.1"},
                    {@"target.enable_watching_ie_control",@"1"},
                    {@"target.finding.executable_filename",@"CTI_TO"},
                    {@"target.finding.window_class.name",@"ThunderRT6FormDC"},
                    {@"target.finding.window_class.position",@"0"},
                    {@"target.finding.window_title.anti_pattern",@"Initiate Action,Log"},
                    {@"target.finding.window_title.anti_pattern_delim",@",@"},
                    {@"target.window_title.logout_pattern",@"logged out"},
                    {@"target.window_title.number_of_characters",@"48"},
                    {@"target.window_title.pattern", @"SoftPhone :: ACD"",""{a+} : Ext"",""{a+} : Ag ID"",""{a+}"},
                    {@"target.window_title.result_index.agentid",@"2"},
                    {@"target.window_title.result_index.extension",@"1"},
                    {@"target.window_title.result_index.extension2",@"0"},
                    {@"target.window_title.start_index",@"0"},
                    {@"update_computer.retry_count_max",@"5"},
                    {@"update_computer.retry_wait",@"10"},
                    {@"update_computer.sending_logoff",@"1"},
                    {@"update_computer.url",@"http://192.168.1.51/aohs/webapi/update_computer_log"},
                    {@"update_extension.url",@"http://192.168.1.51/aohs/webapi/update_extension"},
                    {@"watcher.log4cxx_properties.url",@""},
                    {@"watcher.startup_delay.sec",@"0"},
                    {@"watcher.timer.msec",@"1000.99"},
                    //NEW
                    {@"screencapture.record.vdo.enable",@"1"},
                    {@"screencapture.record.screenshot.enable",@"1"},
                    {@"screencapture.record.vdo.fps",@"4"},
                    {@"screencapture.record.vdo.resize_scale",@"0.5"},
                    {@"screencapture.record.timer.sec",@"2"},
                    {@"watcher.local_app.limit_size_Mb",@"100"},
                    {@"screencapture.record.vdo.upload.url",@"http://192.168.1.153/upload.php" },
                    {@"computer_info.senddata.timer.min",@"60"},
                    {@"screencapture.upload.timer.sec",@"10"},
                    //New not add to web
                    {@"notification.client.max",@"3"},
                    {@"notification.show.icon","true" },
                    {@"notification.duration","20" },
                    {@"notification.register_url",@"http://192.168.1.88:3000/webapi/client_notify" }

              };

            reg4ConfigurationURL = @"HKEY_LOCAL_MACHINE\SOFTWARE\AmiVoice\AmiVoiceWatcher\1.0";
            regkey4ConfigurationURL = "ConfigurationURL";

            
            iniParser = new FileIniDataParser();

            iniData = new IniData();

            myComputerInfo = new ComputerInfo();

            htmlStringTemplate = File.ReadAllText(Path.Combine(Globals.PathLocalAppData, "Template", "NotificationTemplate.html"));
            htmlStringTemplatePure = File.ReadAllText(Path.Combine(Globals.PathLocalAppData, "Template", "NotificationTemplatePure.html"));
            htmlStringTemplateSuper = File.ReadAllText(Path.Combine(Globals.PathLocalAppData, "Template", "SuperNotificationTemplate.html"));


    }

        public static string Configuration_GetStringFromKey(string key)
        {
            if (configuration.ContainsKey(key))
            {
                return configuration[key];
            }else
            {
                return String.Empty;
            }
        }

        public static void Configuration_SetValueFromSetFile()
        {
            switch (Globals.Configuration_GetStringFromKey("screencapture.record.vdo.enable"))
            {
                case "true":
                case "1":
                case "True":
                case "TRUE":
                    EnabledCaptureSceen = true;
                    break;
                default:
                    EnabledCaptureSceen = false;
                    break;
            }

            switch (Globals.Configuration_GetStringFromKey("screencapture.record.screenshot.enable"))
            {
                case "true":
                case "1":
                case "True":
                case "TRUE":
                    EnabledCaptureSceenshot = true;
                    break;
                default:
                    EnabledCaptureSceenshot = false;
                    break;
            }

            switch (Globals.Configuration_GetStringFromKey("notification.show.icon"))
            {
                case "true":
                case "1":
                case "True":
                case "TRUE":
                    notification_show_icon = true;
                    break;
                default:
                    notification_show_icon = false;
                    break;
            }


            //~ notification_client_max
            if (!Int32.TryParse(Globals.configuration["notification.client.max"], out Globals.notification_client_max))
            {
                Globals.log.Error("Cant load Globals.notification_client_max. The program will use default value=" + Globals.notification_client_max);
            }

            if (!Int32.TryParse(Globals.configuration["notification.duration"], out Globals.notification_duration))
            {
                Globals.log.Error("Cant load Globals.notification_duration. The program will use default value=" + Globals.notification_duration);
            }

            if (Globals.notification_client_max > 10)
            {
                Globals.notification_client_max = 10;
            }


        }
        
        public static void CleanAllDirectory()
        {
            string[] paths = {
                //Globals.PathLocalAppData,
                //Path.Combine(Globals.PathLocalAppData, "Screenshots"),
                Path.Combine(Globals.PathLocalAppData, "ScreenshotsRAW"),
                //Path.Combine(Globals.PathLocalAppData, "ScreenshotsZIP"),
                //Path.Combine(Globals.PathLocalAppData, "ScreenshotsZipped"),
            };

            foreach (string path in paths)
            {
                string[] filePaths = Directory.GetFiles(path);
                foreach (var filePath in filePaths)
                    File.Delete(filePath);
            }


        }

        public static void CreateAllDirectoryAndFiles()
        {
            try
            {
                string[] paths = {
                Globals.PathLocalAppData,
                Path.Combine(Globals.PathLocalAppData, "Screenshots"),
                Path.Combine(Globals.PathLocalAppData, "ScreenshotsRAW"),
                Path.Combine(Globals.PathLocalAppData, "ScreenshotsZIP"),
                Path.Combine(Globals.PathLocalAppData, "ScreenshotsZipped"),
                Path.Combine(Globals.PathLocalAppData, "Template")
            };

                foreach (string path in paths)
                {
                    Directory.CreateDirectory(path);
                }

                CleanAllDirectory();

                


               
                var SourcePath = Path.Combine(System.Environment.CurrentDirectory, "_src", "Template");
                var DestinationPath = Path.Combine(Globals.PathLocalAppData, "Template");
                //Now Create all of the directories
                foreach (string dirPath in Directory.GetDirectories(SourcePath, "*",
                    SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));

                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in Directory.GetFiles(SourcePath, "*.*",
                    SearchOption.AllDirectories))
                    File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);

            }
            catch(Exception e)
            {
                Globals.log.Debug("Cant CreateAllDirectoryAndFiles()");
                Globals.log.Debug(e.ToString());
            }
            
        }

        public static void Debug_SetStartTime()
        {
#if DEBUG

            Globals.stopWatch.Start();
#else
            //Console.WriteLine("Mode=Release");
#endif
        }

        public static void Debug_CalculateTime()
        {
#if DEBUG
            Globals.stopWatch.Stop();

            TimeSpan ts = Globals.stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}",
            ts.TotalMilliseconds);
            Console.WriteLine("----------------Stopwatch timer (msec) " + elapsedTime);
#endif
        }

        public static void Debug_SetUploadStartTime()
        {
#if DEBUG
            //Console.WriteLine("Mode=Debug");
            //DebugHelper();
            Globals.dtUploadStart = DateTime.Now;
#else
            //Console.WriteLine("Mode=Release");
#endif
        }

        public static void Debug_CalculateUploadTime()
        {
#if DEBUG
            //Console.WriteLine("Mode=Debug");
            //DebugHelper();
            double duration = DateTime.Now.Subtract(Globals.dtUploadStart).TotalSeconds;
            Globals.TotalUploadTime += duration;
            Globals.log.Debug("Upload finished in " + duration + " sec. >>" + "Total: " + Globals.TotalUploadTime);
#else
            //Console.WriteLine("Mode=Release");
#endif
        }

        public class functions
        {

            /// <summary>
            /// Depth-first recursive delete, with handling for descendant 
            /// directories open in Windows Explorer.
            /// </summary>
            public static void DeleteDirectory(string path)
            {
                if (!Directory.Exists(path))
                {
                    return;
                }
                foreach (string directory in Directory.GetDirectories(path))
                {
                    DeleteDirectory(directory);
                }

                try
                {
                    Directory.Delete(path, true);
                }
                catch (IOException)
                {
                    Thread.Sleep(0);
                    Directory.Delete(path, true);
                }
                catch (UnauthorizedAccessException)
                {
                    Thread.Sleep(0);
                    Directory.Delete(path, true);
                }
            }


            public static bool DeleteFile(string filepath, int retryMax = 2, int retryWaitMilsec = 1000)
            {
                var intRetryCount = 0;

                while (File.Exists(filepath) && intRetryCount < retryMax)
                {
                    try
                    {
                        intRetryCount++;
                        File.Delete(filepath);

                    }
                    catch (Exception e)
                    {

                        Thread.Sleep(retryWaitMilsec);
                        Globals.log.Error("Cant delete file. Retrying now");
                        Globals.log.Error(e.ToString());
                    }

                }

                if (File.Exists(filepath))
                {
                    return false;
                }
                else
                {
                    return true;
                }


            }


            public static long DirSize(string str_dirpath)
            {
                DirectoryInfo d = new DirectoryInfo(str_dirpath);
                long size = 0;
                // Add file sizes.
                FileInfo[] fis = d.GetFiles();
                foreach (FileInfo fi in fis)
                {
                    size += fi.Length;
                }
                // Add subdirectory sizes.
                DirectoryInfo[] dis = d.GetDirectories();
                foreach (DirectoryInfo di in dis)
                {
                    size += DirSize(di.FullName);
                }
                return size;
            }

            public static void HttpPostRequestDictionaryAsync(string path, Dictionary<string, string> dictIn, Dictionary<string, string> dictIn2)
            {
                var dictInAll = dictIn.Concat(dictIn2)
                           .ToDictionary(x => x.Key, x => x.Value);
                HttpPostRequestDictionary(path, dictInAll);
            }

            public enum HttpPostRequestReturnCode
            {
                CONNECTION_ERROR, COMPLETED
            }

            public static HttpPostRequestReturnCode HttpPostRequestDictionary(string path, Dictionary<string, string> dictIn, int timeoutIn = 5000)
            {
                try
                {
                    using (var client = new WebClientWithAwesomeTimeouts { Timeout = System.TimeSpan.FromMilliseconds(timeoutIn) })
                    {
                        var nvTemp = dictIn.Aggregate(new NameValueCollection(),
                                                         (seed, current) =>
                                                         {
                                                             seed.Add(current.Key, current.Value);
                                                             return seed;
                                                         });

                        var response = client.UploadValues(path, nvTemp);

                        return HttpPostRequestReturnCode.COMPLETED;
                    }
                }
                catch
                {
                    return HttpPostRequestReturnCode.CONNECTION_ERROR;
                }
            }

            public static HttpPostRequestReturnCode HttpPostRequestFileUpload(string path, string filepath, int timeoutIn = 100000)
            {
                try
                {
                    using (var client = new WebClientWithAwesomeTimeouts { Timeout = System.TimeSpan.FromMilliseconds(timeoutIn) })
                    {

                        long length = new System.IO.FileInfo(filepath).Length;

                        client.Headers.Add("Content-Disposition", "form-data");
                        client.Headers.Add("name", "\"file\"");
                        client.Headers.Add("filename", "\"" + Path.GetFileName(filepath) + "\"");
                        client.Headers.Add("Content-Type", "application/octet-stream");


                        var response = client.UploadFile(path, "POST", filepath);

                        //var statusDescription = "ddd";
                        //var statusCode = GetStatusCode(client, out statusDescription); 


                        if (System.Text.ASCIIEncoding.ASCII.GetString(response).Contains("FileUploaded"))
                        {
                            return HttpPostRequestReturnCode.COMPLETED;
                        }
                        else
                        {
                            return HttpPostRequestReturnCode.CONNECTION_ERROR;
                        }

                    }
                }
                catch (Exception e)
                {
                    Globals.log.Error(e.ToString());
                    return HttpPostRequestReturnCode.CONNECTION_ERROR;
                }

            }

            public static string HtmlWithAbsolutePaths(string tempPathString, string html)
            {
                //~ Select root path
                //string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string appDirectory = Globals.PathLocalAppData;

                appDirectory = appDirectory.Replace("\\", "/");

                html = html.Replace(tempPathString, "file:///" + appDirectory);
                return html;
            }

            public static string GetTemplatePath(string pathIn)
            {
                string pathOut = Path.Combine(Globals.PathLocalAppData, "Template", pathIn);
                return pathOut;
            }



            public static Image base64image(string base64string)
            {
                //data:image/gif;base64,
                //this image is a single pixel (black)
                byte[] bytes = Convert.FromBase64String(base64string);

                Image image;
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    image = Image.FromStream(ms);
                }

                return image;
            }

            public static Dictionary<string, string> Json_toDictionary(string json)
            {
                //Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }

            public static string Json_toJson(object objIn)
            {
                return JsonConvert.SerializeObject(objIn);
            }

            public static string RandomString(int length)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                return new string(Enumerable.Repeat(chars, length)
                  .Select(s => s[random.Next(s.Length)]).ToArray());
            }


            #region Color

            public static Color DarkerColor(Color color, float correctionfactory = 50f)
            {
                const float hundredpercent = 100f;
                return Color.FromArgb((int)(((float)color.R / hundredpercent) * correctionfactory),
                    (int)(((float)color.G / hundredpercent) * correctionfactory), (int)(((float)color.B / hundredpercent) * correctionfactory));
            }

            public static String Color2HexConverter(System.Drawing.Color c)
            {
                return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
            }

            public static String Color2RGBConverter(System.Drawing.Color c)
            {
                return "RGB(" + c.R.ToString() + "," + c.G.ToString() + "," + c.B.ToString() + ")";
            }

            private static int PerceivedBrightness(Color c)
            {
                return (int)Math.Sqrt(
                c.R * c.R * .299 +
                c.G * c.G * .587 +
                c.B * c.B * .114);
            }

            public static Color CalculateForegroundColor(Color backcolor)
            {
                return (PerceivedBrightness(backcolor) > 130 ? Color.Black : Color.White);
            }

            public static void openSuperNotification(string jsonMsg)
            {
                try
                {
                    var htmlString = Globals.htmlStringTemplateSuper;

#if DEBUG
                    htmlString = File.ReadAllText(@"\\VBOXSVR\SharedLocal\AMIVOICE\projects\AmivoiceWatcher\amivoice_watcher\AmiVoiceWatcher2\AmivoiceWatcherDotNet\AmivoiceWatcherDotNet\_Src\Template\SuperNotificationTemplate.html");
#endif

                    htmlString = Globals.functions.HtmlWithAbsolutePaths("./ReplaceWithAbsolutePath/", htmlString);

                    //htmlString = htmlString.Replace("[[messageJson]]", Globals.Json_toJson(jsonMsg));
                    htmlString = htmlString.Replace("[[messageJson]]", jsonMsg);

                    //MessageBox.Show(jsonMsg, "555");




                    FormSuperNotification frm = new FormSuperNotification();


                    frm.webBrowser1.DocumentText = htmlString;


                    frm.Show();
                }
                catch
                {

                }

            }

            public static void openLinkInDefaultBrowser(string url)
            {
                try
                {
                    Process.Start(url);
                }
                catch
                {

                }

            }

            #endregion


        }

        public class Notifications
        {
            public static void PopupWelcomeMessage()
            {
                try
                {
                    Globals.log.Debug("PopupWelcomeMessage() start");

                    Dictionary<string, string> dictMsg = new Dictionary<string, string>() {
                        {"id", "1234567"},
                        {"title", "Hello " + Globals.myComputerInfo.UserName + " !"},
                        {"body", "สู้ๆ <br />頑張ってください。<br />Please try your best :D"},
                        {"level", "notice"},
                        {"timestamp", "2016-02-03 12:34"},
                        {"icon", "logo.png" }
                        //{"icon", "data:image/png;base64,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/GMzLdsAAETdaVRYdFhNTDpjb20uYWRvYmUueG1wAAAAAAA8P3hwYWNrZXQgYmVnaW49Iu+7vyIgaWQ9Ilc1TTBNcENlaGlIenJlU3pOVGN6a2M5ZCI/Pgo8eDp4bXBtZXRhIHhtbG5zOng9ImFkb2JlOm5zOm1ldGEvIiB4OnhtcHRrPSJBZG9iZSBYTVAgQ29yZSA1LjUtYzAyMSA3OS4xNTU3NzIsIDIwMTQvMDEvMTMtMTk6NDQ6MDAgICAgICAgICI+CiAgIDxyZGY6UkRGIHhtbG5zOnJkZj0iaHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+CiAgICAgIDxyZGY6RGVzY3JpcHRpb24gcmRmOmFib3V0PSIiCiAgICAgICAgICAgIHhtbG5zOnhtcD0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLyIKICAgICAgICAgICAgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iCiAgICAgICAgICAgIHhtbG5zOnN0RXZ0PSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VFdmVudCMiCiAgICAgICAgICAgIHhtbG5zOnBob3Rvc2hvcD0iaHR0cDovL25zLmFkb2JlLmNvbS9waG90b3Nob3AvMS4wLyIKICAgICAgICAgICAgeG1sbnM6ZGM9Imh0dHA6Ly9wdXJsLm9yZy9kYy9lbGVtZW50cy8xLjEvIgogICAgICAgICAgICB4bWxuczp0aWZmPSJodHRwOi8vbnMuYWRvYmUuY29tL3RpZmYvMS4wLyIKICAgICAgICAgICAgeG1sbnM6ZXhpZj0iaHR0cDovL25zLmFkb2JlLmNvbS9leGlmLzEuMC8iPgogICAgICAgICA8eG1wOkNyZWF0b3JUb29sPkFkb2JlIFBob3Rvc2hvcCBDQyAyMDE0IChXaW5kb3dzKTwveG1wOkNyZWF0b3JUb29sPgogICAgICAgICA8eG1wOkNyZWF0ZURhdGU+MjAxNS0wMy0yOFQwMjowNzoxNCswNTozMDwveG1wOkNyZWF0ZURhdGU+CiAgICAgICAgIDx4bXA6TWV0YWRhdGFEYXRlPjIwMTUtMDMtMjhUMDI6MDc6MTQrMDU6MzA8L3htcDpNZXRhZGF0YURhdGU+CiAgICAgICAgIDx4bXA6TW9kaWZ5RGF0ZT4yMDE1LTAzLTI4VDAyOjA3OjE0KzA1OjMwPC94bXA6TW9kaWZ5RGF0ZT4KICAgICAgICAgPHhtcE1NOkluc3RhbmNlSUQ+eG1wLmlpZDpiY2Y1YTk2OS1hYmM0LWVhNGItYTFjNi0zMzliNDI1MmQzYmM8L3htcE1NOkluc3RhbmNlSUQ+CiAgICAgICAgIDx4bXBNTTpEb2N1bWVudElEPmFkb2JlOmRvY2lkOnBob3Rvc2hvcDowMzRhN2Q0Yy1kNGMxLTExZTQtODFmOC1mMTM4NzY4NmE5MjE8L3htcE1NOkRvY3VtZW50SUQ+CiAgICAgICAgIDx4bXBNTTpPcmlnaW5hbERvY3VtZW50SUQ+eG1wLmRpZDplMGVkNzViYS1kNGQ5LTFlNDctYTYxYi1kZGNmOWUxMjQ4MTc8L3htcE1NOk9yaWdpbmFsRG9jdW1lbnRJRD4KICAgICAgICAgPHhtcE1NOkhpc3Rvcnk+CiAgICAgICAgICAgIDxyZGY6U2VxPgogICAgICAgICAgICAgICA8cmRmOmxpIHJkZjpwYXJzZVR5cGU9IlJlc291cmNlIj4KICAgICAgICAgICAgICAgICAgPHN0RXZ0OmFjdGlvbj5jcmVhdGVkPC9zdEV2dDphY3Rpb24+CiAgICAgICAgICAgICAgICAgIDxzdEV2dDppbnN0YW5jZUlEPnhtcC5paWQ6ZTBlZDc1YmEtZDRkOS0xZTQ3LWE2MWItZGRjZjllMTI0ODE3PC9zdEV2dDppbnN0YW5jZUlEPgogICAgICAgICAgICAgICAgICA8c3RFdnQ6d2hlbj4yMDE1LTAzLTI4VDAyOjA3OjE0KzA1OjMwPC9zdEV2dDp3aGVuPgogICAgICAgICAgICAgICAgICA8c3RFdnQ6c29mdHdhcmVBZ2VudD5BZG9iZSBQaG90b3Nob3AgQ0MgMjAxNCAoV2luZG93cyk8L3N0RXZ0OnNvZnR3YXJlQWdlbnQ+CiAgICAgICAgICAgICAgIDwvcmRmOmxpPgogICAgICAgICAgICAgICA8cmRmOmxpIHJkZjpwYXJzZVR5cGU9IlJlc291cmNlIj4KICAgICAgICAgICAgICAgICAgPHN0RXZ0OmFjdGlvbj5zYXZlZDwvc3RFdnQ6YWN0aW9uPgogICAgICAgICAgICAgICAgICA8c3RFdnQ6aW5zdGFuY2VJRD54bXAuaWlkOmJjZjVhOTY5LWFiYzQtZWE0Yi1hMWM2LTMzOWI0MjUyZDNiYzwvc3RFdnQ6aW5zdGFuY2VJRD4KICAgICAgICAgICAgICAgICAgPHN0RXZ0OndoZW4+MjAxNS0wMy0yOFQwMjowNzoxNCswNTozMDwvc3RFdnQ6d2hlbj4KICAgICAgICAgICAgICAgICAgPHN0RXZ0OnNvZnR3YXJlQWdlbnQ+QWRvYmUgUGhvdG9zaG9wIENDIDIwMTQgKFdpbmRvd3MpPC9zdEV2dDpzb2Z0d2FyZUFnZW50PgogICAgICAgICAgICAgICAgICA8c3RFdnQ6Y2hhbmdlZD4vPC9zdEV2dDpjaGFuZ2VkPgogICAgICAgICAgICAgICA8L3JkZjpsaT4KICAgICAgICAgICAgPC9yZGY6U2VxPgogICAgICAgICA8L3htcE1NOkhpc3Rvcnk+CiAgICAgICAgIDxwaG90b3Nob3A6RG9jdW1lbnRBbmNlc3RvcnM+CiAgICAgICAgICAgIDxyZGY6QmFnPgogICAgICAgICAgICAgICA8cmRmOmxpPjAyQjFCNDdCOTFGODAxMjUzNTVGMUY0RUQxODhENEMzPC9yZGY6bGk+CiAgICAgICAgICAgICAgIDxyZGY6bGk+MUE3RTczMTNCMzE1RUExMTM4RjFGQjc5MTQ3NDk0MkI8L3JkZjpsaT4KICAgICAgICAgICAgICAgPHJkZjpsaT40Mzc0Q0EyQkNFOUQwQkEzREY3MzcwN0Y0OTFFNjI0NDwvcmRmOmxpPgogICAgICAgICAgICAgICA8cmRmOmxpPjQzRDE5RTk0M0NDMjk5MTlGRkI1MTY3NTU1OUFFQjJBPC9yZGY6bGk+CiAgICAgICAgICAgICAgIDxyZGY6bGk+NDcxMTI0Q0IxQkZEQjA1NjlGMjkzRkJCQzkwMkQ0Njg8L3JkZjpsaT4KICAgICAgICAgICAgICAgPHJkZjpsaT41QUU4MDZBMEEzQTkwRDg3RUE4RDlERDM1NDJDMEJDNzwvcmRmOmxpPgogICAgICAgICAgICAgICA8cmRmOmxpPjVCOTFFQ0IzOEU4RTlGMUU5MEEyNjRBRjBEOEU3NTAyPC9yZGY6bGk+CiAgICAgICAgICAgICAgIDxyZGY6bGk+QTE1MEYxN0MwMThCQzg3MjAwNDJGMzdEREMyMkY1Qzc8L3JkZjpsaT4KICAgICAgICAgICAgICAgPHJkZjpsaT5BRTIwREM4QkI0NDQzMzg5NDhGQ0Q1MEQ4RkU4MUMwMjwvcmRmOmxpPgogICAgICAgICAgICAgICA8cmRmOmxpPkNFQzREMTk4NzQ1N0NCQUU4MTExN0M1REJBOTcyNUY5PC9yZGY6bGk+CiAgICAgICAgICAgICAgIDxyZGY6bGk+REUxN0Q0NTZCNjY4QjQ3REU3N0MxQTk2QTczMDlEQjk8L3JkZjpsaT4KICAgICAgICAgICAgICAgPHJkZjpsaT5ERUE4NTNFOUZFMjg4NThFQjczQjJCNkVDQ0ExNzUxQjwvcmRmOmxpPgogICAgICAgICAgICAgICA8cmRmOmxpPkVBMTYwOEE2QTE0MkU0M0QzMDlENTA3NzAzNzQ4RTZEPC9yZGY6bGk+CiAgICAgICAgICAgICAgIDxyZGY6bGk+RUU4OEFFNDlBQjhGQzdBNzk0MTJCQjYxQ0NEM0I3RTE8L3JkZjpsaT4KICAgICAgICAgICAgICAgPHJkZjpsaT5GOTQ4NkY3MzA4Rjc1OEZCMzExMzk3MUU1QUVEOTMwRDwvcmRmOmxpPgogICAgICAgICAgICAgICA8cmRmOmxpPmFkb2JlOmRvY2lkOnBob3Rvc2hvcDoxMDMxZGU4ZC02ZjA0LTExNzctYjhhNi04MDhlOGY1ZjJiY2Q8L3JkZjpsaT4KICAgICAgICAgICAgICAgPHJkZjpsaT54bXAuZGlkOjAxODAxMTc0MDcyMDY4MTE4MjJBQjEwMTkyOUU1NERDPC9yZGY6bGk+CiAgICAgICAgICAgICAgIDxyZGY6bGk+eG1wLmRpZDowMTgwMTE3NDA3MjA2ODExOTEwOUZFNEE5ODk1RUQ3NjwvcmRmOmxpPgogICAgICAgICAgICAgICA8cmRmOmxpPnhtcC5kaWQ6MDc4MDExNzQwNzIwNjgxMTgwODNBNkM1QjA1NUE5RkE8L3JkZjpsaT4KICAgICAgICAgICAgICAgPHJkZjpsaT54bXAuZGlkOjA3ODAxMTc0MDcyMDY4MTE4MDgzQjlCNjkxNjczQUE3PC9yZGY6bGk+CiAgICAgICAgICAgICAgIDxyZGY6bGk+eG1wLmRpZDowNzgwMTE3NDA3MjA2ODExOTk0Q0I5ODlDRTQ1NDJFRDwvcmRmOmxpPgogICAgICAgICAgICAgICA8cmRmOmxpPnhtcC5kaWQ6Mjk5NjEyMkYxMjIwNjgxMThEQjg4MzBEOTI4QUU3MEU8L3JkZjpsaT4KICAgICAgICAgICAgICAgPHJkZjpsaT54bXAuZGlkOjQ4RDczMDQ3NzU2QzExRTNBRTUzQkVDMURFNjlBQjlFPC9yZGY6bGk+CiAgICAgICAgICAgICAgIDxyZGY6bGk+eG1wLmRpZDo0QkZFNTkwODNDM0ZFNDExQTRFQ0Q4NDMxMEJEODAxMDwvcmRmOmxpPgogICAgICAgICAgICAgICA8cmRmOmxpPnhtcC5kaWQ6NERGRTU5MDgzQzNGRTQxMUE0RUNEODQzMTBCRDgwMTA8L3JkZjpsaT4KICAgICAgICAgICAgICAgPHJkZjpsaT54bXAuZGlkOjZBNTA3QTZCNzU2QzExRTNBRTUzQkVDMURFNjlBQjlFPC9yZGY6bGk+CiAgICAgICAgICAgICAgIDxyZGY6bGk+eG1wLmRpZDo2RjdBRTVFMUFCNDQxMUUzOTNFQkZENTQ2M0IxQTRGODwvcmRmOmxpPgogICAgICAgICAgICAgICA8cmRmOmxpPnhtcC5kaWQ6NzkyNDFFOEQwRDIwNjgxMUFFNTY4QzA0MzVGOTU1RjY8L3JkZjpsaT4KICAgICAgICAgICAgICAgPHJkZjpsaT54bXAuZGlkOjg3ODM0MTc1RkUyMDY4MTE4MjJBRDQyMDBFNUM4OTY1PC9yZGY6bGk+CiAgICAgICAgICAgICAgIDxyZGY6bGk+eG1wLmRpZDpBRDAzMDc1NDFCQjkxMUUzOTdDOUFEOEJGOTYwRTQzNjwvcmRmOmxpPgogICAgICAgICAgICAgICA8cmRmOmxpPnhtcC5kaWQ6QUQ5RThDRjlFMTlEMTFFMkEyRjNFMzQ4NDI1NzQ1NDg8L3JkZjpsaT4KICAgICAgICAgICAgICAgPHJkZjpsaT54bXAuZGlkOkQyODJFMEIyNkJCQ0U0MTFCNzIzOTk1Q0IwMzM1OUZGPC9yZGY6bGk+CiAgICAgICAgICAgICAgIDxyZGY6bGk+eG1wLmRpZDpEM0RBMTZCOEM2MDMxMUUzODEwRjhFODU2OTQ3QzRBRjwvcmRmOmxpPgogICAgICAgICAgICAgICA8cmRmOmxpPnhtcC5kaWQ6RDVDRjc1QzQzNEZEMTFFM0I0MzdDMEQ4RjBFMjkzRTQ8L3JkZjpsaT4KICAgICAgICAgICAgICAgPHJkZjpsaT54bXAuZGlkOkVDRUJGNTc1RTA3MTExRTE5ODlCRkVGNkYyM0FGNDZCPC9yZGY6bGk+CiAgICAgICAgICAgICAgIDxyZGY6bGk+eG1wLmRpZDpiN2RiOWUzMi05ZTQzLTQ2NmYtYmNiNi00Y2Y2OGQxZWVlNGQ8L3JkZjpsaT4KICAgICAgICAgICAgICAgPHJkZjpsaT54bXAuZGlkOmNjMzA1MTk0LTBlMDQtNDMyMS05YTNmLWU0YmZlYjFiNWE4OTwvcmRmOmxpPgogICAgICAgICAgICA8L3JkZjpCYWc+CiAgICAgICAgIDwvcGhvdG9zaG9wOkRvY3VtZW50QW5jZXN0b3JzPgogICAgICAgICA8cGhvdG9zaG9wOkNvbG9yTW9kZT4zPC9waG90b3Nob3A6Q29sb3JNb2RlPgogICAgICAgICA8cGhvdG9zaG9wOklDQ1Byb2ZpbGU+c1JHQiBJRUM2MTk2Ni0yLjE8L3Bob3Rvc2hvcDpJQ0NQcm9maWxlPgogICAgICAgICA8ZGM6Zm9ybWF0PmltYWdlL3BuZzwvZGM6Zm9ybWF0PgogICAgICAgICA8dGlmZjpPcmllbnRhdGlvbj4xPC90aWZmOk9yaWVudGF0aW9uPgogICAgICAgICA8dGlmZjpYUmVzb2x1dGlvbj43MjAwMDAvMTAwMDA8L3RpZmY6WFJlc29sdXRpb24+CiAgICAgICAgIDx0aWZmOllSZXNvbHV0aW9uPjcyMDAwMC8xMDAwMDwvdGlmZjpZUmVzb2x1dGlvbj4KICAgICAgICAgPHRpZmY6UmVzb2x1dGlvblVuaXQ+MjwvdGlmZjpSZXNvbHV0aW9uVW5pdD4KICAgICAgICAgPGV4aWY6Q29sb3JTcGFjZT4xPC9leGlmOkNvbG9yU3BhY2U+CiAgICAgICAgIDxleGlmOlBpeGVsWERpbWVuc2lvbj4xNDI8L2V4aWY6UGl4ZWxYRGltZW5zaW9uPgogICAgICAgICA8ZXhpZjpQaXhlbFlEaW1lbnNpb24+MTIwPC9leGlmOlBpeGVsWURpbWVuc2lvbj4KICAgICAgPC9yZGY6RGVzY3JpcHRpb24+CiAgIDwvcmRmOlJERj4KPC94OnhtcG1ldGE+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgCjw/eHBhY2tldCBlbmQ9InciPz5dP34gAAAAIGNIUk0AAHolAACAgwAA+f8AAIDpAAB1MAAA6mAAADqYAAAXb5JfxUYAAEgfSURBVHja7L13vGVVef//XmvvfertZeaW6ZUZmHFkqKIgHfwq2KOIooI1NgSCwCBDUZNoJCa/+ErEGBOxJpIvQvSrRkJzgCmUYQaml1vm9nbqbmut3x97n3vPnRkIFobirNdr33vuPvvu+tnP83nqEsYYhBAcHf/rWAasBu78Xf7pr58ovKDtGhMCbWDC12gEoYaMLVDG4GtDWUFBGfIKisrgGcNICMpE/28QWEfoRjx4Th02gLCzR2HxPMOoMhj9WeBdwP1Az4t8SAGsAk4FVgAzgRDoBZ4ENgDPvJT3xD4Kixc0TgbeD2SAL2AlPiXsmufc+NJf7f1DjnV6e1peMyMtz8pYIiMFeMpQCAzjvmbANfS4utjjm/8E/iYG0lHgvAxHBmnfGIMGjLksVlePvgjH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fnT5mWeeecaCBQsSe/fu3XrWWWd975JLLjl3wYIFmYGBAX9gYMBfvHhx+tJLL73w5JNP/tbo6Gjz0qVLG5csWdI0PDxcs3Llyq9cdtllb1m6dGlmaGjIHxwc9BcvXpy+5JJLzjruuOO++fjjj4vly5c3Llu2LJ1MJm3bti3HcSzHcaxSqVQHSCGEGBwczEgpU4Bsa2tLHXfccZlZs2alpJSWbdu267qzjzvuuIZ58+bNeOihh6QQQmzdulV0dna2r1y5Muv7/ixAvv71r69fuXLlm5YtW5ZatmxZetWqVVd0dHSkhBBiYmKiaenSpU3HHHNMY6lUmll5SYUQYtWqVZ/4yEc+cuVJJ53UmM/ndU9Pj9fS0uJcfPHFx51yyik/XLVqVf0b3vCGH7/5zW9eVltba/f09LgTExPq5JNPbrr88suvXbly5YfE4S0aW0q5YNGiRYmGhob5xN0SATE6Opp1HMeRUlqzZs1KHXfccZmZM2cmLcuSQEoIsWTZsmWpBQsWpFOplJ1KpazOzs7keeedN//888//zic+8Yn5h3UAvutd7+o444wzzh0dHTVbtmzJn3766XVLly69GnhgdHTU9n1fB0GAlJK2traTV61a1dre3t4WBAG2beP7vigWi47v+yYIAjM6Omo2btz4y3e/+90rFi1atAhILVq06PL29nZr06ZNpb/8y7+8d926dTcAfPWrX733Bz/4wXcALrnkkstuuummi5ctW3bWunXr/sPzPKOUMrlcrrapqekEgDvuuGPj3//93/+lUsp87GMf+/SHP/zhM5qbm1fs3r3b8X2f/v5+ffPNN389l8v127ZtAxNbtmz5ZeXh3XvvvQO7du16TxiG7Zdffvk1H//4x19799137/r6179+k23bYwsXLpxdUcMXXnjhmZdccklvPp+fL4SQQRDguq4FWBdccMHHVq5cWf/ss8/6juPI448/vvWDH/zgO7/85S9/P5/PO77vo5QyhUIhUSXd7cbGxtMdx+Hee+8duOGGG64slUrFc88997xbbrnlky0tLTNe+9rXvn7evHmt+Xze3HjjjX933333/U8qlUrfeuutX3vrW986q6Oj453Adzi0EaL0fV8A+L5vqp/vXXfd9eyTTz75Vtd1m6666qpb3ve+9y26884719955523l0qlwfe///2fBPjNb34zeOONN35eCGHm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" }
                    };

                    var duration = Globals.notification_duration;
                    //var animationMethod = FormAnimator.AnimationMethod.Slide;
                    var animationMethod = FormAnimator.AnimationMethod.Fade;
                    var animationDirection = FormAnimator.AnimationDirection.Up;

                    var jsonMsg = Globals.functions.Json_toJson(dictMsg);
                    AmivoiceWatcher.Notifications.Popup(jsonMsg, duration, animationMethod, animationDirection);

                    Globals.log.Debug("PopupWelcomeMessage() finished");
                }
                catch (Exception e)
                {
                    Globals.log.Error(e.ToString());
                }


            }
        }


    }

    public class Base64ImageFile
    {

        public string error = Path.Combine(Globals.PathLocalAppData, "Template", "image", "Button-Close-icon.png");
        public string notice = Path.Combine(Globals.PathLocalAppData, "Template", "image", "Button-Info-icon.png");
        public string success = Path.Combine(Globals.PathLocalAppData, "Template", "image", "ok-icon.png");
        public string warning = Path.Combine(Globals.PathLocalAppData, "Template", "image", "Button-Warning-icon.png");
        public string question = Path.Combine(Globals.PathLocalAppData, "Template", "image", "Help-icon.png");

    }



    public class FileParameter
    {
        public byte[] File { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public FileParameter(byte[] file) : this(file, null) { }
        public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
        public FileParameter(byte[] file, string filename, string contenttype)
        {
            File = file;
            FileName = filename;
            ContentType = contenttype;
        }
    }

    
}
