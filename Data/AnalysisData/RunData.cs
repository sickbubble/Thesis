using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adapters;
using ModelInfo;
using OptimizationAlgorithms;
using OptimizationAlgorithms.Particles;
using OptimizationAlgorithms.PSOObjects.Particles;
using OptimizationAlgorithms.PSOObjects.Swarms;
using OptimizationAlgorithms.Swarms;
using ThesisProject.Structural_Members;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using ThesisProject.Loads;

namespace Data
{


    public class RunDataList
    {
        public RunDataList()
        {

        }



        public List<RunData> ListOfRunData { get; set; }
    }

    public abstract class RunData
    {


        #region Private Fields

        private double _FrameHeight;
        private double _AlphaRatio;

        private eHorizon _Horizon;
        private eEndConditionSet _EndConditionValue;


        private double _LatticeMeshSize;
        private double _ShellMeshSize;
        private double _MemberDim;

        private double _ShellThickness;
        private double _Shell_E;
        private double _PoissonsRatio;
        private double _LoadMagnitude;





        #endregion


        #region Public Properties
        public double FrameHeight { get => _FrameHeight; set => _FrameHeight = value; }
        public double AlphaRatio { get => _AlphaRatio; set => _AlphaRatio = value; }
        public eHorizon Horizon { get => _Horizon; set => _Horizon = value; }
        public eEndConditionSet EndConditionValue { get => _EndConditionValue; set => _EndConditionValue = value; }
        public double LatticeMeshSize { get => _LatticeMeshSize; set => _LatticeMeshSize = value; }
        public double ShellMeshSize { get => _ShellMeshSize; set => _ShellMeshSize = value; }
        public double MemberDim { get => _MemberDim; set => _MemberDim = value; }
        public double ShellThickness { get => _ShellThickness; set => _ShellThickness = value; }
        public double Shell_E { get => _Shell_E; set => _Shell_E = value; }
        public double PoissonsRatio { get => _PoissonsRatio; set => _PoissonsRatio = value; }
        public double LoadMagnitude { get => _LoadMagnitude; set => _LoadMagnitude = value; }

        #endregion
    }

    public class RunResult : RunData, ICloneable, IParticleSwarmOptimizationAdaptee
    {
        #region Ctor
        public RunResult()
        {

        }

        public RunResult(RunInfo runInfo)
        {
            FillByRunInfo(runInfo);
        }

        #endregion

        #region Private Fields

        private Node _MinControlNode;
        private List<NodeCompareData> _NodeCompareData;
        private List<double> _ShellPeriods;
        private List<double> _LatticePeriods;
        private double[] _LatticeDisplacements;
        private double[] _ShellDisplacements;
        private double _EqualizationRatio;

        private double _PercentDiff;

        private int _ID;


        #endregion


        #region Public Properties
        public List<NodeCompareData> NodeCompareData { get => _NodeCompareData; set => _NodeCompareData = value; }
        public Node MinControlNode { get => _MinControlNode; set => _MinControlNode = value; }
        public double PercentDiff { get => _PercentDiff; set => _PercentDiff = value; }
        public List<double> ShellPeriods { get => _ShellPeriods; set => _ShellPeriods = value; }
        public List<double> LatticePeriods { get => _LatticePeriods; set => _LatticePeriods = value; }
        public int ID { get => _ID; set => _ID = value; }
        public double EqualizationRatio { get => _EqualizationRatio; set => _EqualizationRatio = value; }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public void FillByRunInfo(RunData runInfo)
        {
            this.FrameHeight = runInfo.FrameHeight;
            this.AlphaRatio = runInfo.AlphaRatio;
            this.Horizon = runInfo.Horizon;
            this.LatticeMeshSize = runInfo.LatticeMeshSize;
            this.ShellMeshSize = runInfo.ShellMeshSize;
            this.PoissonsRatio = runInfo.PoissonsRatio;
            this.Shell_E = runInfo.Shell_E;
            this.ShellThickness = runInfo.ShellThickness;
            this.MemberDim = runInfo.MemberDim;
            this.LoadMagnitude = runInfo.LoadMagnitude;

        }


        public IParticle GetOptimizationObject()
        {
            for (int i = 0; i < this.NodeCompareData.Count; i++)
            {
                _LatticeDisplacements[i] = this.NodeCompareData[i].LatticeVerticalDisp;
            }
            IParticle particle = new LatticeParticle(_LatticeDisplacements);
            return particle;
        }
        public bool SetShellAnalyticalResults()
        {


            var D = (this.Shell_E * Math.Pow(this.ShellThickness, 3)) / (12 * (1 - Math.Pow(this.PoissonsRatio, 2)));
            var G = this.Shell_E / (2 * (1 + this.PoissonsRatio));



            foreach (var data in this.NodeCompareData)
            {
                if (data.ShellVerticalDisp == 0)
                {

                    data.ShellAnalyticalVerticalDisp = 0;
                    continue;
                }
                var r = Math.Sqrt(Math.Pow(MemberDim / 2 - data.Point.X, 2) + Math.Pow(MemberDim / 2 - data.Point.Y, 2));



                data.ShellAnalyticalVerticalDisp = -this.LoadMagnitude / (48 * D) * Math.Pow(1 - Math.Pow(r / this.MemberDim, 2), 2) * (1 + 2 * Math.Pow(r / this.MemberDim, 2));
                //data.ShellAnalyticalVerticalDisp = -this.LoadMagnitude / ((64 * D * G) * ((Math.Pow(10,4) - Math.Pow(5,4))/((5*5 + 5*5)* (5 * 5 + 5 * 5))));
                data.ShellAnalyticalVerticalDisp = -this.LoadMagnitude * 10 * 10 / (64 * D * G) ;

                var ratio = data.ShellVerticalDisp / data.ShellAnalyticalVerticalDisp;

                var percentDiff = Math.Abs(data.ShellAnalyticalVerticalDisp - data.ShellVerticalDisp) / Math.Abs(data.ShellVerticalDisp);

            }


            return false;
        }



