
using Adapters;
using OptimizationAlgorithms.Particles;

namespace Data
{
    public enum eAnalysisModelType
    {
        Lattice = 0,
        Shell = 1
    }
    public interface IAnlResultData 
    {
        int AnalysisID { get; set; }
        eAnalysisModelType AnalysisModelType { get; set; }


    }
}
