using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ThesisProject.Sections
{
    public enum eSectionType
    {
        QuadShell = 0,
        Frame = 1
    }
    public interface ISection
    {
        eSectionType SectionStype { get; set;}

        string Label { get; set; }

        Material Material { get; set; }
    }
}
