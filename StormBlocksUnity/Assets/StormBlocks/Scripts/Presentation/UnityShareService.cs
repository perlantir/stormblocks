using System.IO;
using StormBlocks.Core;
using StormBlocks.Services;
using UnityEngine;

namespace StormBlocks.Presentation
{
    public sealed class UnityShareService : MonoBehaviour, IShareService
    {
        private const int CardWidth = 1200;
        private const int CardHeight = 630;

        public bool CanShare
        {
            get { return true; }
        }

        public void ShareRun(RunSummary runSummary)
        {
            if (runSummary == null)
            {
                return;
            }

            string message = BuildShareMessage(runSummary);
            string imagePath = WriteShareCard(runSummary);

#if UNITY_IOS && !UNITY_EDITOR
            SBShareTextAndImage(message, imagePath);
#else
            GUIUtility.systemCopyBuffer = message;
            Debug.Log("Storm Blocks share text copied: " + message);
#endif
        }

        private static string BuildShareMessage(RunSummary runSummary)
        {
            return "Storm Blocks " + ModeLabel(runSummary.Mode) +
                   ": " + runSummary.Score +
                   " points, " + runSummary.SurvivorsRescued +
                   " survivors, " + runSummary.StormTilesDestroyed +
                   " storm tiles pushed back. " + runSummary.ShareToken;
        }

        private static string ModeLabel(GameModeId mode)
        {
            switch (mode)
            {
                case GameModeId.DailyStorm:
                    return "Daily Storm";
                case GameModeId.StormTrail:
                    return "Storm Trail";
                case GameModeId.TempestTrial:
                    return "Tempest Trials";
                case GameModeId.Practice:
                    return "Practice";
                default:
                    return "Endless Storm";
            }
        }

        private static string WriteShareCard(RunSummary runSummary)
        {
            var texture = new Texture2D(CardWidth, CardHeight, TextureFormat.RGBA32, false);
            var pixels = new Color32[CardWidth * CardHeight];

            var top = new Color32(27, 30, 84, 255);
            var bottom = new Color32(99, 74, 150, 255);
            for (int y = 0; y < CardHeight; y++)
            {
                float t = y / (float)(CardHeight - 1);
                byte r = (byte)Mathf.RoundToInt(Mathf.Lerp(bottom.r, top.r, t));
                byte g = (byte)Mathf.RoundToInt(Mathf.Lerp(bottom.g, top.g, t));
                byte b = (byte)Mathf.RoundToInt(Mathf.Lerp(bottom.b, top.b, t));
                for (int x = 0; x < CardWidth; x++)
                {
                    pixels[y * CardWidth + x] = new Color32(r, g, b, 255);
                }
            }

            DrawRect(pixels, 0, 0, CardWidth, 96, new Color32(255, 174, 66, 255));
            DrawRect(pixels, 0, 0, CardWidth, 22, new Color32(255, 230, 138, 255));
            DrawCircle(pixels, 200, 126, 72, new Color32(255, 196, 80, 255));
            DrawCircle(pixels, 988, 440, 170, new Color32(63, 38, 132, 210));
            DrawCircle(pixels, 1068, 354, 128, new Color32(34, 151, 236, 180));
            DrawCircle(pixels, 920, 318, 92, new Color32(112, 74, 220, 180));

            DrawBoard(pixels);
            DrawCamp(pixels);
            DrawScoreBars(pixels, runSummary);

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            byte[] png = texture.EncodeToPNG();
            Object.Destroy(texture);

            string path = Path.Combine(Application.persistentDataPath, "stormblocks-share-card.png");
            File.WriteAllBytes(path, png);
            return path;
        }

        private static void DrawBoard(Color32[] pixels)
        {
            const int left = 104;
            const int top = 172;
            const int cell = 42;
            const int gap = 6;
            DrawRect(pixels, left - 22, top - 22, cell * 8 + gap * 7 + 44, cell * 8 + gap * 7 + 44, new Color32(17, 22, 65, 255));

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    bool storm = x == 0 || y == 0 || x == 7 || y == 7 || (x == 6 && y < 4);
                    bool block = (x + y) % 5 == 0 || (x == 2 && y > 2) || (y == 5 && x < 5);
                    Color32 color = storm ? new Color32(40, 64, 152, 255) : block ? BlockColor(x + y) : new Color32(83, 102, 186, 255);
                    DrawRect(pixels, left + x * (cell + gap), top + y * (cell + gap), cell, cell, color);
                }
            }

