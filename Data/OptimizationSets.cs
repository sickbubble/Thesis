using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class ThesisDataContainer
    {

        public  Dictionary<int, LatticeModelData> LatticeModels ;


        #region Singleton Implementation

        private ThesisDataContainer() { }
        private static ThesisDataContainer instance = null;
        public static ThesisDataContainer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ThesisDataContainer();
                }
                return instance;
            }
        }

        public static bool IsInstanceValid()
        {
            return (instance != null);
        }


        #endregion
    }
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
