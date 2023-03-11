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
        int ID { get; set; }
        eMemberType MemberType { get; set; }

        MatrixCS GetLocalStiffnessMatrix(bool useEI = false);

        MatrixCS GetGlobalStiffnessMatrix();

        MatrixCS GetRotationMatrix();
    }
}
