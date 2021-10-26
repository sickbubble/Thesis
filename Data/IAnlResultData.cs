using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
