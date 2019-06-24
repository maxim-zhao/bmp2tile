using System;
using System.Collections.Generic;

namespace BMP2Tile
{
    /// <summary>
    /// Interface for something that can convert tiles or tilemaps to bytes
    /// </summary>
    internal interface ICompressor : IDisposable
    {
        string Extension { get; }
        string Name { get; }
        CompressorCapabilities Capabilities { get; }
        IEnumerable<byte> CompressTiles(IList<Tile> tiles, bool asChunky);
        IEnumerable<byte> CompressTilemap(Tilemap tilemap);
    }

    [Flags]
    public enum CompressorCapabilities
    {
        None = 0,
        Tiles,
        Tilemap
    }
}