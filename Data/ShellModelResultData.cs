using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThesisProject;

namespace Data
{
    public class ShellModelResultData : IAnlResultData
    {
        #region Ctor
        public ShellModelResultData()
        {

        }
        #endregion


        #region Public Properties


        #endregion

        #region Private Fields
        private MatrixCS _DispRes;

        #endregion



        #region Interface Implementations
        public int AnalysisID { get; set; }
        public eAnalysisModelType AnalysisModelType { get; set; }
        /// <summary>
        /// // Node ID and ListOfResults
        /// </summary>
        public Dictionary<int, List<double>> NodeResults { get  ; set ; }
        public MatrixCS DispRes { get => _DispRes; set => _DispRes = value; }


        #endregion
    }
}
