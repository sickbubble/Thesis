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

            //SetDefaultParameters();

            SetDefaultMaterial();
        }


        public FrameSection(double b, double h)
        { 
            //SetForGivenDimensions(b, h);

            SetSymmetricSection(h);

            SetDefaultMaterial();
        }

        #endregion

        #region Private Fields

         eSectionType _SectionType;
         string _Label;
         Material _Material;

         double _I11;
         double _EI;
         double _I22;
         double _J;
         double _Area;
         double _b;
         double _h;
        


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
        public double B { get => _b; set => _b = value; }
        public double H { get => _h; set => _h = value; }
        public double EI { get => _EI; set => _EI = value; }
       
        #endregion




        #region Public Methods
        public void SetForGivenDimensions(double b, double h)
        {
            _SectionType = eSectionType.Frame;

            _b = b;
            _h = h;

            double oneOverTw = 1.0 / 12;

            _Area = b * h;
            _I22 = oneOverTw * b * h * h * h;
            _I11 = oneOverTw * h * b * b * b;
            _J = _I11 + _I22;
        }

        public void SetSymmetricSection(double b)
        {
            _SectionType = eSectionType.Frame;

            _b = b;
            _h = b;

            double oneOverTw = 1.0 / 12;

            _Area = b * b;
            _I22 = oneOverTw * Math.Pow(b,4);
            _I11 = oneOverTw * Math.Pow(b,4);
            _J = _I11 + _I22;
        }


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
