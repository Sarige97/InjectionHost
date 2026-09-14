using muju;
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
    public partial class MainForm : Form
    {
        public Homepage Homepage { get; set; } = new Homepage();

        public MainForm()
        {
            InitializeComponent();
            Homepage.Dock = DockStyle.Fill;
            this.Controls.Add(Homepage);
        }

        public void ChangePageToDetail()
        {

        }
    }
}
