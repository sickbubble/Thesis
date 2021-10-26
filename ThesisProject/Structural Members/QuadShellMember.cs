using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThesisProject.Sections;

namespace ThesisProject.Structural_Members
{
    public class QuadShellMember : IStructuralMember
    {
        #region Ctor
        public QuadShellMember()
        {

        }
        #endregion

        #region Private Fields

        private Node _IEndNode;
        private Node _JEndNode;
        private Node _KEndNode;
        private Node _LEndNode;
        private eMemberType _MemberType;
        private ISection _Section;


        #endregion

        #region Public Properties
        public Node IEndNode { get => _IEndNode; set => _IEndNode = value; }
        public Node JEndNode { get => _JEndNode; set => _JEndNode = value; }
        public Node KEndNode { get => _KEndNode; set => _KEndNode = value; }
        public Node LEndNode { get => _LEndNode; set => _LEndNode = value; }

        #endregion

        #region Interface Implementations
        public eMemberType MemberType { get => _MemberType; set => _MemberType = value; }
        public ISection Section { get => _Section; set => _Section = value; }

        public void GetGlobalStiffnessMatrix()
        {
        }

        public void GetLocalStiffnessMatrix()
        {
            throw new NotImplementedException();
        }

        MatrixCS IStructuralMember.GetLocalStiffnessMatrix()
        {
            throw new NotImplementedException();
        }

        MatrixCS IStructuralMember.GetGlobalStiffnessMatrix()
        {
            throw new NotImplementedException();
        }

        public MatrixCS GetRotationMatrix()
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}
