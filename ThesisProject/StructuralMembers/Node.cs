using ModelInfo;
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

        private int _ID;
        private Support _SupportCondition;

        private Point _Point;

        
        #endregion

        #region Public Properties
       

        
        /// <summary>
        /// One based.
        /// </summary>
        public int ID { get => _ID; set => _ID = value; }
        public Support SupportCondition { get => _SupportCondition; set => _SupportCondition = value; }
        public Point Point { get => _Point; set => _Point = value; }

        #endregion

    }

}
