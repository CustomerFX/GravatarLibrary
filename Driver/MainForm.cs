using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FX.Utility;

namespace Driver
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            comboSize.SelectedIndex = 9;
        }

        private void buttonGetImage_Click(object sender, EventArgs e)
        {
            if (textEmail.Text == string.Empty) return;
            pictureAvatar.Image = null;

            Gravatar gravatar = new Gravatar(textEmail.Text, int.Parse(comboSize.Text));
            gravatar.GetImage();
            pictureAvatar.Image = gravatar.Image;
        }

        private void buttonGetImageAsync_Click(object sender, EventArgs e)
        {
            if (textEmail.Text == string.Empty) return;
            pictureAvatar.Image = null;

            Gravatar gravatar = new Gravatar(textEmail.Text, int.Parse(comboSize.Text));
            gravatar.AsyncImageRetrieved += new EventHandler(this.GravatarReceived);
            gravatar.GetImageAsync();
        }

        private void GravatarReceived(object sender, EventArgs e)
        {
            Gravatar gravatar = (Gravatar)sender;
            pictureAvatar.Image = gravatar.Image;
        }
    }
}