            DrawRect(pixels, left + 2 * (cell + gap), top + 3 * (cell + gap), cell * 4 + gap * 3, cell, new Color32(255, 188, 68, 255));
            DrawRect(pixels, left + 3 * (cell + gap), top + 2 * (cell + gap), cell, cell * 4 + gap * 3, new Color32(255, 206, 95, 255));
        }

        private static Color32 BlockColor(int value)
        {
            switch (value % 5)
            {
                case 0:
                    return new Color32(29, 195, 233, 255);
                case 1:
                    return new Color32(255, 106, 84, 255);
                case 2:
                    return new Color32(143, 221, 56, 255);
                case 3:
                    return new Color32(169, 91, 239, 255);
                default:
                    return new Color32(255, 190, 56, 255);
            }
        }

        private static void DrawCamp(Color32[] pixels)
        {
            DrawCircle(pixels, 552, 324, 76, new Color32(255, 145, 62, 255));
            DrawCircle(pixels, 552, 324, 42, new Color32(255, 226, 125, 255));
            DrawRect(pixels, 520, 256, 64, 130, new Color32(255, 121, 56, 255));
            DrawRect(pixels, 484, 356, 136, 42, new Color32(92, 58, 118, 255));
            DrawCircle(pixels, 446, 442, 25, new Color32(255, 223, 76, 255));
            DrawCircle(pixels, 628, 442, 25, new Color32(73, 199, 255, 255));
        }

        private static void DrawScoreBars(Color32[] pixels, RunSummary runSummary)
        {
            int scoreWidth = Mathf.Clamp(runSummary.Score / 12, 120, 460);
            int rescueWidth = Mathf.Clamp(runSummary.SurvivorsRescued * 36, 80, 340);
            int stormWidth = Mathf.Clamp(runSummary.StormTilesDestroyed * 28, 80, 340);

            DrawRect(pixels, 670, 166, 460, 46, new Color32(25, 27, 74, 235));
            DrawRect(pixels, 670, 166, scoreWidth, 46, new Color32(255, 188, 72, 255));
            DrawRect(pixels, 670, 250, 340, 40, new Color32(25, 27, 74, 235));
            DrawRect(pixels, 670, 250, rescueWidth, 40, new Color32(255, 222, 84, 255));
            DrawRect(pixels, 670, 326, 340, 40, new Color32(25, 27, 74, 235));
            DrawRect(pixels, 670, 326, stormWidth, 40, new Color32(83, 197, 255, 255));
            DrawRect(pixels, 670, 460, 420, 54, new Color32(255, 126, 69, 255));
        }

        private static void DrawRect(Color32[] pixels, int x, int y, int width, int height, Color32 color)
        {
            int minX = Mathf.Clamp(x, 0, CardWidth);
            int maxX = Mathf.Clamp(x + width, 0, CardWidth);
            int minY = Mathf.Clamp(y, 0, CardHeight);
            int maxY = Mathf.Clamp(y + height, 0, CardHeight);
            for (int row = minY; row < maxY; row++)
            {
                int offset = row * CardWidth;
                for (int col = minX; col < maxX; col++)
                {
                    pixels[offset + col] = color;
                }
            }
        }

        private static void DrawCircle(Color32[] pixels, int centerX, int centerY, int radius, Color32 color)
        {
            int radiusSquared = radius * radius;
            int minX = Mathf.Clamp(centerX - radius, 0, CardWidth - 1);
            int maxX = Mathf.Clamp(centerX + radius, 0, CardWidth - 1);
            int minY = Mathf.Clamp(centerY - radius, 0, CardHeight - 1);
            int maxY = Mathf.Clamp(centerY + radius, 0, CardHeight - 1);
            for (int y = minY; y <= maxY; y++)
            {
                int dy = y - centerY;
                int offset = y * CardWidth;
                for (int x = minX; x <= maxX; x++)
                {
                    int dx = x - centerX;
                    if (dx * dx + dy * dy <= radiusSquared)
                    {
                        pixels[offset + x] = color;
                    }
                }
            }
        }

#if UNITY_IOS && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void SBShareTextAndImage(string message, string imagePath);
#endif
    }
}
