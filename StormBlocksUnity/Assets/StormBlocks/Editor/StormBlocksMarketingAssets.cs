using System.IO;
using UnityEditor;
using UnityEngine;

namespace StormBlocks.Editor
{
    public static class StormBlocksMarketingAssets
    {
        public const string IconAssetPath = "Assets/StormBlocks/Art/Generated/AppIconDraft.png";

        public static void GenerateLaunchAssets()
        {
            string fullPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), IconAssetPath));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            var icon = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
            DrawIcon(icon);
            File.WriteAllBytes(fullPath, icon.EncodeToPNG());
            Object.DestroyImmediate(icon);

            AssetDatabase.ImportAsset(IconAssetPath);
            var importer = AssetImporter.GetAtPath(IconAssetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void DrawIcon(Texture2D texture)
        {
            Color top = new Color(0.13f, 0.12f, 0.38f, 1f);
            Color bottom = new Color(0.04f, 0.06f, 0.20f, 1f);
            for (int y = 0; y < texture.height; y++)
            {
                float t = y / (float)(texture.height - 1);
                Color row = Color.Lerp(bottom, top, t);
                for (int x = 0; x < texture.width; x++)
                {
                    texture.SetPixel(x, y, row);
                }
            }

            DrawCircle(texture, 512, 512, 430, new Color(0.08f, 0.16f, 0.48f, 0.92f));
            DrawCircle(texture, 512, 512, 360, new Color(0.16f, 0.10f, 0.44f, 0.72f));
            DrawRing(texture, 512, 512, 390, 32, new Color(0.18f, 0.70f, 1.0f, 0.62f));
            DrawRing(texture, 512, 512, 308, 18, new Color(1.0f, 0.62f, 0.18f, 0.45f));

            DrawRoundedRect(texture, 238, 266, 250, 150, 38, new Color(0.05f, 0.74f, 0.94f, 1f));
            DrawRoundedRect(texture, 500, 266, 250, 150, 38, new Color(1.0f, 0.36f, 0.26f, 1f));
            DrawRoundedRect(texture, 369, 430, 250, 150, 38, new Color(0.42f, 0.88f, 0.14f, 1f));
            DrawRoundedRect(texture, 238, 594, 250, 150, 38, new Color(0.66f, 0.30f, 0.96f, 1f));
            DrawRoundedRect(texture, 500, 594, 250, 150, 38, new Color(1.0f, 0.72f, 0.18f, 1f));

            DrawBlockHighlight(texture, 238, 266, 250, 150);
            DrawBlockHighlight(texture, 500, 266, 250, 150);
            DrawBlockHighlight(texture, 369, 430, 250, 150);
            DrawBlockHighlight(texture, 238, 594, 250, 150);
            DrawBlockHighlight(texture, 500, 594, 250, 150);

            DrawCircle(texture, 512, 512, 86, new Color(1.0f, 0.48f, 0.12f, 1f));
            DrawRoundedRect(texture, 470, 496, 84, 72, 18, new Color(0.98f, 0.70f, 0.28f, 1f));
            DrawCircle(texture, 512, 548, 28, new Color(1.0f, 0.88f, 0.24f, 1f));
            DrawCircle(texture, 594, 498, 22, new Color(0.12f, 0.82f, 1.0f, 1f));
            DrawCircle(texture, 604, 525, 18, new Color(1.0f, 0.82f, 0.38f, 1f));

            texture.Apply();
        }

        private static void DrawBlockHighlight(Texture2D texture, int x, int y, int width, int height)
        {
            DrawCircle(texture, x + width - 52, y + height - 42, 24, new Color(1f, 0.92f, 0.48f, 0.46f));
            DrawRoundedRect(texture, x + 22, y + height - 35, width - 58, 14, 7, new Color(1f, 1f, 1f, 0.18f));
        }

        private static void DrawRing(Texture2D texture, int centerX, int centerY, int radius, int thickness, Color color)
        {
            int outer = radius * radius;
            int innerRadius = radius - thickness;
            int inner = innerRadius * innerRadius;
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                for (int x = centerX - radius; x <= centerX + radius; x++)
                {
                    int dx = x - centerX;
                    int dy = y - centerY;
                    int distance = dx * dx + dy * dy;
                    if (distance <= outer && distance >= inner)
                    {
                        BlendPixel(texture, x, y, color);
                    }
                }
            }
        }

        private static void DrawCircle(Texture2D texture, int centerX, int centerY, int radius, Color color)
        {
            int radiusSquared = radius * radius;
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                for (int x = centerX - radius; x <= centerX + radius; x++)
                {
                    int dx = x - centerX;
                    int dy = y - centerY;
                    if (dx * dx + dy * dy <= radiusSquared)
                    {
                        BlendPixel(texture, x, y, color);
                    }
                }
            }
        }

        private static void DrawRoundedRect(Texture2D texture, int x, int y, int width, int height, int radius, Color color)
        {
            int right = x + width - 1;
            int top = y + height - 1;
            for (int py = y; py <= top; py++)
            {
                for (int px = x; px <= right; px++)
                {
                    int cx = Mathf.Clamp(px, x + radius, right - radius);
                    int cy = Mathf.Clamp(py, y + radius, top - radius);
                    int dx = px - cx;
                    int dy = py - cy;
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        BlendPixel(texture, px, py, color);
                    }
                }
            }
        }

        private static void BlendPixel(Texture2D texture, int x, int y, Color color)
        {
            if (x < 0 || y < 0 || x >= texture.width || y >= texture.height)
            {
                return;
            }

            Color existing = texture.GetPixel(x, y);
            texture.SetPixel(x, y, Color.Lerp(existing, color, color.a));
        }
    }
}
