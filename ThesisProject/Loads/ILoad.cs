using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThesisProject.Loads
{
    public enum eLoadType
    {
        Point =0,
        Distributed = 1,
        Area = 2
    }
    public enum eLoadingType
    {
        PointLoad,
        FullAreaLoad
    }
    public interface ILoad
    {
        eLoadType LoadType { get; set; }
    }
}
