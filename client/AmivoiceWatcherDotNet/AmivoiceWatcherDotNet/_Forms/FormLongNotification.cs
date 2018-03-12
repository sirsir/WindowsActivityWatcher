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
    public partial class FormLongNotification : Form
    {

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private Form frmMinimized = new FormLongNotificationMinimized();

        private void FormLongNotification_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }


        private void FormLongNotification_Load(object sender, EventArgs e)
        {
            // Allow webbrowser to run parent function
            //webBrowser1.AllowWebBrowserDrop = false;
            //webBrowser1.IsWebBrowserContextMenuEnabled = false;
            //webBrowser1.WebBrowserShortcutsEnabled = false;
            webBrowser1.ObjectForScripting = this;

#if DEBUG
            webBrowser1.ScriptErrorsSuppressed = true;
#endif

            


        }


        private void Document_MouseDown(object sender, System.Windows.Forms.HtmlElementEventArgs e)
        {
            if (e.MouseButtonsPressed == MouseButtons.Left)
            {
                if (e.MousePosition.Y < 100 && e.MousePosition.X < 287)
                {

                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            }
           
        }

        private void Document_MouseUp(object sender, System.Windows.Forms.HtmlElementEventArgs e)
        {
            //if (e.MouseButtonsPressed == MouseButtons.Left)
            //{
            //if (e.MousePosition.Y < 100)
            //{

            var currentPoint = this.Location;

            currentPoint.Y = 0;

            if (currentPoint.X < 0)
            {
                currentPoint.X = 0;
            }else if (currentPoint.X > System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width * 0.75)
            {
                currentPoint.X = (int)(System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width * 0.75);
            }

            this.Location = currentPoint;

               // }
            //}

        }

        public void ExternalHide()
        {
            this.Hide();

            frmMinimized.Show();

        }

        public FormLongNotification()
        {
            InitializeComponent();

            this.ClientSize = new System.Drawing.Size((int)(System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width * 0.25), (int)(System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height));
            //this.Location = new System.Drawing.Point((int)(System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width * 0.75), 0);
            this.DesktopLocation = new System.Drawing.Point((int)(System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width * 0.75), 0);

            this.Location = new System.Drawing.Point((int)(System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width * 0.75), 0);


            var address = "http://192.168.1.149:3000/watchercli/sirisak/notification";

            //var address = "http://google.com";
            webBrowser1.Navigate(new Uri(address));
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser1.Document != null)
            {
                var htmlDoc = webBrowser1.Document;
                //htmlDoc.Click += Document_MouseDown;
                //htmlDoc.MouseDown += new System.Windows.Forms.HtmlElementEventHandler(this.button1_Click);

                htmlDoc.MouseDown += Document_MouseDown;
                htmlDoc.MouseUp += Document_MouseUp;
                htmlDoc.MouseLeave += Document_MouseUp;

                //htmlDoc.MouseDown += htmlDoc_MouseDown;
                //htmlDoc.MouseMove += htmlDoc_MouseMove;
                //htmlDoc.ContextMenuShowing += htmlDoc_ContextMenuShowing;
            }
        }

        

        private void FormLongNotification_VisibleChanged(object sender, EventArgs e)
        {
            

            if (this.Visible)
            {
                frmMinimized.Hide();
            }else
            {
                frmMinimized.Location = this.Location;
                frmMinimized.Width = this.Width;
                frmMinimized.Opacity = this.Opacity;
                frmMinimized.Show(this);
            }
        }
    }
}
