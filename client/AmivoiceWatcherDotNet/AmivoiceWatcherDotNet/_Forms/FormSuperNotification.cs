using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Permissions;


namespace AmivoiceWatcher
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public partial class FormSuperNotification : Form
    {
        public FormSuperNotification()
        {
            InitializeComponent();
        }

        public void openLinkInDefaultBrowser(string url)
        {
            Globals.functions.openLinkInDefaultBrowser(url);

        }

        public void openSuperNotification(string url)
        {
            Globals.functions.openSuperNotification(url);

        }

        private void FormSuperNotification_Load(object sender, EventArgs e)
        {
          
            webBrowser1.ObjectForScripting = this;



            //webBrowser1.IsWebBrowserContextMenuEnabled = true;
            //webBrowser1.WebBrowserShortcutsEnabled = true;
            //webBrowser1.ObjectForScripting = true;
            //webBrowser1.ScriptErrorsSuppressed = true;
        }
    }


}
