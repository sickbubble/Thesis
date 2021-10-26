namespace ThesisProjectUI
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.grpGeometry = new System.Windows.Forms.GroupBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.TabPageLatticeProperties = new System.Windows.Forms.TabPage();
            this.TabPageShellProperties = new System.Windows.Forms.TabPage();
            this.txtLx = new System.Windows.Forms.TextBox();
            this.lblLx = new System.Windows.Forms.Label();
            this.lblLy = new System.Windows.Forms.Label();
            this.txtLy = new System.Windows.Forms.TextBox();
            this.lblMeshSize = new System.Windows.Forms.Label();
            this.txtMeshSize = new System.Windows.Forms.TextBox();
            this.grpMaterial = new System.Windows.Forms.GroupBox();
            this.lblModulusOfRigidity = new System.Windows.Forms.Label();
            this.txtModulusOfRigidity = new System.Windows.Forms.TextBox();
            this.lblPoissons = new System.Windows.Forms.Label();
            this.txtPoissonsRatio = new System.Windows.Forms.TextBox();
            this.lblModulusOfElasticity = new System.Windows.Forms.Label();
            this.txtModulusOfElasticity = new System.Windows.Forms.TextBox();
            this.grpSectionProperties = new System.Windows.Forms.GroupBox();
            this.lbl_I12 = new System.Windows.Forms.Label();
            this.txtI12 = new System.Windows.Forms.TextBox();
            this.lbl_I22 = new System.Windows.Forms.Label();
            this.txtI22 = new System.Windows.Forms.TextBox();
            this.lbl_I11 = new System.Windows.Forms.Label();
            this.txtI11 = new System.Windows.Forms.TextBox();
            this.lblArea = new System.Windows.Forms.Label();
            this.txtArea = new System.Windows.Forms.TextBox();
            this.grpGeometry.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.TabPageLatticeProperties.SuspendLayout();
            this.grpMaterial.SuspendLayout();
            this.grpSectionProperties.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpGeometry
            // 
            this.grpGeometry.Controls.Add(this.lblMeshSize);
            this.grpGeometry.Controls.Add(this.txtMeshSize);
            this.grpGeometry.Controls.Add(this.lblLy);
            this.grpGeometry.Controls.Add(this.txtLy);
            this.grpGeometry.Controls.Add(this.lblLx);
            this.grpGeometry.Controls.Add(this.txtLx);
            this.grpGeometry.Location = new System.Drawing.Point(6, 6);
            this.grpGeometry.Name = "grpGeometry";
            this.grpGeometry.Size = new System.Drawing.Size(195, 125);
            this.grpGeometry.TabIndex = 0;
            this.grpGeometry.TabStop = false;
            this.grpGeometry.Text = "Geometry";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.TabPageLatticeProperties);
            this.tabControl1.Controls.Add(this.TabPageShellProperties);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(856, 466);
            this.tabControl1.TabIndex = 1;
            // 
            // TabPageLatticeProperties
            // 
            this.TabPageLatticeProperties.Controls.Add(this.grpSectionProperties);
            this.TabPageLatticeProperties.Controls.Add(this.grpMaterial);
            this.TabPageLatticeProperties.Controls.Add(this.grpGeometry);
            this.TabPageLatticeProperties.Location = new System.Drawing.Point(4, 22);
            this.TabPageLatticeProperties.Name = "TabPageLatticeProperties";
            this.TabPageLatticeProperties.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageLatticeProperties.Size = new System.Drawing.Size(848, 440);
            this.TabPageLatticeProperties.TabIndex = 0;
            this.TabPageLatticeProperties.Text = "Lattice Properties";
            this.TabPageLatticeProperties.UseVisualStyleBackColor = true;
            // 
            // TabPageShellProperties
            // 
            this.TabPageShellProperties.Location = new System.Drawing.Point(4, 22);
            this.TabPageShellProperties.Name = "TabPageShellProperties";
            this.TabPageShellProperties.Padding = new System.Windows.Forms.Padding(3);
            this.TabPageShellProperties.Size = new System.Drawing.Size(848, 440);
            this.TabPageShellProperties.TabIndex = 1;
            this.TabPageShellProperties.Text = "Shell Properties";
            this.TabPageShellProperties.UseVisualStyleBackColor = true;
            // 
            // txtLx
            // 
            this.txtLx.Location = new System.Drawing.Point(112, 19);
            this.txtLx.Name = "txtLx";
            this.txtLx.Size = new System.Drawing.Size(67, 20);
            this.txtLx.TabIndex = 0;
            // 
            // lblLx
            // 
            this.lblLx.AutoSize = true;
            this.lblLx.Location = new System.Drawing.Point(18, 26);
            this.lblLx.Name = "lblLx";
            this.lblLx.Size = new System.Drawing.Size(21, 13);
            this.lblLx.TabIndex = 1;
            this.lblLx.Text = "Lx:";
            // 
            // lblLy
            // 
            this.lblLy.AutoSize = true;
            this.lblLy.Location = new System.Drawing.Point(18, 52);
            this.lblLy.Name = "lblLy";
            this.lblLy.Size = new System.Drawing.Size(21, 13);
            this.lblLy.TabIndex = 3;
            this.lblLy.Text = "Ly:";
            // 
            // txtLy
            // 
            this.txtLy.Location = new System.Drawing.Point(112, 45);
            this.txtLy.Name = "txtLy";
            this.txtLy.Size = new System.Drawing.Size(67, 20);
            this.txtLy.TabIndex = 2;
            // 
            // lblMeshSize
            // 
            this.lblMeshSize.AutoSize = true;
            this.lblMeshSize.Location = new System.Drawing.Point(18, 78);
            this.lblMeshSize.Name = "lblMeshSize";
            this.lblMeshSize.Size = new System.Drawing.Size(59, 13);
            this.lblMeshSize.TabIndex = 5;
            this.lblMeshSize.Text = "Mesh Size:";
            // 
            // txtMeshSize
            // 
            this.txtMeshSize.Location = new System.Drawing.Point(112, 71);
            this.txtMeshSize.Name = "txtMeshSize";
            this.txtMeshSize.Size = new System.Drawing.Size(67, 20);
            this.txtMeshSize.TabIndex = 4;
            // 
            // grpMaterial
            // 
            this.grpMaterial.Controls.Add(this.lblModulusOfRigidity);
            this.grpMaterial.Controls.Add(this.txtModulusOfRigidity);
            this.grpMaterial.Controls.Add(this.lblPoissons);
            this.grpMaterial.Controls.Add(this.txtPoissonsRatio);
            this.grpMaterial.Controls.Add(this.lblModulusOfElasticity);
            this.grpMaterial.Controls.Add(this.txtModulusOfElasticity);
            this.grpMaterial.Location = new System.Drawing.Point(6, 137);
            this.grpMaterial.Name = "grpMaterial";
            this.grpMaterial.Size = new System.Drawing.Size(195, 125);
            this.grpMaterial.TabIndex = 1;
            this.grpMaterial.TabStop = false;
            this.grpMaterial.Text = "Material";
            // 
            // lblModulusOfRigidity
            // 
            this.lblModulusOfRigidity.AutoSize = true;
            this.lblModulusOfRigidity.Location = new System.Drawing.Point(18, 78);
            this.lblModulusOfRigidity.Name = "lblModulusOfRigidity";
            this.lblModulusOfRigidity.Size = new System.Drawing.Size(18, 13);
            this.lblModulusOfRigidity.TabIndex = 5;
            this.lblModulusOfRigidity.Text = "G:";
            // 
            // txtModulusOfRigidity
            // 
            this.txtModulusOfRigidity.Location = new System.Drawing.Point(112, 71);
            this.txtModulusOfRigidity.Name = "txtModulusOfRigidity";
            this.txtModulusOfRigidity.Size = new System.Drawing.Size(67, 20);
            this.txtModulusOfRigidity.TabIndex = 4;
            // 
            // lblPoissons
            // 
            this.lblPoissons.AutoSize = true;
            this.lblPoissons.Location = new System.Drawing.Point(18, 52);
            this.lblPoissons.Name = "lblPoissons";
            this.lblPoissons.Size = new System.Drawing.Size(82, 13);
            this.lblPoissons.TabIndex = 3;
            this.lblPoissons.Text = "Poisson\'s Ratio:";
            // 
            // txtPoissonsRatio
            // 
            this.txtPoissonsRatio.Location = new System.Drawing.Point(112, 45);
            this.txtPoissonsRatio.Name = "txtPoissonsRatio";
            this.txtPoissonsRatio.Size = new System.Drawing.Size(67, 20);
            this.txtPoissonsRatio.TabIndex = 2;
            // 
            // lblModulusOfElasticity
            // 
            this.lblModulusOfElasticity.AutoSize = true;
            this.lblModulusOfElasticity.Location = new System.Drawing.Point(18, 26);
            this.lblModulusOfElasticity.Name = "lblModulusOfElasticity";
            this.lblModulusOfElasticity.Size = new System.Drawing.Size(17, 13);
            this.lblModulusOfElasticity.TabIndex = 1;
            this.lblModulusOfElasticity.Text = "E:";
            // 
            // txtModulusOfElasticity
            // 
            this.txtModulusOfElasticity.Location = new System.Drawing.Point(112, 19);
            this.txtModulusOfElasticity.Name = "txtModulusOfElasticity";
            this.txtModulusOfElasticity.Size = new System.Drawing.Size(67, 20);
            this.txtModulusOfElasticity.TabIndex = 0;
            // 
            // grpSectionProperties
            // 
            this.grpSectionProperties.Controls.Add(this.lblArea);
            this.grpSectionProperties.Controls.Add(this.txtArea);
            this.grpSectionProperties.Controls.Add(this.lbl_I12);
            this.grpSectionProperties.Controls.Add(this.txtI12);
            this.grpSectionProperties.Controls.Add(this.lbl_I22);
            this.grpSectionProperties.Controls.Add(this.txtI22);
            this.grpSectionProperties.Controls.Add(this.lbl_I11);
            this.grpSectionProperties.Controls.Add(this.txtI11);
            this.grpSectionProperties.Location = new System.Drawing.Point(6, 268);
            this.grpSectionProperties.Name = "grpSectionProperties";
            this.grpSectionProperties.Size = new System.Drawing.Size(195, 125);
            this.grpSectionProperties.TabIndex = 2;
            this.grpSectionProperties.TabStop = false;
            this.grpSectionProperties.Text = "Section";
            // 
            // lbl_I12
            // 
            this.lbl_I12.AutoSize = true;
            this.lbl_I12.Location = new System.Drawing.Point(18, 78);
            this.lbl_I12.Name = "lbl_I12";
            this.lbl_I12.Size = new System.Drawing.Size(31, 13);
            this.lbl_I12.TabIndex = 5;
            this.lbl_I12.Text = "I_12:";
            // 
            // txtI12
            // 
            this.txtI12.Location = new System.Drawing.Point(112, 71);
            this.txtI12.Name = "txtI12";
            this.txtI12.Size = new System.Drawing.Size(67, 20);
            this.txtI12.TabIndex = 4;
            // 
            // lbl_I22
            // 
            this.lbl_I22.AutoSize = true;
            this.lbl_I22.Location = new System.Drawing.Point(18, 52);
            this.lbl_I22.Name = "lbl_I22";
            this.lbl_I22.Size = new System.Drawing.Size(31, 13);
            this.lbl_I22.TabIndex = 3;
            this.lbl_I22.Text = "I_22:";
            // 
            // txtI22
            // 
            this.txtI22.Location = new System.Drawing.Point(112, 45);
            this.txtI22.Name = "txtI22";
            this.txtI22.Size = new System.Drawing.Size(67, 20);
            this.txtI22.TabIndex = 2;
            // 
            // lbl_I11
            // 
            this.lbl_I11.AutoSize = true;
            this.lbl_I11.Location = new System.Drawing.Point(18, 26);
            this.lbl_I11.Name = "lbl_I11";
            this.lbl_I11.Size = new System.Drawing.Size(31, 13);
            this.lbl_I11.TabIndex = 1;
            this.lbl_I11.Text = "I_11:";
            // 
            // txtI11
            // 
            this.txtI11.Location = new System.Drawing.Point(112, 19);
            this.txtI11.Name = "txtI11";
            this.txtI11.Size = new System.Drawing.Size(67, 20);
            this.txtI11.TabIndex = 0;
            // 
            // lblArea
            // 
            this.lblArea.AutoSize = true;
            this.lblArea.Location = new System.Drawing.Point(18, 104);
            this.lblArea.Name = "lblArea";
            this.lblArea.Size = new System.Drawing.Size(35, 13);
            this.lblArea.TabIndex = 7;
            this.lblArea.Text = "Area: ";
            // 
            // txtArea
            // 
            this.txtArea.Location = new System.Drawing.Point(112, 97);
            this.txtArea.Name = "txtArea";
            this.txtArea.Size = new System.Drawing.Size(67, 20);
            this.txtArea.TabIndex = 6;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(880, 490);
            this.Controls.Add(this.tabControl1);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.grpGeometry.ResumeLayout(false);
            this.grpGeometry.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.TabPageLatticeProperties.ResumeLayout(false);
            this.grpMaterial.ResumeLayout(false);
            this.grpMaterial.PerformLayout();
            this.grpSectionProperties.ResumeLayout(false);
            this.grpSectionProperties.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpGeometry;
        private System.Windows.Forms.Label lblMeshSize;
        private System.Windows.Forms.TextBox txtMeshSize;
        private System.Windows.Forms.Label lblLy;
        private System.Windows.Forms.TextBox txtLy;
        private System.Windows.Forms.Label lblLx;
        private System.Windows.Forms.TextBox txtLx;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage TabPageLatticeProperties;
        private System.Windows.Forms.GroupBox grpSectionProperties;
        private System.Windows.Forms.Label lblArea;
        private System.Windows.Forms.TextBox txtArea;
        private System.Windows.Forms.Label lbl_I12;
        private System.Windows.Forms.TextBox txtI12;
        private System.Windows.Forms.Label lbl_I22;
        private System.Windows.Forms.TextBox txtI22;
        private System.Windows.Forms.Label lbl_I11;
        private System.Windows.Forms.TextBox txtI11;
        private System.Windows.Forms.GroupBox grpMaterial;
        private System.Windows.Forms.Label lblModulusOfRigidity;
        private System.Windows.Forms.TextBox txtModulusOfRigidity;
        private System.Windows.Forms.Label lblPoissons;
        private System.Windows.Forms.TextBox txtPoissonsRatio;
        private System.Windows.Forms.Label lblModulusOfElasticity;
        private System.Windows.Forms.TextBox txtModulusOfElasticity;
        private System.Windows.Forms.TabPage TabPageShellProperties;
    }
}

