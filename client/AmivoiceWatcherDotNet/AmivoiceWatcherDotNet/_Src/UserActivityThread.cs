using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using System.Timers;

namespace AmivoiceWatcher
{
    class UserActivityThread
    {
        public static bool blnThreadAborted = false;

        public static List<string> notSubmitTitle;

        private EventHook.WinEventDelegate procDelegate = new EventHook.WinEventDelegate(WinEventProc);
        //private List<UserActivity> userActivities = new List<UserActivity>();
        private static List<UserActivity> userActivities;
        private static System.Timers.Timer aTimer;


        // improve speed for SubmitActivitiesLog()
        private static string _agentactivity_url = Globals.configuration["agentactivity.url"];
        private static string _path_unsent_activities = Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), @"AmivoiceWatcher\unsent_activities.txt");
        private static string _str_buff_heading;

        private static string _strLastTitleNotIdle = "";
        
        // Uncommment if use winhook
        //private IntPtr hhook;
        //private static int intIdleTimeCounter =0;


        public void stop()
        {
            // Uncommment if use winhook
            //EventHook.UnhookWinEvent(hhook);
            SubmitRemainingActivitiesLog();

        }

        private void startMonitor()
        {
            Initialize_userActivities();
        }

        private static void Initialize_userActivities()
        {
            _str_buff_heading = "#LoginName=";
            _str_buff_heading += Globals.myComputerInfo.UserName;
            _str_buff_heading += "\n";
            _str_buff_heading += "#MacAddress=";
            _str_buff_heading += Globals.myComputerInfo.MacAddress;
            _str_buff_heading += "\n\n";
            _str_buff_heading += "__AGENT_ACTIVITY__\n";

            userActivities = new List<UserActivity>();

            //~ Comment out if required
            var userActivityTemp = new UserActivity();
            userActivityTemp.StartTime = DateTime.Now;
            userActivityTemp.WinTitle = "__START__";
            userActivityTemp.ProcName = "__WATCHER__";
            userActivityTemp.ProcExe = "__WATCHER__";

            userActivities.Add(userActivityTemp);


            notSubmitTitle = new List<string>();

            notSubmitTitle.Add("__START__");
        }

        public static void ThreadMain()
        {
            Initialize_userActivities();
            SetTimer();

            while (! blnThreadAborted)
            {
                //Remove this if use winhook
                MonitorWindowActive();

                Thread.Sleep(200);
            }
        }

        static void WinEventProc(IntPtr hWinEventHook, uint eventType,
                                   IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {

            if (userActivities.Count > 0)
            {
                userActivities.Last().Duration = Convert.ToInt32(DateTime.Now.Subtract(userActivities.Last().StartTime).TotalSeconds);
            }

            var userActivityTemp = new UserActivity();
            userActivityTemp.StartTime = DateTime.Now;

            userActivityTemp.WinTitle = EventHook.GetActiveWindowTitle();

            var arrTemp = EventHook.GetActiveWindowProcessNameExe();
            userActivityTemp.ProcName = arrTemp[0];
            userActivityTemp.ProcExe = arrTemp[1];

            if (String.IsNullOrEmpty(userActivityTemp.WinTitle+ userActivityTemp.ProcName + userActivityTemp.ProcExe))
            {
                return;
            }

            if (EventHook.IsOnDesktop())
            {
                if (userActivityTemp.WinTitle == userActivities.Last().WinTitle)
                {
                    userActivityTemp.WinTitle = "IDLE_TIME";
                    userActivityTemp.ProcName = "IDLE_TIME";
                    userActivityTemp.ProcExe = "IDLE_TIME";
                }
                
            }
            else
            {
                if (userActivityTemp.WinTitle == userActivities.Last().WinTitle &&
                    userActivityTemp.ProcName == userActivities.Last().ProcName &&
                    userActivityTemp.ProcExe == userActivities.Last().ProcExe)
                {
                    return;
                }
                else if (String.IsNullOrEmpty(userActivityTemp.WinTitle) &&
                    userActivityTemp.ProcName == userActivities.Last().ProcName &&
                    userActivityTemp.ProcExe == userActivities.Last().ProcExe)
                {
                    return;
                }
            }

            userActivities.Add(userActivityTemp);

            if (userActivityTemp.WinTitle != "IDLE_TIME")
            {
                _strLastTitleNotIdle = userActivityTemp.WinTitle;
            }
            
            Globals.log.Debug(String.Format("\nForeground changed to {0}\nProcName: {1}\nProcExe: {2}", userActivityTemp.WinTitle, userActivityTemp.ProcName, userActivityTemp.ProcExe));
        }



        static void MonitorWindowActive()
        {
            try
            {
                if (userActivities.Count > 0)
                {
                    userActivities.Last().Duration = (int)DateTime.Now.Subtract(userActivities.Last().StartTime).TotalSeconds;
                }

                var userActivityTemp = new UserActivity();
                userActivityTemp.StartTime = DateTime.Now;

                userActivityTemp.WinTitle = EventHook.GetActiveWindowTitle();

                var arrTemp = EventHook.GetActiveWindowProcessNameExe();
                userActivityTemp.ProcName = arrTemp[0];
                userActivityTemp.ProcExe = arrTemp[1];


                if (String.IsNullOrEmpty(userActivityTemp.WinTitle + userActivityTemp.ProcName + userActivityTemp.ProcExe))
                {
                    userActivityTemp.WinTitle = "IDLE_TIME";
                    userActivityTemp.ProcName = "IDLE_TIME";
                    userActivityTemp.ProcExe = "IDLE_TIME";
                }
                else if ((userActivityTemp.WinTitle == userActivities.Last().WinTitle) || String.IsNullOrEmpty(userActivityTemp.WinTitle))
                {

                    if (String.IsNullOrEmpty(userActivityTemp.ProcExe))
                    {
                        userActivityTemp.WinTitle = "IDLE_TIME";
                        userActivityTemp.ProcName = "IDLE_TIME";
                        userActivityTemp.ProcExe = "IDLE_TIME";
                    }
                    else
                    {
                        return;
                    }

                }

                //Fix Program Manager case :: i.e. after right click on notification area
                if (userActivityTemp.WinTitle == "Program Manager" &&
                        userActivityTemp.ProcName == "Window Explorer")
                {
                    userActivityTemp.WinTitle = userActivities.Last().WinTitle;
                    userActivityTemp.ProcName = userActivities.Last().ProcName;
                    userActivityTemp.ProcExe = userActivities.Last().ProcExe;
                }

                // ==== NOT RECORD CASE
                if (String.IsNullOrEmpty(userActivityTemp.WinTitle) &&
                    userActivityTemp.ProcName == userActivities.Last().ProcName &&
                    userActivityTemp.ProcExe == userActivities.Last().ProcExe)
                {
                    return;
                }
                // ==== Check Last one is same as current one
                else if (userActivityTemp.WinTitle == userActivities.Last().WinTitle &&
                    userActivityTemp.ProcName == userActivities.Last().ProcName &&
                        userActivityTemp.ProcExe == userActivities.Last().ProcExe)
                {
                    return;
                    
                }
                else
                {
                    //Globals.log.Debug(userActivityTemp.WinTitle);
                    //Globals.log.Debug(_strLastTitleNotIdle);
                    //Globals.log.Debug(Regex.Replace(userActivityTemp.WinTitle, @"[\d*]+", string.Empty));
                    //Globals.log.Debug(Regex.Replace(_strLastTitleNotIdle, @"[\d*]+", string.Empty));
                    //Globals.log.Debug(Regex.Replace(userActivityTemp.WinTitle, @"[\d*]+", string.Empty) == Regex.Replace(_strLastTitleNotIdle, @"[\d*]+", string.Empty));

                    if (Regex.Replace(userActivityTemp.WinTitle, @"[\d*]+", string.Empty) == Regex.Replace(_strLastTitleNotIdle, @"[\d*]+", string.Empty))
                    {
                        return;
                    }
                }

                // ==== check if wintitle contain "???" case
                if (userActivityTemp.WinTitle.Contains("????"))
                {
                    string[] strArr = Regex.Split(userActivityTemp.WinTitle, @"\?{2,}");
                    bool boolMatchAll = true;
                    foreach (string strTemp in strArr)
                    {
                        if (!userActivities.Last().WinTitle.Contains(strTemp))
                        {
                            boolMatchAll = false;
                            break;
                        }
                    }
                    if (boolMatchAll)
                    {
                        return;
                    }   
                }

                userActivities.Add(userActivityTemp);

                if (userActivityTemp.WinTitle != "IDLE_TIME")
                {
                    _strLastTitleNotIdle = userActivityTemp.WinTitle;
                }

                Globals.log.Debug(String.Format("\nForeground changed to {0}\nProcName: {1}\nProcExe: {2}", userActivityTemp.WinTitle, userActivityTemp.ProcName, userActivityTemp.ProcExe));

            }
            catch(Exception e)
            {
                Globals.log.Warn(e.ToString());
            }

        }

        private static void SetTimer()
        {
            double dblTimer;

            if (Double.TryParse(Globals.configuration["agentactivity.senddata.timer.sec"], out dblTimer))
                aTimer = new System.Timers.Timer(dblTimer*1000);
            else
                aTimer = new System.Timers.Timer(20000);

            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEventSubmitActivityLog;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private static void OnTimedEventSubmitActivityLog(Object source, ElapsedEventArgs e)
        {
            SubmitActivitiesLog();
        }


        private static void SubmitRemainingActivitiesLog()
        {
            if (userActivities.Count() > 0)
            {
                userActivities.Last().Duration = (int)DateTime.Now.Subtract(userActivities.Last().StartTime).TotalSeconds;


                var userActivityTemp = new UserActivity();
                userActivityTemp.StartTime = DateTime.Now;
                userActivityTemp.Duration = 0;

                userActivityTemp.WinTitle = "__STOP__";
                userActivityTemp.ProcName = "__WATCHER__";
                userActivityTemp.ProcExe = "__WATCHER__";

                userActivities.Add(userActivityTemp);
                // DUMMY because the last one will not be submitted
                userActivities.Add(userActivityTemp);

                SubmitActivitiesLog();
            }

        }


        private static void SubmitActivitiesLog()
        {

            if (userActivities.Count > 1)
            {
                userActivities.Reverse();
                var buf = _str_buff_heading;

                string strActivities = "";

                if (File.Exists(_path_unsent_activities))
                {
                    strActivities += File.ReadAllText(_path_unsent_activities);
                }


                for (int i = userActivities.Count - 1; i >= 1; i--)
                {
                    // FILTERING userActivities
                    bool boolOkToSent = false;
                    if (userActivities[i].WinTitle != "IDLE_TIME")
                    {

                        boolOkToSent = true;
                    }
                    else  //= userActivities[i].WinTitle == "IDLE_TIME"
                    {
                        if (userActivities[i].Duration >= 1)
                        {
                            boolOkToSent = true;
                        }
                    }
                        

                    if (boolOkToSent)
                    {
                        if (!notSubmitTitle.Contains(userActivities[i].WinTitle))
                        {
                            strActivities += userActivities[i];
                            strActivities += "\n";
                        }
                        
                    }

                    userActivities.RemoveAt(i);


                }

                buf += strActivities;

                var dictTemp4http = new Dictionary<string, string> {
                    { "result", buf }
                   };

                try
                {
                    var returnCode = Globals.functions.HttpPostRequestDictionary(_agentactivity_url, dictTemp4http);

                    if (returnCode == Globals.functions.HttpPostRequestReturnCode.CONNECTION_ERROR)
                    {
                        Globals.log.Warn("Http post fail");
                        File.WriteAllText(_path_unsent_activities, strActivities);

                    }
                    else if (returnCode == Globals.functions.HttpPostRequestReturnCode.COMPLETED)
                    {
                        File.Delete(_path_unsent_activities);
                        return;
                    }
                }
                catch(Exception e)
                {
                    Globals.log.Warn(String.Format("Http post fail: {0}", e.ToString()));
                }
            }
        }
    }
}
