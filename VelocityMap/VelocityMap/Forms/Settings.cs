using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VelocityMap
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            this.ipaddress.Text = Properties.Settings.Default.IpAddress;
            this.username.Text = Properties.Settings.Default.Username;
            this.password.Text = Properties.Settings.Default.Password;
            this.riopath.Text = Properties.Settings.Default.RioMPPath;
            this.trackWidth.Text = Properties.Settings.Default.TrackWidth.ToString();
        }

        private void save_Click(object sender, EventArgs e)
        {


            if (!ValidateIPv4(this.ipaddress.Text))
            {
                MessageBox.Show("This ip address is invalid!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Properties.Settings.Default.IpAddress = this.ipaddress.Text;
            Properties.Settings.Default.Username = this.username.Text;
            Properties.Settings.Default.Password = this.password.Text;
            Properties.Settings.Default.RioMPPath = this.riopath.Text;
            Properties.Settings.Default.TrackWidth = float.Parse(this.trackWidth.Text.ToString());

            Properties.Settings.Default.Save();

            this.Close();
        }

        /// <summary>
        /// Used to validate the ip address of the robot to make sure that it is in an ipv4 format.
        /// </summary>
        /// <param name="ipString">The ip string value.</param>
        /// <returns>a boolean that tells you if the ip is in ipv4 format.</returns>
        public bool ValidateIPv4(string ipString)
        {
            // if the text contains a whitespace/space or a null value then it is clearly not a ip address.
            if (String.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }
            //Split the ip address into different parts
            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }

            byte tempForParsing;
            //check to see if all of the values are bytes.
            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void trackWidth_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}
