using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThesisProject.Structural_Members;

namespace ThesisProject.Loads
{
    public class PointLoad : ILoad
    {
        #region Ctor

        public PointLoad()
        {

        }
        #endregion

        #region Private Fields

        private eLoadType _LoadType;
        private Node _Node;
        private double _Magnitude;

        #endregion


        #region Public Properties

        public Node Node { get => _Node; set => _Node = value; }
        public double Magnitude { get => _Magnitude; set => _Magnitude = value; }

        #endregion


        #region Interface Implementations
        public eLoadType LoadType { get => _LoadType; set => _LoadType = value; }
        

        #endregion

    }
}
