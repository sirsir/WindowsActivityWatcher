using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmivoiceWatcher
{
    class UserActivity
    {

        private string procName;
        private string winTitle;
        private int duration;
        private DateTime startTime;

        private string procExe;

        public string ProcExe
        {
            get { return procExe; }
            set { procExe = value; }
        }


        public DateTime StartTime
        {
            get { return startTime; }
            set { startTime = value; }
        }


        public string ProcName
        {
            get { return procName; }
            set { procName = value; }
        }


        public string WinTitle
        {
            get { return winTitle; }
            set { winTitle = value; }
        }


        public int Duration
        {
            get { return duration; }
            set { duration = value; }
        }


        public Dictionary<string, string> ToDictionary()
        {
            Dictionary<string, string> dictTemp = new Dictionary<string, string>();

            dictTemp.Add("proc_name", procName);
            dictTemp.Add("proc_exe", procExe);
            dictTemp.Add("window_title", winTitle);
            dictTemp.Add("duration", duration.ToString());
            dictTemp.Add("start_time", startTime.ToString("yyyy-MM-dd HH:mm:ss"));

            return dictTemp;
        }


        public override string ToString()
        {
            string strTemp = "";
            string strDELIM = "\t";
            
            strTemp += startTime.ToString("yyyy-MM-dd HH:mm:ss") + strDELIM;
            strTemp += duration.ToString() + strDELIM;
            strTemp += procName + strDELIM;
            strTemp += winTitle + strDELIM;
            strTemp += procExe;

            return strTemp;
        }
    }
}
