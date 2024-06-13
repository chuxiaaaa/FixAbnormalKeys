using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace FixAbnormalKeys
{
    public partial class Form1 : Form
    {
        public Dictionary<Keys, bool> KeyDownState = new Dictionary<Keys, bool>();
        public Dictionary<Keys, List<DateTime>> KeyDownTimes = new Dictionary<Keys, List<DateTime>>();

        public int handleCount;

        public string savePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\FixAbnormalKeys\\keys.txt";

        public Hook hook;

        public Form1()
        {
            InitializeComponent();
            textBox1.TextAlign = HorizontalAlignment.Center;
            textBox2.TextAlign = HorizontalAlignment.Center;
            listBox1.Items.Clear();
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            hook.Stop();
        }

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            ((TextBox)sender).Text = e.KeyData.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
            {
                return;
            }
            listBox1.Items.Add(textBox1.Text + "," + textBox2.Text);
            FileInfo fileInfo = new FileInfo(savePath);
            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }
            string[] lines = new string[listBox1.Items.Count];
            listBox1.Items.CopyTo(lines, 0);
            File.WriteAllLines(savePath, lines);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(Handle, 0x00A1, 2, 0);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
            {
                return;
            }
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            if (listBox1.Items.Count > 0)
            {
                listBox1.SelectedIndex = 0;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists(savePath))
            {
                foreach (var item in File.ReadAllLines(savePath))
                {
                    if (item.Contains(",") && item.Split(',').Length == 2)
                    {
                        var arr = item.Split(',');
                        if (Enum.TryParse<Keys>(arr[0], out _) && Enum.TryParse<Keys>(arr[1], out _))
                        {
                            listBox1.Items.Add(item);
                        }
                    }
                }
            }
            hook = new Hook();
            hook.KeyDown += KeyEvent;
            hook.KeyUp += KeyEvent;
            hook.Start();
        }

        public bool KeyEvent(bool IsKeyDown, KeyEventArgs e)
        {
            var list = listBox1.Items;
            foreach (var item in list)
            {
                var data = item.ToString();
                var arr = data.Split(',');
                var key1 = (Keys)Enum.Parse(typeof(Keys), arr[0]);
                var key2 = (Keys)Enum.Parse(typeof(Keys), arr[1]);
                if (e.KeyCode == key2)
                {
                    if (KeyDownState.TryGetValue(key1, out var down) && down)
                    {
                        UpdateHandleCount();
                        return true;
                    }
                    if (KeyDownTimes.TryGetValue(key1, out var times))
                    {
                        foreach (var item2 in times)
                        {
                            if (item2.AddMilliseconds(100) > DateTime.Now)
                            {
                                UpdateHandleCount();
                                return true;
                            }
                        }
                    }
                }
                if (e.KeyCode == key1)
                {
                    if (!KeyDownState.ContainsKey(key1))
                    {
                        KeyDownState.Add(key1, IsKeyDown);
                    }
                    else
                    {
                        KeyDownState[key1] = IsKeyDown;
                    }
                    if (!KeyDownTimes.ContainsKey(key1))
                    {
                        KeyDownTimes.Add(key1, new List<DateTime>());
                    }
                    KeyDownTimes[key1].Add(DateTime.Now);
                    if (KeyDownTimes[key1].Count > 100)
                    {
                        KeyDownTimes[key1].RemoveRange(0, 99);
                    }
                }
            }
            return false;
        }

        public void UpdateHandleCount()
        {
            handleCount++;
            label4.Text = $"拦截次数：{handleCount}";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.notifyIcon1.Visible = false;
            this.Hide();
        }
    }
}
