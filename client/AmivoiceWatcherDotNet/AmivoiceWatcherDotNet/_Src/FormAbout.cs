using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AmivoiceWatcher
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();
        }

        private void FormAbout_Load(object sender, EventArgs e)
        {
            this.Icon = new Icon("AmivoiceWatcher.ico");

            labelAppVersion.Text = "Amivoice Watcher v." + Assembly.GetEntryAssembly().GetName().Version;
            //load image
            pictureBoxLogo.SizeMode = PictureBoxSizeMode.StretchImage;


            //load PC info
            textBoxAbout.Text = String.Format(@"Username = {0}
Computer name = {1}
Domain name = {4}
Computer ip = {2}
Computer mac address = {3}
", Globals.myComputerInfo.UserName, Globals.myComputerInfo.ComputerName, ComputerInfo.IpForShow(Globals.myComputerInfo.Ip), Globals.myComputerInfo.MacAddress, Globals.myComputerInfo.DomainName);

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
