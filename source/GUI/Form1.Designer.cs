namespace BMP2TileGUI
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            groupBox1 = new System.Windows.Forms.GroupBox();
            btnBrowse = new System.Windows.Forms.Button();
            tbFilename = new System.Windows.Forms.TextBox();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabPage1 = new System.Windows.Forms.TabPage();
            pbPreview = new System.Windows.Forms.PictureBox();
            tabPage2 = new System.Windows.Forms.TabPage();
            btnSave = new System.Windows.Forms.Button();
            cbPlanar = new System.Windows.Forms.CheckBox();
            cb8x16 = new System.Windows.Forms.CheckBox();
            cbUseMirroring = new System.Windows.Forms.CheckBox();
            cbRemoveDuplicates = new System.Windows.Forms.CheckBox();
            tbTiles = new System.Windows.Forms.TextBox();
            tabPage3 = new System.Windows.Forms.TabPage();
            tbFirstTileIndex = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            btnSaveTilemap = new System.Windows.Forms.Button();
            cbHighPriority = new System.Windows.Forms.CheckBox();
            cbSpritePalette = new System.Windows.Forms.CheckBox();
            tbTilemap = new System.Windows.Forms.TextBox();
            tabPage4 = new System.Windows.Forms.TabPage();
            rbHexGG = new System.Windows.Forms.RadioButton();
            rbHexSMS = new System.Windows.Forms.RadioButton();
            cbPaletteConstants = new System.Windows.Forms.CheckBox();
            pbPalette = new System.Windows.Forms.PictureBox();
            btnSavePalette = new System.Windows.Forms.Button();
            cbFullPalette = new System.Windows.Forms.CheckBox();
            tbPalette = new System.Windows.Forms.TextBox();
            tabPage5 = new System.Windows.Forms.TabPage();
            tbMessages = new System.Windows.Forms.TextBox();
            imageList1 = new System.Windows.Forms.ImageList(components);
            groupBox1.SuspendLayout();
            statusStrip1.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbPreview).BeginInit();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbPalette).BeginInit();
            tabPage5.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupBox1.Controls.Add(btnBrowse);
            groupBox1.Controls.Add(tbFilename);
            groupBox1.Location = new System.Drawing.Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(643, 58);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Bitmap";
            // 
            // btnBrowse
            // 
            btnBrowse.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnBrowse.Location = new System.Drawing.Point(558, 25);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new System.Drawing.Size(77, 23);
            btnBrowse.TabIndex = 1;
            btnBrowse.Text = "&Browse";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // tbFilename
            // 
            tbFilename.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbFilename.Location = new System.Drawing.Point(8, 26);
            tbFilename.Name = "tbFilename";
            tbFilename.ReadOnly = true;
            tbFilename.Size = new System.Drawing.Size(542, 23);
            tbFilename.TabIndex = 0;
            tbFilename.Text = "Drag and drop a BMP or PNG file or click ->";
            // 
            // statusStrip1
            // 
            statusStrip1.AutoSize = false;
            statusStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { lblStatus });
            statusStrip1.Location = new System.Drawing.Point(0, 369);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new System.Drawing.Size(667, 22);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(0, 17);
            // 
            // tabControl1
            // 
            tabControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Controls.Add(tabPage5);
            tabControl1.ImageList = imageList1;
            tabControl1.Location = new System.Drawing.Point(12, 82);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(643, 284);
            tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(pbPreview);
            tabPage1.ImageKey = "picture.png";
            tabPage1.Location = new System.Drawing.Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Size = new System.Drawing.Size(635, 256);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Source";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // pbPreview
            // 
            pbPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            pbPreview.Location = new System.Drawing.Point(0, 0);
            pbPreview.Name = "pbPreview";
            pbPreview.Size = new System.Drawing.Size(635, 256);
            pbPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pbPreview.TabIndex = 0;
            pbPreview.TabStop = false;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(btnSave);
            tabPage2.Controls.Add(cbPlanar);
            tabPage2.Controls.Add(cb8x16);
            tabPage2.Controls.Add(cbUseMirroring);
            tabPage2.Controls.Add(cbRemoveDuplicates);
            tabPage2.Controls.Add(tbTiles);
            tabPage2.ImageKey = "application_view_tile.png";
            tabPage2.Location = new System.Drawing.Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Size = new System.Drawing.Size(635, 256);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Tiles";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            btnSave.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnSave.Location = new System.Drawing.Point(551, 225);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(81, 28);
            btnSave.TabIndex = 5;
            btnSave.Text = "&Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // cbPlanar
            // 
            cbPlanar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            cbPlanar.AutoSize = true;
            cbPlanar.Checked = true;
            cbPlanar.CheckState = System.Windows.Forms.CheckState.Checked;
            cbPlanar.Location = new System.Drawing.Point(135, 231);
            cbPlanar.Name = "cbPlanar";
            cbPlanar.Size = new System.Drawing.Size(117, 19);
            cbPlanar.TabIndex = 4;
            cbPlanar.Text = "&Planar tile output";
            cbPlanar.UseVisualStyleBackColor = true;
            cbPlanar.CheckedChanged += ControlChanged;
            // 
            // cb8x16
            // 
            cb8x16.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            cb8x16.AutoSize = true;
            cb8x16.Location = new System.Drawing.Point(135, 206);
            cb8x16.Name = "cb8x16";
            cb8x16.Size = new System.Drawing.Size(95, 19);
            cb8x16.TabIndex = 3;
            cb8x16.Text = "Treat as 8×1&6";
            cb8x16.UseVisualStyleBackColor = true;
            cb8x16.CheckedChanged += ControlChanged;
            // 
            // cbUseMirroring
            // 
            cbUseMirroring.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            cbUseMirroring.AutoSize = true;
            cbUseMirroring.Checked = true;
            cbUseMirroring.CheckState = System.Windows.Forms.CheckState.Checked;
            cbUseMirroring.Location = new System.Drawing.Point(4, 231);
            cbUseMirroring.Name = "cbUseMirroring";
            cbUseMirroring.Size = new System.Drawing.Size(117, 19);
            cbUseMirroring.TabIndex = 2;
            cbUseMirroring.Text = "&Use tile mirroring";
            cbUseMirroring.UseVisualStyleBackColor = true;
            cbUseMirroring.CheckedChanged += ControlChanged;
            // 
            // cbRemoveDuplicates
            // 
            cbRemoveDuplicates.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            cbRemoveDuplicates.AutoSize = true;
            cbRemoveDuplicates.Checked = true;
            cbRemoveDuplicates.CheckState = System.Windows.Forms.CheckState.Checked;
            cbRemoveDuplicates.Location = new System.Drawing.Point(4, 206);
            cbRemoveDuplicates.Name = "cbRemoveDuplicates";
            cbRemoveDuplicates.Size = new System.Drawing.Size(126, 19);
            cbRemoveDuplicates.TabIndex = 1;
            cbRemoveDuplicates.Text = "&Remove duplicates";
            cbRemoveDuplicates.UseVisualStyleBackColor = true;
            cbRemoveDuplicates.CheckedChanged += ControlChanged;
            // 
            // tbTiles
            // 
            tbTiles.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbTiles.Font = new System.Drawing.Font("Consolas", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            tbTiles.Location = new System.Drawing.Point(3, 3);
            tbTiles.Multiline = true;
            tbTiles.Name = "tbTiles";
            tbTiles.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            tbTiles.Size = new System.Drawing.Size(629, 197);
            tbTiles.TabIndex = 0;
            tbTiles.WordWrap = false;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(tbFirstTileIndex);
            tabPage3.Controls.Add(label1);
            tabPage3.Controls.Add(btnSaveTilemap);
            tabPage3.Controls.Add(cbHighPriority);
            tabPage3.Controls.Add(cbSpritePalette);
            tabPage3.Controls.Add(tbTilemap);
            tabPage3.ImageKey = "map.png";
            tabPage3.Location = new System.Drawing.Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new System.Drawing.Size(635, 256);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Tilemap";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // tbFirstTileIndex
            // 
            tbFirstTileIndex.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            tbFirstTileIndex.Location = new System.Drawing.Point(232, 204);
            tbFirstTileIndex.MaxLength = 4;
            tbFirstTileIndex.Name = "tbFirstTileIndex";
            tbFirstTileIndex.Size = new System.Drawing.Size(73, 23);
            tbFirstTileIndex.TabIndex = 4;
            tbFirstTileIndex.Text = "0";
            tbFirstTileIndex.TextChanged += ControlChanged;
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(135, 207);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(91, 15);
            label1.TabIndex = 3;
            label1.Text = "&Index of first tile";
            // 
            // btnSaveTilemap
            // 
            btnSaveTilemap.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnSaveTilemap.Location = new System.Drawing.Point(551, 225);
            btnSaveTilemap.Name = "btnSaveTilemap";
            btnSaveTilemap.Size = new System.Drawing.Size(81, 28);
            btnSaveTilemap.TabIndex = 5;
            btnSaveTilemap.Text = "&Save";
            btnSaveTilemap.UseVisualStyleBackColor = true;
            btnSaveTilemap.Click += btnSaveTilemap_Click;
            // 
            // cbHighPriority
            // 
            cbHighPriority.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            cbHighPriority.AutoSize = true;
            cbHighPriority.Location = new System.Drawing.Point(4, 231);
            cbHighPriority.Name = "cbHighPriority";
            cbHighPriority.Size = new System.Drawing.Size(116, 19);
            cbHighPriority.TabIndex = 2;
            cbHighPriority.Text = "In &front of sprites";
            cbHighPriority.UseVisualStyleBackColor = true;
            cbHighPriority.CheckedChanged += ControlChanged;
            // 
            // cbSpritePalette
            // 
            cbSpritePalette.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            cbSpritePalette.AutoSize = true;
            cbSpritePalette.Location = new System.Drawing.Point(4, 206);
            cbSpritePalette.Name = "cbSpritePalette";
            cbSpritePalette.Size = new System.Drawing.Size(116, 19);
            cbSpritePalette.TabIndex = 1;
            cbSpritePalette.Text = "&Use sprite palette";
            cbSpritePalette.UseVisualStyleBackColor = true;
            cbSpritePalette.CheckedChanged += ControlChanged;
            // 
            // tbTilemap
            // 
            tbTilemap.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbTilemap.Font = new System.Drawing.Font("Consolas", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            tbTilemap.Location = new System.Drawing.Point(3, 3);
            tbTilemap.Multiline = true;
            tbTilemap.Name = "tbTilemap";
            tbTilemap.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            tbTilemap.Size = new System.Drawing.Size(629, 197);
            tbTilemap.TabIndex = 0;
            tbTilemap.WordWrap = false;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(rbHexGG);
            tabPage4.Controls.Add(rbHexSMS);
            tabPage4.Controls.Add(cbPaletteConstants);
            tabPage4.Controls.Add(pbPalette);
            tabPage4.Controls.Add(btnSavePalette);
            tabPage4.Controls.Add(cbFullPalette);
            tabPage4.Controls.Add(tbPalette);
            tabPage4.ImageKey = "palette.png";
            tabPage4.Location = new System.Drawing.Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new System.Drawing.Size(635, 256);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Palette";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // rbHexGG
            // 
            rbHexGG.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            rbHexGG.AutoSize = true;
            rbHexGG.Location = new System.Drawing.Point(3, 230);
            rbHexGG.Name = "rbHexGG";
            rbHexGG.Size = new System.Drawing.Size(108, 19);
            rbHexGG.TabIndex = 2;
            rbHexGG.Text = "&GG (12-bit RGB)";
            rbHexGG.UseVisualStyleBackColor = true;
            rbHexGG.CheckedChanged += ControlChanged;
            // 
            // rbHexSMS
            // 
            rbHexSMS.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            rbHexSMS.AutoSize = true;
            rbHexSMS.Checked = true;
            rbHexSMS.Location = new System.Drawing.Point(3, 205);
            rbHexSMS.Name = "rbHexSMS";
            rbHexSMS.Size = new System.Drawing.Size(109, 19);
            rbHexSMS.TabIndex = 0;
            rbHexSMS.TabStop = true;
            rbHexSMS.Text = "S&MS (6-bit RGB)";
            rbHexSMS.UseVisualStyleBackColor = true;
            rbHexSMS.CheckedChanged += ControlChanged;
            // 
            // cbPaletteConstants
            // 
            cbPaletteConstants.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            cbPaletteConstants.AutoSize = true;
            cbPaletteConstants.Location = new System.Drawing.Point(135, 206);
            cbPaletteConstants.Name = "cbPaletteConstants";
            cbPaletteConstants.Size = new System.Drawing.Size(137, 19);
            cbPaletteConstants.TabIndex = 10;
            cbPaletteConstants.Text = "Use &constants (cl123)";
            cbPaletteConstants.UseVisualStyleBackColor = true;
            cbPaletteConstants.CheckedChanged += ControlChanged;
            // 
            // pbPalette
            // 
            pbPalette.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pbPalette.Location = new System.Drawing.Point(3, 162);
            pbPalette.Name = "pbPalette";
            pbPalette.Size = new System.Drawing.Size(629, 37);
            pbPalette.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pbPalette.TabIndex = 8;
            pbPalette.TabStop = false;
            // 
            // btnSavePalette
            // 
            btnSavePalette.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnSavePalette.Location = new System.Drawing.Point(550, 225);
            btnSavePalette.Name = "btnSavePalette";
            btnSavePalette.Size = new System.Drawing.Size(82, 28);
            btnSavePalette.TabIndex = 4;
            btnSavePalette.Text = "&Save";
            btnSavePalette.UseVisualStyleBackColor = true;
            btnSavePalette.Click += btnSavePalette_Click;
            // 
            // cbFullPalette
            // 
            cbFullPalette.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            cbFullPalette.AutoSize = true;
            cbFullPalette.Location = new System.Drawing.Point(135, 231);
            cbFullPalette.Name = "cbFullPalette";
            cbFullPalette.Size = new System.Drawing.Size(176, 19);
            cbFullPalette.TabIndex = 3;
            cbFullPalette.Text = "Always emit &16 or 32 colours";
            cbFullPalette.UseVisualStyleBackColor = true;
            cbFullPalette.CheckedChanged += ControlChanged;
            // 
            // tbPalette
            // 
            tbPalette.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tbPalette.Font = new System.Drawing.Font("Consolas", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            tbPalette.Location = new System.Drawing.Point(3, 3);
            tbPalette.Multiline = true;
            tbPalette.Name = "tbPalette";
            tbPalette.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            tbPalette.Size = new System.Drawing.Size(629, 153);
            tbPalette.TabIndex = 0;
            tbPalette.WordWrap = false;
            // 
            // tabPage5
            // 
            tabPage5.Controls.Add(tbMessages);
            tabPage5.ImageKey = "page_white_text.png";
            tabPage5.Location = new System.Drawing.Point(4, 24);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new System.Windows.Forms.Padding(3);
            tabPage5.Size = new System.Drawing.Size(635, 256);
            tabPage5.TabIndex = 4;
            tabPage5.Text = "Messages";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // tbMessages
            // 
            tbMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            tbMessages.Location = new System.Drawing.Point(3, 3);
            tbMessages.Multiline = true;
            tbMessages.Name = "tbMessages";
            tbMessages.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            tbMessages.Size = new System.Drawing.Size(629, 250);
            tbMessages.TabIndex = 0;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            imageList1.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = System.Drawing.Color.Transparent;
            imageList1.Images.SetKeyName(0, "picture.png");
            imageList1.Images.SetKeyName(1, "application_view_tile.png");
            imageList1.Images.SetKeyName(2, "map.png");
            imageList1.Images.SetKeyName(3, "palette.png");
            imageList1.Images.SetKeyName(4, "page_white_text.png");
            // 
            // Form1
            // 
            AllowDrop = true;
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(667, 391);
            Controls.Add(tabControl1);
            Controls.Add(statusStrip1);
            Controls.Add(groupBox1);
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            Name = "Form1";
            Text = "Bitmap to SMS/GG tile converter 0.63 by Maxim :: smspower.org";
            FormClosed += Form1_FormClosed;
            Load += Form1_Load;
            DragDrop += Form1_DragDrop;
            DragEnter += Form1_DragEnter;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pbPreview).EndInit();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pbPalette).EndInit();
            tabPage5.ResumeLayout(false);
            tabPage5.PerformLayout();
            ResumeLayout(false);

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
        private System.Windows.Forms.ImageList imageList1;
    }
}

