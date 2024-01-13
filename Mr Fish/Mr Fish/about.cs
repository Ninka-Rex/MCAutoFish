using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MCAutoFish
{
    public partial class about : Form
    {
        public about()
        {
            InitializeComponent();
        }

        private void about_Load(object sender, EventArgs e)
        {
            try
            {
                Int64 allTimeFish = MCAutoFish.Settings.Get<Int64>("TotalFish");
                richTextBox1.AppendText($"\nAll time catches: {allTimeFish.ToString()}");
            } catch {}
            richTextBox1.SelectAll();
            richTextBox1.SelectionAlignment = HorizontalAlignment.Center;
            richTextBox1.DeselectAll();

            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
