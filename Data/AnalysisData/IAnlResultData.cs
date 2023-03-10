
using Adapters;

namespace Data
{
    public enum eAnalysisModelType
    {
        Lattice = 0,
        Shell = 1
    }
    public interface IAnlResultData : IOptimizationAdaptee<IOptimizationObject>
    {
        int AnalysisID { get; set; }
        eAnalysisModelType AnalysisModelType { get; set; }


    }
}
