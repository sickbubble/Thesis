using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelInfo;
using ThesisProject.Structural_Members;

namespace Data
{
    public class RunDataList
    {
        public RunDataList()
        {

        }



        public List<RunData> ListOfRunData { get  ; set  ; }
    }

    public class RunData : ICloneable
    {
        #region Ctor
        public RunData()
        {

        }

        #endregion

        #region Private Fields

        private Node _MinControlNode; 
        private List<NodeCompareData> _NodeCompareData;
        private List<double> _ShellPeriods;
        private List<double> _LatticePeriods;
        private double _ShellThickness;
        private double _EnergyRatio;
        private double _AlphaRatio;
        private double _Horizon;
        private bool _IsTorsionalRelease;
        private double _PercentDiff;
        private double _LatticeMeshSize;
        private double _ShellMeshSize;
        private double _MemberDim;
        private int _ID ;

        private eModelGeometryType _GeometryType;
        private eSupportType _BorderSupportType;

        #endregion


        #region Public Properties
        public List<NodeCompareData> NodeCompareData { get => _NodeCompareData; set => _NodeCompareData = value; }
        public double FrameHeight { get => _ShellThickness; set => _ShellThickness = value; }
        public double EnergyRatio { get => _EnergyRatio; set => _EnergyRatio = value; }
        public double AlphaRatio { get => _AlphaRatio; set => _AlphaRatio = value; }
        public double Horizon { get => _Horizon; set => _Horizon = value; }
        public bool IsTorsionalRelease { get => _IsTorsionalRelease; set => _IsTorsionalRelease = value; }
        public Node MinControlNode { get => _MinControlNode; set => _MinControlNode = value; }
        public double PercentDiff { get => _PercentDiff; set => _PercentDiff = value; }
        public List<double> ShellPeriods { get => _ShellPeriods; set => _ShellPeriods = value; }
        public List<double> LatticePeriods { get => _LatticePeriods; set => _LatticePeriods = value; }
        public double LatticeMeshSize { get => _LatticeMeshSize; set => _LatticeMeshSize = value; }
        public double ShellMeshSize { get => _ShellMeshSize; set => _ShellMeshSize = value; }
        public int ID { get => _ID; set => _ID = value; }
        public double MemberDim { get => _MemberDim; set => _MemberDim = value; }
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
            this.IsTorsionalRelease = runInfo.IsTorsionalRelease;

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
