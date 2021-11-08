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
        private Dictionary<int, FrameMemberResults> _FrameResults ; 
        


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

        #endregion
    }



    public class FrameMemberResults
    {
        public FrameMemberResults()
        {

        }


        private Dictionary<int, List<double>> _INodeResults;
        private Dictionary<int, List<double>> _JNodeResults;

        public Dictionary<int, List<double>> INodeResults { get => _INodeResults; set => _INodeResults = value; }
        public Dictionary<int, List<double>> JNodeResults { get => _JNodeResults; set => _JNodeResults = value; }
    }
}
