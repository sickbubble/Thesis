using Adapters;
using OptimizationAlgorithms.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThesisProject;

namespace Data
{
  
    /// <summary>
    /// Implement singleton to have only one optimal model
    /// </summary>
    public class ShellModelResultData : IAnlResultData
    {
        #region Ctor
        

        #endregion

        #region Singleton Implementation

        private ShellModelResultData() { }
        private static ShellModelResultData instance = null;
        public static ShellModelResultData Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ShellModelResultData();
                }
                return instance;
            }
        }
        public static void KillInstance()
        {
            instance = null;
        }

        public static bool IsInstanceValid() 
        {
            return (instance != null);
        }


        #endregion


        #region Public Properties


        #endregion

        #region Private Fields
        private MatrixCS _DispRes;

        private double _InternalEnergy;


        #endregion



        #region Interface Implementations
        public int AnalysisID { get; set; }
        public eAnalysisModelType AnalysisModelType { get; set; }
        /// <summary>
        /// // Node ID and ListOfResults
        /// </summary>
        public Dictionary<ModelInfo.Point, List<double>> NodeResults { get  ; set ; }
        public MatrixCS DispRes { get => _DispRes; set => _DispRes = value; }
        public double InternalEnergy { get => _InternalEnergy; set => _InternalEnergy = value; }

        public IParticle GetOptimizationObject()
        {
            throw new NotImplementedException();
        }


        #endregion
    }
}
