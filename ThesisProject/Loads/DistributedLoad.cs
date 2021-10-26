using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThesisProject.Structural_Members;

namespace ThesisProject.Loads
{
    class DistributedLoad: ILoad
    {
        #region Ctor

        public DistributedLoad()
        {

        }
        #endregion

        #region Private Fields

        private eLoadType _LoadType;
        private IStructuralMember _Member;
        private double _Magnitude;

        #endregion


        #region Public Properties

        public IStructuralMember Member { get => _Member; set => _Member = value; }
        public double Magnitude { get => _Magnitude; set => _Magnitude = value; }

        #endregion


        #region Interface Implementations
        public eLoadType LoadType { get => _LoadType; set => _LoadType = value; }


        #endregion

    }
}
