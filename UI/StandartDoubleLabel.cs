using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace moju.UI
{
    public partial class StandardDoubleLabel : UserControl
    {
        public StandardDoubleLabel()
        {
            InitializeComponent();
        }

        public string LeftLabelText
        {
            get => this.LeftLabel.Text;

            set => this.LeftLabel.Text = value;
        }

        public string RightLabelText
        {
            get => this.RightLabel.Text;
            set => this.RightLabel.Text = value;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void RightLabel_Click(object sender, EventArgs e)
        {

        }
    }
}
