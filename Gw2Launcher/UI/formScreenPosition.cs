﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI
{
    public partial class formScreenPosition : Form
    {
        public event EventHandler<ScreenPositionChangedEventArgs> ScreenPositionChanged;

        public class ScreenPositionChangedEventArgs : EventArgs
        {
            public ScreenPositionChangedEventArgs(int screen, Settings.ScreenAnchor anchor)
            {
                this.Screen = screen;
                this.Anchor = anchor;
            }

            public int Screen
            {
                get;
                private set;
            }

            public Settings.ScreenAnchor Anchor
            {
                get;
                private set;
            }
        }

        private ushort focus;
        private Screen[] screens;
        private int currentScreen;
        private Settings.ScreenAnchor currentAnchor;
        private Tools.BackgroundPatcher.DownloadProgressEventArgs pe;
        private bool sample;

        public formScreenPosition(int screen, Settings.ScreenAnchor anchor)
        {
            InitializeComponent();

            pe = new Tools.BackgroundPatcher.DownloadProgressEventArgs()
            {
                build = 12345,
                filesTotal = 123,
                startTime = DateTime.UtcNow,
                contentBytesTotal = 12345
            };

            screens = Screen.AllScreens;
            currentScreen = screen;

            if (currentScreen >= screens.Length)
                currentScreen = 0;

            if (screens.Length > 1)
            {
                arrowLeft.Visible = true;
                arrowRight.Visible = true;
            }

            labelScreen.Text = (currentScreen + 1).ToString();

            radioBL.Tag = Settings.ScreenAnchor.BottomLeft;
            radioBR.Tag = Settings.ScreenAnchor.BottomRight;
            radioTL.Tag = Settings.ScreenAnchor.TopLeft;
            radioTR.Tag = Settings.ScreenAnchor.TopRight;

            radioBR.Checked = true;
            currentAnchor = Settings.ScreenAnchor.BottomRight;

            foreach (Control c in this.Controls)
            {
                c.LostFocus += c_LostFocus;
                if (c is RadioButton)
                {
                    var r = (RadioButton)c;
                    if (r.Tag != null && (Settings.ScreenAnchor)r.Tag == anchor)
                    {
                        r.Checked = true;
                        currentAnchor = anchor;
                    }
                    r.CheckedChanged += radio_CheckedChanged;
                }
            }
        }

        public int SelectedScreen
        {
            get
            {
                return currentScreen;
            }
        }

        public Settings.ScreenAnchor SelectedAnchor
        {
            get
            {
                return currentAnchor;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.Focus();

            radioBL.Click += radio_Click;
            radioTL.Click += radio_Click;
            radioBR.Click += radio_Click;
            radioTR.Click += radio_Click;
        }

        void radio_Click(object sender, EventArgs e)
        {
            ShowSample();
        }

        void radio_CheckedChanged(object sender, EventArgs e)
        {
            var r = (RadioButton)sender;
            if (r.Checked)
            {
                currentAnchor = (Settings.ScreenAnchor)r.Tag;
                OnScreenPositionChanged();
            }
        }

        async void OnLostFocus()
        {
            var f = ++focus;
            await Task.Delay(100);
            if (f == focus && !this.ContainsFocus)
                this.Close();
        }

        void c_LostFocus(object sender, EventArgs e)
        {
            if (!this.ContainsFocus)
                OnLostFocus();
        }

        private void formScreenPosition_Load(object sender, EventArgs e)
        {

        }

        private void ShowSample()
        {
            if (sample = !sample)
                formBuildNotify.Show(formBuildNotify.NotifyType.DownloadingManifests, currentScreen, currentAnchor, pe);   
            else
                formBuildNotify.Show(formBuildNotify.NotifyType.PatchReady, currentScreen, currentAnchor, pe);

            this.Focus();
        }

        private void OnScreenChanged()
        {
            labelScreen.Text = (currentScreen + 1).ToString();
            ShowSample();
            OnScreenPositionChanged();
        }

        private void OnScreenPositionChanged()
        {
            if (ScreenPositionChanged != null)
                ScreenPositionChanged(this, new ScreenPositionChangedEventArgs(currentScreen, currentAnchor));
        }

        private void arrowRight_Click(object sender, EventArgs e)
        {
            if (--currentScreen < 0)
                currentScreen = screens.Length - 1;
            OnScreenChanged();
        }

        private void arrowLeft_Click(object sender, EventArgs e)
        {
            if (++currentScreen >= screens.Length)
                currentScreen = 0;
            OnScreenChanged();
        }
    }
}
