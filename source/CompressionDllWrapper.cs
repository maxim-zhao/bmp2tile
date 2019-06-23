﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace BMP2Tile
{
    internal class CompressionDllWrapper: ICompressor
    {
        private readonly IntPtr _hModule;

        // BMP2Tile exported function signatures
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SaveTilesDelegate(byte[] source, int numtiles, byte[] dest, int destLen); 
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SaveTilemapDelegate(byte[] source, int width, int height, byte[] dest, int destLen);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr GetStringDelegate();

        private readonly SaveTilesDelegate _saveTiles;
        private readonly SaveTilemapDelegate _saveTilemap;

        public CompressionDllWrapper(string filename)
        {
            _hModule = NativeMethods.LoadLibrary(filename);
            if (_hModule == IntPtr.Zero)
            {
                return;
            }

            Name = Marshal.PtrToStringAnsi(GetFunction<GetStringDelegate>("getName")());
            Extension = Marshal.PtrToStringAnsi(GetFunction<GetStringDelegate>("getExt")());

            _saveTiles = GetFunction<SaveTilesDelegate>("compressTiles");
            _saveTilemap = GetFunction<SaveTilemapDelegate>("compressTilemap");

            Capabilities = CompressorCapabilities.None;

            if (_saveTilemap != null)
            {
                Capabilities |= CompressorCapabilities.Tilemap;
            }
            if (_saveTiles != null)
            {
                Capabilities |= CompressorCapabilities.Tiles;
            }
        }

        public string Extension { get; }

        public string Name { get; }

        public CompressorCapabilities Capabilities { get; }

        public IEnumerable<byte> CompressTiles(IList<Tile> tiles, bool asChunky)
        {
            if (_saveTiles == null)
            {
                throw new NotSupportedException($"{Name} compressor does not support tiles");
            }
            // First we convert the tiles to a buffer
            var source = tiles.SelectMany(tile => tile.GetValue(asChunky)).ToArray();

            return Compress(dest => _saveTiles(source, tiles.Count(), dest, dest.Length));
        }

        public IEnumerable<byte> CompressTilemap(Tilemap tilemap)
        {
            if (_saveTilemap == null)
            {
                throw new NotSupportedException($"{Name} compressor does not support tilemaps");
            }
            // First we convert the tilemap to a buffer
            var source = new List<byte>();
            for (int y = 0; y < tilemap.Height; ++y)
            for (int x = 0; x < tilemap.Width; ++x)
            {
                var entry = tilemap[x, y].GetValue();
                // Little-endian
                source.Add((byte)((entry >> 0) & 0xff));
                source.Add((byte)((entry >> 8) & 0xff));
            }

            return Compress(dest => _saveTilemap(source.ToArray(), tilemap.Width, tilemap.Height, dest, dest.Length));
        }

        private IEnumerable<byte> Compress(Func<byte[], int> compressor)
        {
            // Then we make a buffer to compress into... we try bigger sizes if it doesn't seem big enough
            for (var bufferSize = 16 * 1024; bufferSize <= 1024 * 1024; bufferSize *= 2)
            {
                var dest = new byte[bufferSize];
                var result = compressor(dest);
                if (result < 0)
                {
                    // Failure
                    break;
                }
                if (result > 0)
                {
                    // Success - result is the byte length
                    return dest.Take(result);
                }
                // Else we need a bigger buffer...
            }
            throw new Exception("Failure compressing tiles");
        }

        private T GetFunction<T>(string functionName) where T: Delegate
        {
            var functionPointer = NativeMethods.GetProcAddress(_hModule, functionName);
            if (functionPointer == IntPtr.Zero)
            {
                return null;
            }
            return Marshal.GetDelegateForFunctionPointer<T>(functionPointer);
        }

        private void ReleaseUnmanagedResources()
        {
            if (_hModule != IntPtr.Zero)
            {
                NativeMethods.FreeLibrary(_hModule);
            }
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~CompressionDllWrapper()
        {
            ReleaseUnmanagedResources();
        }
    }
}