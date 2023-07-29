using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace USBPortControllerAppEA
{
    
    public partial class Form1 : Form
    {
        private bool allowHIDDevices = false; // HID aygıtlarına izin vermek için kullanılacak flag
        private Timer notificationTimer;
        private bool isNotifying = false;
        public Form1()
        {
            InitializeComponent();
            InitializeNotifyIcons();
            InitializeTimer();

        }
        private void InitializeNotifyIcons()
        {
            // NotifyIcon1: Power Off
            notifyIcon1 = new NotifyIcon();
            notifyIcon1.Icon = SystemIcons.Information;
            notifyIcon1.Text = "USB Ports Power Off";
            notifyIcon1.Visible = false;

            // NotifyIcon2: Power On
            notifyIcon2 = new NotifyIcon();
            notifyIcon2.Icon = SystemIcons.Information;
            notifyIcon2.Text = "USB Ports Power On";
            notifyIcon2.Visible = false;
        }
        private void InitializeTimer()
        {
            notificationTimer = new Timer();
            notificationTimer.Interval = 1500; // 1.5 saniye
            notificationTimer.Tick += NotificationTimer_Tick;
        }

        private void NotificationTimer_Tick(object sender, EventArgs e)
        {
            HideNotifyIcons();
            notificationTimer.Stop();
        }

        private void HideNotifyIcons()
        {
            notifyIcon1.Visible = false;
            notifyIcon2.Visible = false;
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Hide();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DisableUSBPorts(); // Disable method for USB Ports;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            EnableUSBPorts(); // Enable method for USB Ports;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            DisableUSBPorts();
            AllowHIDDevices(); // Toggle AllowHIDDevices flag on button click
        }
        private void DisableUSBPorts()
        {
            try
            {
                ManagementScope scope = new ManagementScope("\\\\.\\ROOT\\cimv2");
                ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%USB%'");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject device in collection)
                {
                    string deviceId = device.GetPropertyValue("DeviceID").ToString();
                    using (ManagementBaseObject inParams = device.GetMethodParameters("Disable"))
                    {
                        using (ManagementBaseObject outParams = device.InvokeMethod("Disable", inParams, null))
                        {
                            uint returnValue = (uint)outParams["ReturnValue"];
                            if (returnValue == 0)
                            {
                                label1.Text = "USB Ports Power Off";
                                if (!isNotifying)
                                {
                                    notifyIcon1.Visible = true;
                                    notifyIcon1.ShowBalloonTip(1000, "USBPowerApp ", "The USB ports have been powered off.", ToolTipIcon.Info);
                                    isNotifying = true;
                                    notificationTimer.Start();
                                }
                            }
                            else
                            {
                                label1.Text = "Failed to power off USB ports. Error Code: " + returnValue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                label1.Text = "Something went wrong: " + ex.Message;
            }
        }
        private void EnableUSBPorts()
        {
            try
            {
                ManagementScope scope = new ManagementScope("\\\\.\\ROOT\\cimv2");
                ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%USB%'");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject device in collection)
                {
                    string deviceId = device.GetPropertyValue("DeviceID").ToString();
                    using (ManagementBaseObject inParams = device.GetMethodParameters("Enable"))
                    {
                        using (ManagementBaseObject outParams = device.InvokeMethod("Enable", inParams, null))
                        {
                            uint returnValue = (uint)outParams["ReturnValue"];
                            if (returnValue == 0)
                            {
                                label1.Text = "USB Ports Power On";
                                if (!isNotifying)
                                {
                                    notifyIcon2.Visible = true;
                                    notifyIcon2.ShowBalloonTip(1000, "USBPowerApp ", "The USB ports have been powered on.", ToolTipIcon.Info);
                                    isNotifying = true;
                                    notificationTimer.Start();
                                }
                            }
                            else
                            {
                                label1.Text = "Failed to power on USB ports. Error Code: " + returnValue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                label1.Text = "Something went wrong: " + ex.Message;
            }
        }
        private void AllowHIDDevices()
        {
            try
            {
                // Change devcon location
                string devconPath = @"C:\Program Files (x86)\Windows Kits\10\Tools\10.0.22621.0\arm64\devcon.exe"; // Devcon.exe'nin yolunu ayarlayın

                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = devconPath;

                // Change device location
                startInfo.Arguments = "HID\\VID_03F0&PID_3A41&MI_00\\7&2D82FB87&0&0000\r\n";

                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
                int exitCode = process.ExitCode;

                if (exitCode == 0)
                {
                    label1.Text = "Only HID devices (e.g., keyboard, mouse) are allowed.";
                }
                else
                {
                    label1.Text = "Failed to enable HID devices. Exit Code: " + exitCode;
                }
            }
            catch (Exception ex)
            {
                label1.Text = "Something went wrong: " + ex.Message;
            }
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {
            
        }

       
    }
}
