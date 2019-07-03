using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BMP2Tile;

namespace BMP2TileGUI
{
    public partial class Form1 : Form
    {
        private readonly Converter _converter;

        public Form1()
        {
            InitializeComponent();

            _converter = new Converter(OnMessageLogged);
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _converter.Dispose();
        }

        private void OnMessageLogged(string message, Converter.LogLevel logLevel)
        {
            tbMessages.AppendText(message + Environment.NewLine);

            if (logLevel >= Converter.LogLevel.Normal)
            {
                lblStatus.Text = message;
            }

            if (logLevel >= Converter.LogLevel.Error)
            {
                MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog
            {
                Filter = "Image files (*.bmp;*.png;*.gif)|*.bmp;*.png;*.gif|All files (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            })
            {
                if (ofd.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                LoadImage(ofd.FileName);
            }
        }

        private void Try(Action a)
        {
            try
            {
                a();
            }
            catch (Exception ex)
            {
                var message = $"Error: {ex.Message}";
                if (!(ex is AppException))
                {
                    message += $"\n\nGuru meditation:\n\n{ex.StackTrace}";
                }
                OnMessageLogged(message, Converter.LogLevel.Error);
            }
        }

        private void LoadImage(string filename)
        {
            Try(() =>
            {
                _converter.Filename = filename;
                ConvertForDisplay();
                tbFilename.Text = filename;
                pbPreview.ImageLocation = filename;
            });
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                LoadImage(files[0]);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Try(() =>
            {
                var compressors = _converter.GetCompressorInfo()
                    .Where(x => x.Capabilities.HasFlag(CompressorCapabilities.Tiles))
                    .OrderBy(x => x.Name)
                    .ToList();

                var filter = string.Join("|",
                    compressors.SelectMany(x => new[] {$"{x.Name} (*.{x.Extension})", $"*.{x.Extension}"}));

                using (var sfd = new SaveFileDialog {Filter = filter})
                {
                    if (sfd.ShowDialog(this) == DialogResult.OK)
                    {
                        _converter.SaveTiles(sfd.FileName);
                    }
                }
            });
        }

        private void btnSaveTilemap_Click(object sender, EventArgs e)
        {
            Try(() =>
            {
                var compressors = _converter.GetCompressorInfo()
                    .Where(x => x.Capabilities.HasFlag(CompressorCapabilities.Tilemap))
                    .OrderBy(x => x.Name)
                    .ToList();

                var filter = string.Join("|",
                    compressors.SelectMany(x => new[] {$"{x.Name} (*.{x.Extension})", $"*.{x.Extension}"}));

                using (var sfd = new SaveFileDialog {Filter = filter})
                {
                    if (sfd.ShowDialog(this) == DialogResult.OK)
                    {
                        _converter.SaveTilemap(sfd.FileName);
                    }
                }
            });
        }

        private void btnSavePalette_Click(object sender, EventArgs e)
        {
            Try(() =>
            {
                string filter = "Include files (*.inc)|*.inc";
                if (rbHexGG.Checked || (rbHexSMS.Checked && !cbPaletteConstants.Checked))
                {
                    filter += "|Binary files (*.bin)|*.bin";
                }

                using (var sfd = new SaveFileDialog {Filter = filter})
                {
                    if (sfd.ShowDialog(this) == DialogResult.OK)
                    {
                        _converter.SavePalette(sfd.FileName);
                    }
                }
            });
        }

        private void ControlChanged(object sender, EventArgs e)
        {
            Try(ConvertForDisplay);
        }

        private void ConvertForDisplay()
        {
            _converter.RemoveDuplicates = cbRemoveDuplicates.Checked;
            _converter.UseMirroring = cbUseMirroring.Checked;
            _converter.AdjacentBelow = cb8x16.Checked;
            if (!uint.TryParse(tbFirstTileIndex.Text, out var tileOffset))
            {
                tileOffset = 0;
            }
            _converter.TileOffset = tileOffset;

            _converter.UseSpritePalette = cbSpritePalette.Checked;
            _converter.HighPriority = cbHighPriority.Checked;

            _converter.FullPalette = cbFullPalette.Checked;

            // Disable mirroring checkbox if optimization is off
            cbUseMirroring.Enabled = cbRemoveDuplicates.Checked;

            // Disable constants if not SMS
            cbPaletteConstants.Enabled = rbHexSMS.Checked;

            // We want to generate text versions of everything for the text boxes
            tbTiles.Text = _converter.GetTilesAsText();
            tbTilemap.Text = _converter.GetTilemapAsText();
            var format = rbHexSMS.Checked
                ? cbPaletteConstants.Checked
                    ? Palette.Formats.MasterSystemConstants
                    : Palette.Formats.MasterSystem
                : Palette.Formats.GameGear;
            tbPalette.Text = _converter.GetPaletteAsText(format);

            // We also want to display the palette before and after conversion
            var palettes = _converter.GetPalettes(format);
            var bitmap = new Bitmap(pbPalette.Width, pbPalette.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                var n = palettes[0].Count;
                for (int x = 0; x < n; ++x)
                {
                    var xpos = x * bitmap.Width / n;
                    var ypos = 0;
                    var width = (x + 1) * bitmap.Width / n - xpos;
                    var height = bitmap.Height / 2;
                    using (var brush = new SolidBrush(palettes[0][x]))
                    {
                        g.FillRectangle(brush, xpos, ypos, width, height);
                    }

                    ypos = height;
                    height = bitmap.Height - height;
                    using (var brush = new SolidBrush(palettes[1][x]))
                    {
                        g.FillRectangle(brush, xpos, ypos, width, height);
                    }
                }
            }

            pbPalette.Image = bitmap; // It takes ownership
        }
    }
}
