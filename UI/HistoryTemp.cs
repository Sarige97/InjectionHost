using moju.device;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using moju.domain;
using muju.task;

namespace moju.UI
{
    public partial class HistoryTemp : Form
    {
        private InjectionMoldingMachineSlave _injectionMoldingMachineSlave;

        public HistoryTemp(InjectionMoldingMachineSlave injectionMoldingMachineSlave)
        {
            _injectionMoldingMachineSlave = injectionMoldingMachineSlave;
            InitializeComponent();
            Timer timer = new Timer
            {
                Interval = 500
            };
            timer.Tick += refreshData;
            timer.Start();
        }

        public void refreshData(Object sender, EventArgs args)
        {
            Queue<DataHistory<int>> historyTemp = _injectionMoldingMachineSlave.HistoryTemp;
            if (chart1 != null && chart1.Series != null && chart1.Series["temperature"] != null)
            {
                Series series = chart1.Series["temperature"];
                series.Points.Clear();
                foreach (DataHistory<int> tempData in historyTemp)
                {
                    series.Points.AddXY(tempData.dateTime, tempData.value / 10.0f);
                }
            }
        }


    }
}
