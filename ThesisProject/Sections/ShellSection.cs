﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThesisProject.Sections
{
    public class ShellSection : ISection
    {
        #region Ctor
        public ShellSection()
        {

        }

        #endregion

        #region Private Fields

        private double _Thickness;
        private eSectionType _SectionType;
        private string _Label;
        private Material _Material;


        #endregion


        #region Public Properties
        public double Thickness { get => _Thickness; set => _Thickness = value; }
        #endregion


        #region Interface Implementations

        public string Label { get => _Label; set => _Label = value; }
        public eSectionType SectionStype { get => _SectionType; set => _SectionType = value; }
        public Material Material { get => _Material; set => _Material = value; }
        #endregion


    }
}
