﻿namespace BMP2TileGUI
{
    sealed partial class Form1
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.tbFilename = new System.Windows.Forms.TextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.pbPreview = new System.Windows.Forms.PictureBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnSave = new System.Windows.Forms.Button();
            this.cbPlanar = new System.Windows.Forms.CheckBox();
            this.cb8x16 = new System.Windows.Forms.CheckBox();
            this.cbUseMirroring = new System.Windows.Forms.CheckBox();
            this.cbRemoveDuplicates = new System.Windows.Forms.CheckBox();
            this.tbTiles = new System.Windows.Forms.TextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tbFirstTileIndex = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSaveTilemap = new System.Windows.Forms.Button();
            this.cbHighPriority = new System.Windows.Forms.CheckBox();
            this.cbSpritePalette = new System.Windows.Forms.CheckBox();
            this.tbTilemap = new System.Windows.Forms.TextBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.rbHexGG = new System.Windows.Forms.RadioButton();
            this.rbHexSMS = new System.Windows.Forms.RadioButton();
            this.cbPaletteConstants = new System.Windows.Forms.CheckBox();
            this.pbPalette = new System.Windows.Forms.PictureBox();
            this.btnSavePalette = new System.Windows.Forms.Button();
            this.cbFullPalette = new System.Windows.Forms.CheckBox();
            this.tbPalette = new System.Windows.Forms.TextBox();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.tbMessages = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPreview)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPalette)).BeginInit();
            this.tabPage5.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnBrowse);
            this.groupBox1.Controls.Add(this.tbFilename);
            this.groupBox1.Location = new System.Drawing.Point(13, 14);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Size = new System.Drawing.Size(1411, 101);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Bitmap";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.Location = new System.Drawing.Point(1229, 37);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(162, 49);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "&Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // tbFilename
            // 
            this.tbFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilename.Location = new System.Drawing.Point(9, 42);
            this.tbFilename.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbFilename.Name = "tbFilename";
            this.tbFilename.ReadOnly = true;
            this.tbFilename.Size = new System.Drawing.Size(1212, 39);
            this.tbFilename.TabIndex = 0;
            this.tbFilename.Text = "Drag and drop a BMP or PNG file or click ->";
            // 
            // statusStrip1
            // 
            this.statusStrip1.AutoSize = false;
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 625);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 15, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1437, 61);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 0);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Location = new System.Drawing.Point(13, 125);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1411, 495);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.pbPreview);
            this.tabPage1.Location = new System.Drawing.Point(8, 46);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabPage1.Size = new System.Drawing.Size(1395, 441);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Source";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // pbPreview
            // 
            this.pbPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbPreview.Location = new System.Drawing.Point(4, 5);
            this.pbPreview.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pbPreview.Name = "pbPreview";
            this.pbPreview.Size = new System.Drawing.Size(1387, 431);
            this.pbPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbPreview.TabIndex = 0;
            this.pbPreview.TabStop = false;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btnSave);
            this.tabPage2.Controls.Add(this.cbPlanar);
            this.tabPage2.Controls.Add(this.cb8x16);
            this.tabPage2.Controls.Add(this.cbUseMirroring);
            this.tabPage2.Controls.Add(this.cbRemoveDuplicates);
            this.tabPage2.Controls.Add(this.tbTiles);
            this.tabPage2.Location = new System.Drawing.Point(8, 46);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(8);
            this.tabPage2.Size = new System.Drawing.Size(1395, 441);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Tiles";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(1219, 369);
            this.btnSave.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(162, 56);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "&Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // cbPlanar
            // 
            this.cbPlanar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbPlanar.AutoSize = true;
            this.cbPlanar.Checked = true;
            this.cbPlanar.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbPlanar.Location = new System.Drawing.Point(274, 389);
            this.cbPlanar.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.cbPlanar.Name = "cbPlanar";
            this.cbPlanar.Size = new System.Drawing.Size(230, 36);
            this.cbPlanar.TabIndex = 4;
            this.cbPlanar.Text = "&Planar tile output";
            this.cbPlanar.UseVisualStyleBackColor = true;
            this.cbPlanar.CheckedChanged += new System.EventHandler(this.ControlChanged);
            // 
            // cb8x16
            // 
            this.cb8x16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cb8x16.AutoSize = true;
            this.cb8x16.Location = new System.Drawing.Point(274, 337);
            this.cb8x16.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.cb8x16.Name = "cb8x16";
            this.cb8x16.Size = new System.Drawing.Size(189, 36);
            this.cb8x16.TabIndex = 3;
            this.cb8x16.Text = "Treat as 8×1&6";
            this.cb8x16.UseVisualStyleBackColor = true;
            this.cb8x16.CheckedChanged += new System.EventHandler(this.ControlChanged);
            // 
            // cbUseMirroring
            // 
            this.cbUseMirroring.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbUseMirroring.AutoSize = true;
            this.cbUseMirroring.Checked = true;
            this.cbUseMirroring.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbUseMirroring.Location = new System.Drawing.Point(14, 389);
            this.cbUseMirroring.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.cbUseMirroring.Name = "cbUseMirroring";
            this.cbUseMirroring.Size = new System.Drawing.Size(231, 36);
            this.cbUseMirroring.TabIndex = 2;
            this.cbUseMirroring.Text = "&Use tile mirroring";
            this.cbUseMirroring.UseVisualStyleBackColor = true;
            this.cbUseMirroring.CheckedChanged += new System.EventHandler(this.ControlChanged);
            // 
            // cbRemoveDuplicates
            // 
            this.cbRemoveDuplicates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbRemoveDuplicates.AutoSize = true;
            this.cbRemoveDuplicates.Checked = true;
            this.cbRemoveDuplicates.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbRemoveDuplicates.Location = new System.Drawing.Point(15, 337);
            this.cbRemoveDuplicates.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.cbRemoveDuplicates.Name = "cbRemoveDuplicates";
            this.cbRemoveDuplicates.Size = new System.Drawing.Size(247, 36);
            this.cbRemoveDuplicates.TabIndex = 1;
            this.cbRemoveDuplicates.Text = "&Remove duplicates";
            this.cbRemoveDuplicates.UseVisualStyleBackColor = true;
            this.cbRemoveDuplicates.CheckedChanged += new System.EventHandler(this.ControlChanged);
            // 
            // tbTiles
            // 
            this.tbTiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbTiles.Font = new System.Drawing.Font("Consolas", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbTiles.Location = new System.Drawing.Point(15, 13);
            this.tbTiles.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbTiles.Multiline = true;
            this.tbTiles.Name = "tbTiles";
            this.tbTiles.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbTiles.Size = new System.Drawing.Size(1366, 274);
            this.tbTiles.TabIndex = 0;
            this.tbTiles.WordWrap = false;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.tbFirstTileIndex);
            this.tabPage3.Controls.Add(this.label1);
            this.tabPage3.Controls.Add(this.btnSaveTilemap);
            this.tabPage3.Controls.Add(this.cbHighPriority);
            this.tabPage3.Controls.Add(this.cbSpritePalette);
            this.tabPage3.Controls.Add(this.tbTilemap);
            this.tabPage3.Location = new System.Drawing.Point(8, 46);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(8);
            this.tabPage3.Size = new System.Drawing.Size(1395, 441);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Tilemap";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tbFirstTileIndex
            // 
            this.tbFirstTileIndex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbFirstTileIndex.Location = new System.Drawing.Point(458, 334);
            this.tbFirstTileIndex.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.tbFirstTileIndex.MaxLength = 4;
            this.tbFirstTileIndex.Name = "tbFirstTileIndex";
            this.tbFirstTileIndex.Size = new System.Drawing.Size(73, 39);
            this.tbFirstTileIndex.TabIndex = 4;
            this.tbFirstTileIndex.Text = "0";
            this.tbFirstTileIndex.TextChanged += new System.EventHandler(this.ControlChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(258, 337);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(188, 32);
            this.label1.TabIndex = 3;
            this.label1.Text = "&Index of first tile";
            // 
            // btnSaveTilemap
            // 
            this.btnSaveTilemap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveTilemap.Location = new System.Drawing.Point(1219, 369);
            this.btnSaveTilemap.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.btnSaveTilemap.Name = "btnSaveTilemap";
            this.btnSaveTilemap.Size = new System.Drawing.Size(162, 56);
            this.btnSaveTilemap.TabIndex = 5;
            this.btnSaveTilemap.Text = "&Save";
            this.btnSaveTilemap.UseVisualStyleBackColor = true;
            this.btnSaveTilemap.Click += new System.EventHandler(this.btnSaveTilemap_Click);
            // 
            // cbHighPriority
            // 
            this.cbHighPriority.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbHighPriority.AutoSize = true;
            this.cbHighPriority.Location = new System.Drawing.Point(14, 389);
            this.cbHighPriority.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.cbHighPriority.Name = "cbHighPriority";
            this.cbHighPriority.Size = new System.Drawing.Size(230, 36);
            this.cbHighPriority.TabIndex = 2;
            this.cbHighPriority.Text = "In &front of sprites";
            this.cbHighPriority.UseVisualStyleBackColor = true;
            this.cbHighPriority.CheckedChanged += new System.EventHandler(this.ControlChanged);
            // 
            // cbSpritePalette
            // 
            this.cbSpritePalette.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbSpritePalette.AutoSize = true;
            this.cbSpritePalette.Location = new System.Drawing.Point(14, 337);
            this.cbSpritePalette.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.cbSpritePalette.Name = "cbSpritePalette";
            this.cbSpritePalette.Size = new System.Drawing.Size(232, 36);
            this.cbSpritePalette.TabIndex = 1;
            this.cbSpritePalette.Text = "&Use sprite palette";
            this.cbSpritePalette.UseVisualStyleBackColor = true;
            this.cbSpritePalette.CheckedChanged += new System.EventHandler(this.ControlChanged);
            // 
            // tbTilemap
            // 
            this.tbTilemap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbTilemap.Font = new System.Drawing.Font("Consolas", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbTilemap.Location = new System.Drawing.Point(12, 13);
            this.tbTilemap.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbTilemap.Multiline = true;
            this.tbTilemap.Name = "tbTilemap";
            this.tbTilemap.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbTilemap.Size = new System.Drawing.Size(1371, 308);
            this.tbTilemap.TabIndex = 0;
            this.tbTilemap.WordWrap = false;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.rbHexGG);
            this.tabPage4.Controls.Add(this.rbHexSMS);
            this.tabPage4.Controls.Add(this.cbPaletteConstants);
            this.tabPage4.Controls.Add(this.pbPalette);
            this.tabPage4.Controls.Add(this.btnSavePalette);
            this.tabPage4.Controls.Add(this.cbFullPalette);
            this.tabPage4.Controls.Add(this.tbPalette);
            this.tabPage4.Location = new System.Drawing.Point(8, 46);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(8);
            this.tabPage4.Size = new System.Drawing.Size(1395, 441);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Palette";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // rbHexGG
            // 
            this.rbHexGG.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rbHexGG.AutoSize = true;
            this.rbHexGG.Location = new System.Drawing.Point(14, 388);
            this.rbHexGG.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.rbHexGG.Name = "rbHexGG";
            this.rbHexGG.Size = new System.Drawing.Size(213, 36);
            this.rbHexGG.TabIndex = 2;
            this.rbHexGG.Text = "&GG (12-bit RGB)";
            this.rbHexGG.UseVisualStyleBackColor = true;
            this.rbHexGG.CheckedChanged += new System.EventHandler(this.ControlChanged);
            // 
            // rbHexSMS
            // 
            this.rbHexSMS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rbHexSMS.AutoSize = true;
            this.rbHexSMS.Checked = true;
            this.rbHexSMS.Location = new System.Drawing.Point(11, 336);
            this.rbHexSMS.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.rbHexSMS.Name = "rbHexSMS";
            this.rbHexSMS.Size = new System.Drawing.Size(216, 36);
            this.rbHexSMS.TabIndex = 0;
            this.rbHexSMS.TabStop = true;
            this.rbHexSMS.Text = "S&MS (6-bit RGB)";
            this.rbHexSMS.UseVisualStyleBackColor = true;
            this.rbHexSMS.CheckedChanged += new System.EventHandler(this.ControlChanged);
            // 
            // cbPaletteConstants
            // 
            this.cbPaletteConstants.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbPaletteConstants.AutoSize = true;
            this.cbPaletteConstants.Location = new System.Drawing.Point(242, 337);
            this.cbPaletteConstants.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.cbPaletteConstants.Name = "cbPaletteConstants";
            this.cbPaletteConstants.Size = new System.Drawing.Size(270, 36);
            this.cbPaletteConstants.TabIndex = 10;
            this.cbPaletteConstants.Text = "Use &constants (cl123)";
            this.cbPaletteConstants.UseVisualStyleBackColor = true;
            this.cbPaletteConstants.CheckedChanged += new System.EventHandler(this.ControlChanged);
            // 
            // pbPalette
            // 
            this.pbPalette.Location = new System.Drawing.Point(11, 13);
            this.pbPalette.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.pbPalette.Name = "pbPalette";
            this.pbPalette.Size = new System.Drawing.Size(1417, 96);
            this.pbPalette.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbPalette.TabIndex = 8;
            this.pbPalette.TabStop = false;
            // 
            // btnSavePalette
            // 
            this.btnSavePalette.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSavePalette.Location = new System.Drawing.Point(1221, 368);
            this.btnSavePalette.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.btnSavePalette.Name = "btnSavePalette";
            this.btnSavePalette.Size = new System.Drawing.Size(162, 56);
            this.btnSavePalette.TabIndex = 4;
            this.btnSavePalette.Text = "&Save";
            this.btnSavePalette.UseVisualStyleBackColor = true;
            this.btnSavePalette.Click += new System.EventHandler(this.btnSavePalette_Click);
            // 
            // cbFullPalette
            // 
            this.cbFullPalette.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbFullPalette.AutoSize = true;
            this.cbFullPalette.Location = new System.Drawing.Point(242, 389);
            this.cbFullPalette.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.cbFullPalette.Name = "cbFullPalette";
            this.cbFullPalette.Size = new System.Drawing.Size(352, 36);
            this.cbFullPalette.TabIndex = 3;
            this.cbFullPalette.Text = "Always emit &16 or 32 colours";
            this.cbFullPalette.UseVisualStyleBackColor = true;
            this.cbFullPalette.CheckedChanged += new System.EventHandler(this.ControlChanged);
            // 
            // tbPalette
            // 
            this.tbPalette.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbPalette.Font = new System.Drawing.Font("Consolas", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbPalette.Location = new System.Drawing.Point(19, 122);
            this.tbPalette.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tbPalette.Multiline = true;
            this.tbPalette.Name = "tbPalette";
            this.tbPalette.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbPalette.Size = new System.Drawing.Size(1364, 201);
            this.tbPalette.TabIndex = 0;
            this.tbPalette.WordWrap = false;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.tbMessages);
            this.tabPage5.Location = new System.Drawing.Point(8, 46);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(8);
            this.tabPage5.Size = new System.Drawing.Size(1395, 441);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Messages";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // tbMessages
            // 
            this.tbMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbMessages.Location = new System.Drawing.Point(8, 8);
            this.tbMessages.Multiline = true;
            this.tbMessages.Name = "tbMessages";
            this.tbMessages.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbMessages.Size = new System.Drawing.Size(1379, 425);
            this.tbMessages.TabIndex = 0;
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1437, 686);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Form1";
            this.Text = "Bitmap to SMS/GG tile converter 0.62 by Maxim :: smspower.org";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbPreview)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPalette)).EndInit();
            this.tabPage5.ResumeLayout(false);
            this.tabPage5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox tbFilename;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.PictureBox pbPreview;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox tbTiles;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.CheckBox cbPlanar;
        private System.Windows.Forms.CheckBox cb8x16;
        private System.Windows.Forms.CheckBox cbUseMirroring;
        private System.Windows.Forms.CheckBox cbRemoveDuplicates;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button btnSaveTilemap;
        private System.Windows.Forms.CheckBox cbHighPriority;
        private System.Windows.Forms.CheckBox cbSpritePalette;
        private System.Windows.Forms.TextBox tbTilemap;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.RadioButton rbHexGG;
        private System.Windows.Forms.RadioButton rbHexSMS;
        private System.Windows.Forms.PictureBox pbPalette;
        private System.Windows.Forms.Button btnSavePalette;
        private System.Windows.Forms.CheckBox cbFullPalette;
        private System.Windows.Forms.TextBox tbPalette;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.TextBox tbMessages;
        private System.Windows.Forms.TextBox tbFirstTileIndex;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbPaletteConstants;
    }
}

