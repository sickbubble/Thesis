using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThesisProject
{
    public class Material
    {
        public Material(double E, double poissons)
        {
            BaseE = E;
            _E = E;
            _Poissons = poissons;
            _G = GetModulusOfRigidity(E,poissons);
            _Uw = 1;
        }

        #region Private Fields

        private double _E; // Modulus Of Elasticity
        private double _G; // Modulus Of Rigidity
        private double _Poissons; // Poisson's Ratio 
        private double _Uw; // Unit weigth

        private readonly double BaseE;// First Modulus Of Elasticity


        #endregion

        #region Public Properties
        public double E { get => _E; private set => _E = value; }
        public double G { get => _G; set => _G = value; }
        public double Poissons { get => _Poissons; set => _Poissons = value; }
        public double Uw { get => _Uw; set => _Uw = value; }

        #endregion

        #region Private Methods

        private double GetModulusOfRigidity( double E, double poissons)
        {
            var ret = E / (2 * (1 + poissons));

            return ret;

        }


        #endregion


        #region Public Methods

        public void SetBaseE()
        {
            _E = BaseE; 
        }

        public void SetE(double E)
        {
            _E = E;
        }


        #endregion
    }
}
