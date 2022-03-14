using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThesisProject;

namespace Data
{
    public class LatticeModelResultData : IAnlResultData
    {
         #region Ctor
        public LatticeModelResultData()
        {

        }
        #endregion


        #region Public Properties


        #endregion

        #region Private Fields

        private Dictionary<int, List<double>> _NodeResults; 
        private Dictionary<int, FrameMemberResults> _FrameResults;
        private MatrixCS _DispRes;
        private List<double> _ListOfPeriods;




        #endregion



        #region Interface Implementations
        public int AnalysisID { get ; set ; }
        public eAnalysisModelType AnalysisModelType { get ; set ; }
        /// <summary>
        /// // Node ID and ListOfResults
        /// </summary>
        public Dictionary<int, List<double>> NodeResults { get => _NodeResults; set => _NodeResults = value; }
        /// <summary>
        /// Frame ID and FrameMemberResults
        /// </summary>
        public Dictionary<int, FrameMemberResults> FrameResults { get => _FrameResults; set => _FrameResults = value; }
        public MatrixCS DispRes { get => _DispRes; set => _DispRes = value; }
        public List<double> ListOfPeriods { get => _ListOfPeriods; set => _ListOfPeriods = value; }

        #endregion
    }



    public class FrameMemberResults
    {
        public FrameMemberResults()
        {

        }


        private List<double> _INodeDisplacements_Global;
        private List<double> _JNodeDisplacements_Global;
        private List<double> _INodeForces_Global;
        private List<double> _JNodeForces_Global;

        private List<double> _INodeDisplacements_Local;
        private List<double> _JNodeDisplacements_Local;
        private List<double> _INodeForces_Local;
        private List<double> _JNodeForces_Local;

        public List<double> INodeDisplacements_Global { get => _INodeDisplacements_Global; set => _INodeDisplacements_Global = value; }
        public List<double> JNodeDisplacements_Global { get => _JNodeDisplacements_Global; set => _JNodeDisplacements_Global = value; }
        public List<double> INodeForces_Global { get => _INodeForces_Global; set => _INodeForces_Global = value; }
        public List<double> JNodeForces_Global { get => _JNodeForces_Global; set => _JNodeForces_Global = value; }
        public List<double> INodeDisplacements_Local { get => _INodeDisplacements_Local; set => _INodeDisplacements_Local = value; }
        public List<double> JNodeDisplacements_Local { get => _JNodeDisplacements_Local; set => _JNodeDisplacements_Local = value; }
        public List<double> INodeForces_Local { get => _INodeForces_Local; set => _INodeForces_Local = value; }
        public List<double> JNodeForces_Local { get => _JNodeForces_Local; set => _JNodeForces_Local = value; }
    }
}
