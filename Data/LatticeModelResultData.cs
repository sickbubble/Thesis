using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private Dictionary<int, FrameMemberResults> _LocalFrameResults ;
        private Dictionary<int, FrameMemberResults> _GlobalFrameResults;





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
        public Dictionary<int, FrameMemberResults> LocalFrameResults { get => _LocalFrameResults; set => _LocalFrameResults = value; }
        public Dictionary<int, FrameMemberResults> GlobalFrameResults { get => _GlobalFrameResults; set => _GlobalFrameResults = value; }

        #endregion
    }



    public class FrameMemberResults
    {
        public FrameMemberResults()
        {

        }


        private List<double> _INodeDisplacements;
        private List<double> _JNodeDisplacements;
        private List<double> _INodeForces;
        private List<double> _JNodeForces;


        public  List<double> INodeDisplacements { get => _INodeDisplacements; set => _INodeDisplacements = value; }
        public  List<double> JNodeDisplacements { get => _JNodeDisplacements; set => _JNodeDisplacements = value; }
        public  List<double> INodeForces { get => _INodeForces; set => _INodeForces = value; }
        public  List<double> JNodeForces { get => _JNodeForces; set => _JNodeForces = value; }
    }
}
