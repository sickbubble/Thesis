using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThesisProject.Sections;

namespace ThesisProject
{
    public enum eMemberType
    {
        QuadShell = 0,
        Frame = 1
    }
    public interface IStructuralMember
    {

        eMemberType MemberType { get; set; }
        ISection Section { get; set; }

        MatrixCS GetLocalStiffnessMatrix();

        MatrixCS GetGlobalStiffnessMatrix();

        MatrixCS GetRotationMatrix();
    }
}