        public double[] GetBestSolution()
        {
            if (_ShellDisplacements == null)
            {
                _ShellDisplacements = new double[this.NodeCompareData.Count];
            }
            for (int i = 0; i < this.NodeCompareData.Count; i++)
            {
                _ShellDisplacements[i] = this.NodeCompareData[i].LatticeVerticalDisp;
            }
            return _ShellDisplacements;
        }

        public double[] GetDisplacementProfile()
        {
            if (_LatticeDisplacements == null)
            {
                _LatticeDisplacements = new double[this.NodeCompareData.Count];
            }

            for (int i = 0; i < this.NodeCompareData.Count; i++)
            {
                _LatticeDisplacements[i] = this.NodeCompareData[i].LatticeVerticalDisp;
            }
            return _LatticeDisplacements;
        }

        ISwarm IOptimizationAdaptee<ISwarm>.GetOptimizationObject()
        {
            throw new NotImplementedException();
        }

        public void PrepareCSVFilesForGnuplot()
        {

            // Define the file path and name
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string lattice_fp = desktop + "\\RunResults\\latticeRes2.csv";
            string shell_fp = desktop + "\\RunResults\\shellRes2.csv";
            string shellAnalytic_fp = desktop + "\\RunResults\\shellResAnalytic2.csv";


            StringBuilder sbShell = new StringBuilder();
            StringBuilder sbLattice = new StringBuilder();
            StringBuilder sbShellAnalytic = new StringBuilder();

            foreach (var data in NodeCompareData)
            {
                sbShell.AppendLine($"{data.Point.X  } ;{data.Point.Y}; {data.ShellVerticalDisp }");
                sbLattice.AppendLine($"{data.Point.X } ;{data.Point.Y}; {data.LatticeVerticalDisp}");
                sbShellAnalytic.AppendLine($"{data.Point.X } ;{data.Point.Y}; {data.ShellAnalyticalVerticalDisp}");

            }

            // Write the data to the CSV file
            using (StreamWriter writer = new StreamWriter(lattice_fp))
            {
                writer.Write(sbLattice.ToString());
            }
            using (StreamWriter writer = new StreamWriter(shell_fp))
            {
                writer.Write(sbShell.ToString());
            }

            using (StreamWriter writer = new StreamWriter(shellAnalytic_fp))
            {
                writer.Write(sbShellAnalytic.ToString());
            }

            var process = new Process();
            //process.StartInfo = new ProcessStartInfo(desktop + "\\RunResults\\gnuplot\\gnuplot 5.2 patchlevel 8 - console version", desktop + "\\RunResults\\GnuplotCommands.gp");
            //process.StartInfo = new ProcessStartInfo( desktop + "\\RunResults\\GnuplotCommands.gp");
            process.StartInfo = new ProcessStartInfo( desktop + "\\RunResults\\GnuplotCommandsAnalytic.gp");
            process.Start();
            process.WaitForExit();

        }





        #endregion


    }


    public class RunInfo : RunData, ICloneable
    {
        #region Ctor
        public RunInfo()
        {

        }

        #endregion

        #region Private Fields



        private eSupportType _BorderSupportType;
        private eModelGeometryType _GeometryType;
        private double _ShelllUnitWeigth;
        private bool _IsShellOnlyPlate;
        private eLoadingType _LoadingType;


        #endregion


        #region Public Properties

        public eModelGeometryType GeometryType { get => _GeometryType; set => _GeometryType = value; }
        public eSupportType BorderSupportType { get => _BorderSupportType; set => _BorderSupportType = value; }
        public double ShelllUnitWeigth { get => _ShelllUnitWeigth; set => _ShelllUnitWeigth = value; }
        public bool IsShellOnlyPlate { get => _IsShellOnlyPlate; set => _IsShellOnlyPlate = value; }
        public eLoadingType LoadingType { get => _LoadingType; set => _LoadingType = value; }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public void FillByRunInfo(RunData runInfo)
        {
            this.FrameHeight = runInfo.FrameHeight;
            this.AlphaRatio = runInfo.AlphaRatio;
            this.Horizon = runInfo.Horizon;
            this.LatticeMeshSize = runInfo.LatticeMeshSize;
            this.ShellMeshSize = runInfo.ShellMeshSize;
            this.EndConditionValue = runInfo.EndConditionValue;

        }





        #endregion


    }

    public class NodeCompareData
    {
        #region Ctor
        public NodeCompareData()
        {

        }

        #endregion

        #region Private Fields

        private int _NodeID;
        private ModelInfo.Point _Point;


        private double _ShellVerticalDisp;
        private double _ShellAnalyticalVerticalDisp;
        private double _LatticeVerticalDisp;
        private double _PercentDiff;




        #endregion


        #region Public Properties
        public int NodeID { get => _NodeID; set => _NodeID = value; }
        public double ShellVerticalDisp { get => _ShellVerticalDisp; set => _ShellVerticalDisp = value; }
        public double ShellAnalyticalVerticalDisp { get => _ShellAnalyticalVerticalDisp; set => _ShellAnalyticalVerticalDisp = value; }
        public double LatticeVerticalDisp { get => _LatticeVerticalDisp; set => _LatticeVerticalDisp = value; }
        public double PercentDiff { get => _PercentDiff; set => _PercentDiff = value; }
        public Point Point { get => _Point; set => _Point = value; }

        #endregion

    }



}
