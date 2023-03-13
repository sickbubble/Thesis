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




        #endregion


        #region Public Properties
        public double FrameHeight { get => _FrameHeight; set => _FrameHeight = value; }
        public double AlphaRatio { get => _AlphaRatio; set => _AlphaRatio = value; }
        public eHorizon Horizon { get => _Horizon; set => _Horizon = value; }
        public eEndConditionSet EndConditionValue { get => _EndConditionValue; set => _EndConditionValue = value; }
        public double LatticeMeshSize { get => _LatticeMeshSize; set => _LatticeMeshSize = value; }
        public double ShellMeshSize { get => _ShellMeshSize; set => _ShellMeshSize = value; }
        public double MemberDim { get => _MemberDim; set => _MemberDim = value; }

        #endregion
    }

    public class RunResult : RunData, ICloneable, IParticleSwarmOptimizationAdaptee
    {
        #region Ctor
        public RunResult()
        {

        }

        #endregion

        #region Private Fields

        private Node _MinControlNode;
        private List<NodeCompareData> _NodeCompareData;
        private List<double> _ShellPeriods;
        private List<double> _LatticePeriods;
        private double _EnergyRatio;
        private double[] _LatticeDisplacements;
        private double[] _ShellDisplacements;

        private double _PercentDiff;

        private int _ID;


        #endregion


        #region Public Properties
        public List<NodeCompareData> NodeCompareData { get => _NodeCompareData; set => _NodeCompareData = value; }
        public double EnergyRatio { get => _EnergyRatio; set => _EnergyRatio = value; }
        public Node MinControlNode { get => _MinControlNode; set => _MinControlNode = value; }
        public double PercentDiff { get => _PercentDiff; set => _PercentDiff = value; }
        public List<double> ShellPeriods { get => _ShellPeriods; set => _ShellPeriods = value; }
        public List<double> LatticePeriods { get => _LatticePeriods; set => _LatticePeriods = value; }
        public int ID { get => _ID; set => _ID = value; }

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



        private double _ShellThickness;
        private eSupportType _BorderSupportType;
        private eModelGeometryType _GeometryType;


        #endregion


        #region Public Properties
        public double ShellThickness { get => _ShellThickness; set => _ShellThickness = value; }

        public eModelGeometryType GeometryType { get => _GeometryType; set => _GeometryType = value; }
        public eSupportType BorderSupportType { get => _BorderSupportType; set => _BorderSupportType = value; }

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
        private double _LatticeVerticalDisp;
        private double _PercentDiff;




        #endregion


        #region Public Properties
        public int NodeID { get => _NodeID; set => _NodeID = value; }
        public double ShellVerticalDisp { get => _ShellVerticalDisp; set => _ShellVerticalDisp = value; }
        public double LatticeVerticalDisp { get => _LatticeVerticalDisp; set => _LatticeVerticalDisp = value; }
        public double PercentDiff { get => _PercentDiff; set => _PercentDiff = value; }
        public Point Point { get => _Point; set => _Point = value; }

        #endregion

    }



}
