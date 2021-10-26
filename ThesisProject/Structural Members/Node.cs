using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThesisProject.Structural_Members
{
   public class Node
    {
        public Node()
        {

        }

        #region Private Fields
        private double _X;
        private double _Y;
        private double _Z;
        private int _ID;
        private Support _SupportCondition;

        #endregion

        #region Public Properties
        public double X { get => _X; set => _X = value; }
        public double Y { get => _Y; set => _Y = value; }
        public double Z { get => _Z; set => _Z = value; }
        /// <summary>
        /// One based.
        /// </summary>
        public int ID { get => _ID; set => _ID = value; }
        public Support SupportCondition { get => _SupportCondition; set => _SupportCondition = value; }

        #endregion


    }

}
