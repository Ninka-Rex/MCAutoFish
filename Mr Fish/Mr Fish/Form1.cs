using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using CSCore.CoreAudioAPI;
using System.Threading;
using System.Threading.Tasks;
using MCAutoFish;

namespace Mr_Fish
{
    public partial class Form1 : Form
    {
        static public int highVal = 0;
        static public int fished = 0;
        static public Thread botThread;
        static public bool active = false;
        static public int pID = 0;
        static public Int64 allTimeFish = 0;
        private DateTime startTime;

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public Form1()
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            InitializeComponent();
            updateStartStopButtons();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            addProcesses();
            try
            {
                allTimeFish = MCAutoFish.Settings.Get<Int64>("TotalFish");
            }
            catch
            {

            }
        }

        private void addProcesses()
        {
            Process[] processlist = Process.GetProcesses();
            foreach (Process theprocess in processlist)
            {
                if (theprocess.ProcessName.Contains("Minecraft"))
                {
                    var txt = String.Format("{0} - {1}", theprocess.ProcessName, theprocess.Id);
                    var pID = theprocess.Id;
                    var i = cb_processes.Items.Add(new Item(txt, pID));

                    if (txt.Contains("Minecraft") && txt != "Minecraft Launcher")
                    {
                        cb_processes.SelectedIndex = i;
                        tssl_status.Text = "Minecraft found!";
                    }
                }
            }
        }
        private void fish()
        {
            try
            {
                var oVol = 0;
                System.Threading.Thread.Sleep(1500);
                while (oVol <= int.Parse(tb_volume.Text) && active)
                {
                    var cVol = getVolume();
                    if (cVol > oVol)
                    {
                        oVol = cVol;
                    }
                }

                send_rightclick();
                System.Threading.Thread.Sleep(200);
                send_rightclick();
                fished = fished + 1;
                MCAutoFish.Settings.Set("TotalFish", fished + allTimeFish);
            }
            catch { }
        }
        private void bot_start()
        {
            System.Threading.Thread.Sleep(2500);
            while (active)
            {
                fish();
            }
        }
        private int getVolume()
        {
            int ret = 0;
            Item item;
            item = (Item)GetControlPropertyThreadSafe(cb_processes, t => t.SelectedItem);


            if (item.Name != null)
            {
                var pID = item.Value;

                var sessionManager = GetDefaultAudioSessionManager2(CSCore.CoreAudioAPI.DataFlow.Render);
                var sessionEnumerator = sessionManager.GetSessionEnumerator();
                foreach (var session in sessionEnumerator)
                {
                    var audioMeterInformation = session.QueryInterface<CSCore.CoreAudioAPI.AudioMeterInformation>();
                    var session2 = session.QueryInterface<AudioSessionControl2>();
                    var processID = session2.ProcessID;
                    if (processID == pID)
                    {
                        ret = (int)(audioMeterInformation.GetPeakValue() * 1000);
                    }
                }
            }

            return ret;
        }



        public static U GetControlPropertyThreadSafe<T, U>(T control, Func<T, U> func) where T : Control
        {
            if (control.InvokeRequired)
            {
                return (U)control.Invoke(func, new object[] { control });
            }
            else
            {
                return func(control);
            }
        }
        private static AudioSessionManager2 GetDefaultAudioSessionManager2(CSCore.CoreAudioAPI.DataFlow dataFlow)
        {
            using (var enumerator = new CSCore.CoreAudioAPI.MMDeviceEnumerator())
            {
                using (var device = enumerator.GetDefaultAudioEndpoint(dataFlow, CSCore.CoreAudioAPI.Role.Multimedia))
                {
                    var sessionManager = AudioSessionManager2.FromMMDevice(device);
                    return sessionManager;
                }
            }
        }
        [DllImport("user32.dll", SetLastError = true)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        [Flags]
        public enum MouseEventFlags : uint
        {
            MOUSEEVENTF_RIGHTDOWN = 0x0008,
            MOUSEEVENTF_RIGHTUP = 0x0010,
        }

        public static void send_rightclick()
        {
            // Simulate right mouse button down
            mouse_event((uint)MouseEventFlags.MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, UIntPtr.Zero);

            // Simulate right mouse button up
            mouse_event((uint)MouseEventFlags.MOUSEEVENTF_RIGHTUP, 0, 0, 0, UIntPtr.Zero);
        }

        private void formatStatusText()
        {
            if (active)
            {
                TimeSpan duration = DateTime.Now.Subtract(startTime);
                string durationString = duration.ToString(@"d\:hh\:mm\:ss");
                string perHour = Math.Floor(fished / duration.TotalHours).ToString();
                tssl_status.Text = "Active for " + durationString + ", ~" + perHour + " catches/hour, current " + fished;
            }
        }
        private void updateStartStopButtons()
        {
            btn_start.Enabled = !active;
            btn_stop.Enabled = active;
        }


        private void cb_processes_SelectedIndexChanged(object sender, EventArgs e)
        {
            Item itm = (Item)cb_processes.SelectedItem;
            pID = itm.Value;
        }
        private void tb_volume_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            if (cb_processes.SelectedItem != null)
            {
                try
                {
                    formatStatusText();
                    Item itm = (Item)cb_processes.SelectedItem;

                    Task.Run(() =>
                    {
                        var sessionManager = GetDefaultAudioSessionManager2(CSCore.CoreAudioAPI.DataFlow.Render);
                        var sessionEnumerator = sessionManager.GetSessionEnumerator();
                        foreach (var session in sessionEnumerator)
                        {
                            var audioMeterInformation = session.QueryInterface<CSCore.CoreAudioAPI.AudioMeterInformation>();
                            var session2 = session.QueryInterface<AudioSessionControl2>();
                            var processID = session2.ProcessID;

                            if (processID == pID)
                            {
                            
                            
                                var vol = (int)(audioMeterInformation.GetPeakValue() * 1000);
                            
                                this.pb_audio.Invoke((MethodInvoker)delegate
                                {
                                    pb_audio.Maximum = 1000;
                                    pb_audio.Value = vol;
                                    lbl_curVol.Text = vol.ToString();
                                });

                                Debug.WriteLine(audioMeterInformation.GetPeakValue());
                                if (highVal < vol)
                                {

                                    this.lbl_maxVol.Invoke((MethodInvoker)delegate
                                    {
                                        highVal = vol;
                                        lbl_maxVol.Text = highVal.ToString();
                                    });
                                }
                            }
                        }
                    });

                    
                } catch {}
            }
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            active = true;
            botThread = new Thread(new ThreadStart(bot_start));
            botThread.IsBackground = true;
            botThread.Start();
            startTime = DateTime.Now;
            lblAbout.Visible = false;
            formatStatusText();
            updateStartStopButtons();
        }
        private void btn_stop_Click(object sender, EventArgs e)
        {
            try
            {
                active = false;
                botThread.Abort();
                botThread = null;
                tssl_status.Text = "Inactive";
                lblAbout.Visible = true;
                updateStartStopButtons();
            } catch { };
        }
        private void btn_calibrate_Click(object sender, EventArgs e)
        {
            highVal = 0;
            lbl_maxVol.Text = highVal.ToString();
        }

        private void lblAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MCAutoFish.about aboutForm = new MCAutoFish.about();
            aboutForm.ShowDialog();
        }
    }

    public class Item
    {
        public string Name;
        public int Value;
        public Item(string name, int value)
        {
            Name = name; Value = value;
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
