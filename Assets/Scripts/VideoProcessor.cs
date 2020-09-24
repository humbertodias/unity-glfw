﻿using System;
//using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Render
{
 public class VideoProcessor
    {
        public Action<Texture2D> OnTextureRecreated;

        public Texture2D Texture { get; private set; } = new Texture2D(8, 8, TextureFormat.BGRA32, false) { filterMode = FilterMode.Point };
        public FilterMode VideoFilterMode
        {
            get => _filterMode;
            set
            {
                _filterMode = value;
                CreateTexture(Texture.width, Texture.height);
            }
        }

        private FilterMode _filterMode = FilterMode.Point;

        public unsafe void ProcessFrame0RGB1555(ushort* data, int width, int height, int pitch)
        {
            CreateTexture(width, height);

            new ProcessFrame0RGB1555Job
            {
                SourceData  = data,
                Width       = width,
                Height      = height,
                PitchPixels = pitch / sizeof(ushort),
                TextureData = Texture.GetRawTextureData<uint>()
            }.Schedule().Complete();

            Texture.Apply();
        }

        public unsafe void ProcessFrameXRGB8888(uint* data, int width, int height, int pitch)
        {
            CreateTexture(width, height);

            new ProcessFrameXRGB8888Job
            {
                SourceData  = data,
                Width       = width,
                Height      = height,
                PitchPixels = pitch / sizeof(uint),
                TextureData = Texture.GetRawTextureData<uint>()
            }.Schedule().Complete();

            Texture.Apply();
        }

        public unsafe void ProcessFrameRGB565(ushort* data, int width, int height, int pitch)
        {
            CreateTexture(width, height);

            new ProcessFrameRGB565Job
            {
                SourceData  = data,
                Width       = width,
                Height      = height,
                PitchPixels = pitch / sizeof(ushort),
                TextureData = Texture.GetRawTextureData<uint>()
            }.Schedule().Complete();

            Texture.Apply();
        }

        private void CreateTexture(int width, int height)
        {
            if (Texture.width != width || Texture.height != height || Texture.filterMode != VideoFilterMode)
            {
                Texture = new Texture2D(width, height, TextureFormat.BGRA32, false)
                {
                    filterMode = VideoFilterMode
                };
                OnTextureRecreated?.Invoke(Texture);
            }
        }

        //[BurstCompile]
        private unsafe struct ProcessFrame0RGB1555Job : IJob
        {
            [ReadOnly] [NativeDisableUnsafePtrRestriction] public ushort* SourceData;
            public int Width;
            public int Height;
            public int PitchPixels;
            [WriteOnly] public NativeArray<uint> TextureData;

            public void Execute()
            {
                ushort* line = SourceData;
                for (int y = 0; y < Height; ++y)
                {
                    for (int x = 0; x < Width; ++x)
                    {
                        TextureData[y * Width + x] = ARGB1555toBGRA32(line[x]);
                    }
                    line += PitchPixels;
                }
            }
        }

        //[BurstCompile]
        private unsafe struct ProcessFrameXRGB8888Job : IJob
        {
            [ReadOnly] [NativeDisableUnsafePtrRestriction] public uint* SourceData;
            public int Width;
            public int Height;
            public int PitchPixels;
            [WriteOnly] public NativeArray<uint> TextureData;

            public void Execute()
            {
                uint* line = SourceData;
                for (int y = 0; y < Height; ++y)
                {
                    for (int x = 0; x < Width; ++x)
                    {
                        TextureData[y * Width + x] = line[x];
                    }
                    line += PitchPixels;
                }
            }
        }

        //[BurstCompile]
        private unsafe struct ProcessFrameRGB565Job : IJob
        {
            [ReadOnly] [NativeDisableUnsafePtrRestriction] public ushort* SourceData;
            public int Width;
            public int Height;
            public int PitchPixels;
            [WriteOnly] public NativeArray<uint> TextureData;

            public void Execute()
            {
                ushort* line = SourceData;
                for (int y = 0; y < Height; ++y)
                {
                    for (int x = 0; x < Width; ++x)
                    {
                        TextureData[y * Width + x] = RGB565toBGRA32(line[x]);
                    }
                    line += PitchPixels;
                }
            }
        }

        private static uint ARGB1555toBGRA32(ushort packed)
        {
            uint a = (uint)packed & 0x8000;
            uint r = (uint)packed & 0x7C00;
            uint g = (uint)packed & 0x03E0;
            uint b = (uint)packed & 0x1F;
            uint rgb = (r << 9) | (g << 6) | (b << 3);
            return (a * 0x1FE00) | rgb | ((rgb >> 5) & 0x070707);
        }

        private static uint RGB565toBGRA32(ushort packed)
        {
            uint r = ((uint)packed >> 11) & 0x1f;
            uint g = ((uint)packed >> 5) & 0x3f;
            uint b = ((uint)packed >> 0) & 0x1f;
            r      = (r << 3) | (r >> 2);
            g      = (g << 2) | (g >> 4);
            b      = (b << 3) | (b >> 2);
            return (0xffu << 24) | (r << 16) | (g << 8) | (b << 0);
        }
    }
}