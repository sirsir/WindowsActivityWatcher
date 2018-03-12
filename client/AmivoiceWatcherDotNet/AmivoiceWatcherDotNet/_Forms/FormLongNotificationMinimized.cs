using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AmivoiceWatcher
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public partial class FormLongNotificationMinimized : Form
    {

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();


        public FormLongNotificationMinimized()
        {
            InitializeComponent();

            //this.ClientSize = new System.Drawing.Size((int)(System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width * 0.25), (int)(System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height));

            //this.DesktopLocation = new System.Drawing.Point((int)(System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width * 0.75), 0);

            //this.Location = new System.Drawing.Point((int)(System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width * 0.75), 0);

        }

        private void FormLongNotificationMinimized_Load(object sender, EventArgs e)
        {
            this.Location = new System.Drawing.Point((int)(System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width * 0.75), 0);

            webBrowser1.ObjectForScripting = this;

#if DEBUG
            webBrowser1.ScriptErrorsSuppressed = true;
#endif

            //webBrowser1.Url = new Uri( @"www.google.com");

            //webBrowser1.Navigate(new System.Uri(@"http://www.google.com"));

            webBrowser1.Navigate(new System.Uri(Globals.functions.GetTemplatePath("LongNotificationTemplate.html")));
            
        }

        private void FormLongNotificationMinimized_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void Document_MouseDown(object sender, System.Windows.Forms.HtmlElementEventArgs e)
        {
            if (e.MouseButtonsPressed == MouseButtons.Left)
            {
                if (e.MousePosition.Y < 100 && e.MousePosition.X < this.Width*0.8)
                {

                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            }

        }

        public void ExternalHide()
        {
            //this.Hide();
            AmivoiceWatcher.TrayMenu.ToggleFormLongNotification();
            //frmMinimized.Show();

        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser1.Document != null)
            {
                var htmlDoc = webBrowser1.Document;
                //htmlDoc.Click += Document_MouseDown;
                //htmlDoc.MouseDown += new System.Windows.Forms.HtmlElementEventHandler(this.button1_Click);

                htmlDoc.MouseDown += Document_MouseDown;
                //htmlDoc.MouseUp += Document_MouseUp;
                //htmlDoc.MouseLeave += Document_MouseUp;

                //htmlDoc.MouseDown += htmlDoc_MouseDown;
                //htmlDoc.MouseMove += htmlDoc_MouseMove;
                //htmlDoc.ContextMenuShowing += htmlDoc_ContextMenuShowing;
            }
        }
    }
}
