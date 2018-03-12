using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;

namespace AmivoiceWatcher
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public partial class Notification : Form
    {
        private static readonly List<Notification> OpenNotifications = new List<Notification>();
        private bool _allowFocus;
        private readonly FormAnimator _animator;
        private IntPtr _currentForegroundWindow;

        public Notification(string title, string body, int duration, FormAnimator.AnimationMethod animation, FormAnimator.AnimationDirection direction, int width = 0, int height = 0)
        {
            InitializeComponent();

            if (duration < 0)
                duration = int.MaxValue;
            else
                duration = duration * 1000;

            lifeTimer.Interval = duration;

            if (width > 0)
            {
                this.Width = width;
            }

            if (width > 0)
            {
                this.Height = height;
            }

            _animator = new FormAnimator(this, animation, direction, 500);

        }

        #region Methods
        public static void Popup(string title, string body, int duration, FormAnimator.AnimationMethod animationMethod, FormAnimator.AnimationDirection animationDirection)
        {
            Notification toastNotification = new Notification(title, body, duration, animationMethod, animationDirection);
            toastNotification.Show();
        }

        public new void Show()
        {
            // Determine the current foreground window so it can be reactivated each time this form tries to get the focus
            _currentForegroundWindow = NativeMethods.GetForegroundWindow();

            base.Show();
        }

        public void Close(String message)
        {
            MessageBox.Show(message, "client code");
        }

        public void openLinkInDefaultBrowser(string url)
        {
            Globals.functions.openLinkInDefaultBrowser(url);

        }

        public void openSuperNotification(string url)
        {
            Globals.functions.openSuperNotification(url);

        }

        public bool HasHorizontalScrollbar
        {
            get
            {
                var width1 = webBrowser1.Document.Body.ScrollRectangle.Width;
                var width2 = webBrowser1.Document.Window.Size.Width;

                return width1 > width2;
            }
        }

        #endregion // Methods

        #region Event Handlers





        private void Notification_Load(object sender, EventArgs e)
        {
            // Allow webbrowser to run parent function
            webBrowser1.AllowWebBrowserDrop = false;
            webBrowser1.IsWebBrowserContextMenuEnabled = false;
            webBrowser1.WebBrowserShortcutsEnabled = false;
            webBrowser1.ObjectForScripting = this;

#if DEBUG
            webBrowser1.ScriptErrorsSuppressed = true;
#endif

            webBrowser1.Navigating += new WebBrowserNavigatingEventHandler(webBrowser1_Navigating);


            // Display the form just above the system tray.
            Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - Width,
                                      Screen.PrimaryScreen.WorkingArea.Height - Height);

            // Move each open form upwards to make room for this one
            foreach (Notification openForm in OpenNotifications)
            {
                openForm.Top -= Height;
            }

            OpenNotifications.Add(this);
            lifeTimer.Start();


        }

        private void Notification_Activated(object sender, EventArgs e)
        {
            // Prevent the form taking focus when it is initially shown
            if (!_allowFocus)
            {
                // Activate the window that previously had focus
                NativeMethods.SetForegroundWindow(_currentForegroundWindow);
            }
            //Console.WriteLine(" [x] Done");
        }

        private void Notification_Shown(object sender, EventArgs e)
        {
            // Once the animation has completed the form can receive focus
            _allowFocus = true;

            // Close the form by sliding down.
            _animator.Duration = 0;
            _animator.Direction = FormAnimator.AnimationDirection.Down;

        }

        private void Notification_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Move down any open forms above this one
            foreach (Notification openForm in OpenNotifications)
            {
                if (openForm == this)
                {
                    // Remaining forms are below this one
                    break;
                }
                openForm.Top += Height;
            }

            OpenNotifications.Remove(this);
        }

        private void lifeTimer_Tick(object sender, EventArgs e)
        {
            Close();
        }

        private void Notification_Click(object sender, EventArgs e)
        {
            //Close();
        }

        private void labelTitle_Click(object sender, EventArgs e)
        {
            //Close();
        }

        private void labelRO_Click(object sender, EventArgs e)
        {
            //Close();
        }

        private void buttonX_Click_1(object sender, EventArgs e)
        {
            Close();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (HasHorizontalScrollbar)
            {
                webBrowser1.Size = new Size(webBrowser1.Document.Body.ScrollRectangle.Width, webBrowser1.Document.Body.ScrollRectangle.Height);
                Notification.ActiveForm.Size = new Size(webBrowser1.Document.Body.ScrollRectangle.Width, webBrowser1.Document.Body.ScrollRectangle.Height);
            }
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {

        }

        #endregion // Event Handlers

    }
}