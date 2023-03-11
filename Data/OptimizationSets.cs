using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public enum eEndConditionSet
    {
        AllFixed = 1,
        TorsionalRelease = 2,
        WeakSideRotation = 3,
        TorsionalReleaseAndWeakSide = 4
    }

    public enum eHorizon
    {
        LightMesh = 1,
        DenseMesh = 2
    }
}
