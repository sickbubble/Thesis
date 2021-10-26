using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThesisProject.Structural_Members
{
    public enum eRestrainedCondition
    {
        Restrained = 0,
        NotRestrained = 1
    }

    public enum eSupportType
    {
        Pinned = 0,
        Fixed = 1,
        Free = 2
    }

    public class Support
    {
        public Support(eSupportType supportType)
        {

            switch (supportType)
            {
                case eSupportType.Pinned:
                    SetPinned();
                    break;
                case eSupportType.Fixed:
                    SetFixed();
                    break;
                case eSupportType.Free:
                    SetFree();
                    break;
            }


        }

        #region Private Fields

        Dictionary<int, eRestrainedCondition> _Restraints;

        #endregion

        #region Public Properties

        public Dictionary<int, eRestrainedCondition> Restraints { get => _Restraints; set => _Restraints = value; }

        #endregion

        #region Private Methods


        #endregion


        #region PublicMethods


        public void SetPinned()
        {
            _Restraints = new Dictionary<int, eRestrainedCondition>();

            for (int i = 0; i < 3; i++)
            {
                _Restraints.Add(i, eRestrainedCondition.Restrained);
            }
            for (int i = 3; i < 6; i++)
            {
                _Restraints.Add(i, eRestrainedCondition.NotRestrained);
            }
        }

        public void SetFixed()
        {
            _Restraints = new Dictionary<int, eRestrainedCondition>();
            for (int i = 0; i < 6; i++)
            {
                _Restraints.Add(i, eRestrainedCondition.Restrained);
            }
        }

        public void SetFree()
        {
            _Restraints = new Dictionary<int, eRestrainedCondition>();
            for (int i = 0; i < 6; i++)
            {
                _Restraints.Add(i, eRestrainedCondition.NotRestrained);
            }
        }


        #endregion
    }
}
