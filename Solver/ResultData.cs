using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelInfo;
using ThesisProject.Structural_Members;

namespace Solver
{
    public class RunData : ICloneable
    {
        #region Ctor
        public RunData()
        {

        }

        #endregion

        #region Private Fields

        private Node _MinControlNode; 
        private Dictionary<int, NodeCompareData> _NodeCompareData;
        private double _ShellThickness;
        private double _EnergyRatio;
        private double _AlphaRatio;
        private double _Horizon;
        private bool _IsTorsionalRelease;
        private double _PercentDiff;

        #endregion


        #region Public Properties
        public Dictionary<int,NodeCompareData> NodeCompareData { get => _NodeCompareData; set => _NodeCompareData = value; }
        public double ShellThickness { get => _ShellThickness; set => _ShellThickness = value; }
        public double EnergyRatio { get => _EnergyRatio; set => _EnergyRatio = value; }
        public double AlphaRatio { get => _AlphaRatio; set => _AlphaRatio = value; }
        public double Horizon { get => _Horizon; set => _Horizon = value; }
        public bool IsTorsionalRelease { get => _IsTorsionalRelease; set => _IsTorsionalRelease = value; }
        public Node MinControlNode { get => _MinControlNode; set => _MinControlNode = value; }
        public double PercentDiff { get => _PercentDiff; set => _PercentDiff = value; }

        public object Clone()
        {
            throw new NotImplementedException();
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

        private Node _Node;
        private double _ShellVerticalDisp;
        private double _LatticeVerticalDisp;
        private double _PercentDiff;




        #endregion


        #region Public Properties
        public Node Node { get => _Node; set => _Node = value; }
        public double ShellVerticalDisp { get => _ShellVerticalDisp; set => _ShellVerticalDisp = value; }
        public double LatticeVerticalDisp { get => _LatticeVerticalDisp; set => _LatticeVerticalDisp = value; }
        public double PercentDiff { get => _PercentDiff; set => _PercentDiff = value; }

        #endregion

    }



}
