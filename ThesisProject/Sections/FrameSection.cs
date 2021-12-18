using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThesisProject.Sections
{
    public class FrameSection : ISection
    {
        #region Ctor
        public FrameSection()
        {
            _SectionType = eSectionType.Frame;

            SetDefaultParameters();

            SetDefaultMaterial();
        }

        #endregion

        #region Private Fields

        private eSectionType _SectionType;
        private string _Label;
        private Material _Material;

        private double _I11;
        private double _I22;
        private double _J;
        private double _Area;


        #endregion


        #region Public Properties

        public double I11 { get => _I11; set => _I11 = value; }
        public double I22 { get => _I22; set => _I22 = value; }
        public double J { get => _J; set => _J = value; }
        public double Area { get => _Area; set => _Area = value; }
        #endregion


        #region Interface Implementations

        public string Label { get => _Label; set => _Label = value; }
        public eSectionType SectionStype { get => _SectionType; set => _SectionType = value; }
        public Material Material { get => _Material; set => _Material= value; }


        #endregion




        #region Public Methods
        

        public void SetDefaultParameters()
        {
            this.I11 = 1;
            this.I22 = 1;
            this.J = this.I11 + this.I22;
            this.Area = 1;
        }


        public void SetDefaultMaterial()
        {
            this.Material = new Material(1, 0.3);
            
        }
        
        #endregion

    }
}
