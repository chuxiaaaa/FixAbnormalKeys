using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FixAbnormalKeys
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            textBox1.TextAlign = HorizontalAlignment.Center;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            textBox1.Text = e.KeyData.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
