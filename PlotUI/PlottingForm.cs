using Data;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Windows.Forms;
using OxyPlot.WindowsForms;
using OxyPlot.Series;
using System.Windows.Forms.Integration;

namespace PlotUI
{
    public partial class PlottingForm : Form
    {
        public PlottingForm(RunData runResult )
        {

            InitializeComponent();
            _RunResult = runResult;

        }



        private void Draw()
        {
           var viewPort = new HelixToolkit.Wpf.HelixViewport3D();

            var elementHost = new ElementHost();

            elementHost.Child = viewPort;


            
        }

        private RunData _RunResult;
    }
}
