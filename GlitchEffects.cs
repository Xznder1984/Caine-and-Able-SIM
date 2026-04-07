using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace CASimulator
{
    public static class GlitchEffects
    {
        private static Random random = new Random();

        /// <summary>
        /// Creates RGB color shift glitch effect
        /// </summary>
        public static void ApplyRGBShift(TextBlock target, double intensity)
        {
            if (target == null) return;

            double shiftAmount = intensity * 0.3;
            
            // Random color shift
            Color glitchColor = Color.FromRgb(
                (byte)(Math.Min(255, 0 + shiftAmount * 200)),
                (byte)(Math.Max(0, 255 - shiftAmount * 100)),
                (byte)(Math.Min(255, shiftAmount * 150))
            );

            target.Foreground = new SolidColorBrush(glitchColor);
        }

        /// <summary>
        /// Screen shake effect via transform
        /// </summary>
        public static void ApplyScreenShake(UIElement target, double intensity)
        {
            if (target == null) return;

            int shakeAmount = (int)(intensity * 30);
            int offsetX = random.Next(-shakeAmount, shakeAmount);
            int offsetY = random.Next(-shakeAmount, shakeAmount);

            target.RenderTransform = new TranslateTransform(offsetX, offsetY);
        }

        /// <summary>
        /// Text corruption effect - replaces characters
        /// </summary>
        public static string ApplyTextCorruption(string text, double intensity)
        {
            if (string.IsNullOrEmpty(text)) return text;

            char[] glitchChars = { '█', '▓', '░', '▀', '▄', '▌', '▐', '├', '┤', '┼' };
            char[] chars = text.ToCharArray();
            int corruptionCount = (int)(chars.Length * intensity);

            for (int i = 0; i < corruptionCount; i++)
            {
                int randomIndex = random.Next(chars.Length);
                chars[randomIndex] = glitchChars[random.Next(glitchChars.Length)];
            }

            return new string(chars);
        }

        /// <summary>
        /// Draw RGB shift lines for visual distortion
        /// </summary>
        public static void DrawGlitchLines(Canvas canvas, double intensity)
        {
            canvas.Children.Clear();

            int lineCount = (int)(20 * intensity);
            int thickness = Math.Max(1, (int)(3 * intensity));

            for (int i = 0; i < lineCount; i++)
            {
                Line line = new Line();
                line.X1 = random.Next((int)canvas.ActualWidth);
                line.Y1 = random.Next((int)canvas.ActualHeight);
                line.X2 = line.X1 + random.Next(100, 400);
                line.Y2 = line.Y1 + random.Next(-50, 50);

                // Random RGB colors
                byte r = (byte)random.Next(256);
                byte g = (byte)random.Next(256);
                byte b = (byte)random.Next(256);

                line.Stroke = new SolidColorBrush(Color.FromRgb(r, g, b));
                line.StrokeThickness = thickness;
                line.Opacity = 0.3 + intensity * 0.7;

                canvas.Children.Add(line);
            }
        }

        /// <summary>
        /// Block distortion effect
        /// </summary>
        public static void ApplyBlockDistortion(Canvas canvas, double intensity)
        {
            canvas.Children.Clear();

            int blockCount = (int)(50 * intensity);
            int blockSize = (int)(20 + 40 * intensity);

            for (int i = 0; i < blockCount; i++)
            {
                Rectangle block = new Rectangle();
                block.Width = random.Next(blockSize / 2, blockSize);
                block.Height = random.Next(blockSize / 2, blockSize);

                // Random colors for each block
                byte r = (byte)random.Next(256);
                byte g = (byte)random.Next(256);
                byte b = (byte)random.Next(256);

                block.Fill = new SolidColorBrush(Color.FromRgb(r, g, b));
                block.Opacity = 0.1 + intensity * 0.5;

                Canvas.SetLeft(block, random.Next((int)canvas.ActualWidth));
                Canvas.SetTop(block, random.Next((int)canvas.ActualHeight));

                canvas.Children.Add(block);
            }
        }

        /// <summary>
        /// Scanline flicker effect
        /// </summary>
        public static void ApplyScanlineFlicker(Canvas canvas, double intensity)
        {
            canvas.Children.Clear();

            if (random.NextDouble() < intensity)
            {
                int lineHeight = 2;
                int spacing = (int)(4 / intensity);

                for (double y = 0; y < canvas.ActualHeight; y += spacing)
                {
                    if (random.NextDouble() < 0.5)
                    {
                        Line line = new Line();
                        line.X1 = 0;
                        line.Y1 = y;
                        line.X2 = canvas.ActualWidth;
                        line.Y2 = y;
                        line.Stroke = new SolidColorBrush(Color.FromArgb((byte)(50 * intensity), 0, 0, 0));
                        line.StrokeThickness = lineHeight;
                        canvas.Children.Add(line);
                    }
                }
            }
        }

        /// <summary>
        /// Horizontal shift effect
        /// </summary>
        public static void ApplyHorizontalShift(UIElement target, double intensity)
        {
            if (target == null) return;

            int shiftAmount = (int)(intensity * 50);
            int offset = random.Next(-shiftAmount, shiftAmount);

            target.RenderTransform = new TranslateTransform(offset, 0);
        }

        /// <summary>
        /// Vertical shift effect
        /// </summary>
        public static void ApplyVerticalShift(UIElement target, double intensity)
        {
            if (target == null) return;

            int shiftAmount = (int)(intensity * 50);
            int offset = random.Next(-shiftAmount, shiftAmount);

            target.RenderTransform = new TranslateTransform(0, offset);
        }

        /// <summary>
        /// Rotation/tilt effect
        /// </summary>
        public static void ApplyRotationGlitch(UIElement target, double intensity)
        {
            if (target == null) return;

            double rotationAmount = (intensity - 0.5) * 10; // -5 to +5 degrees
            RotateTransform rotation = new RotateTransform(rotationAmount);
            rotation.CenterX = 0.5;
            rotation.CenterY = 0.5;

            target.RenderTransform = rotation;
        }

        /// <summary>
        /// Scale/zoom glitch effect
        /// </summary>
        public static void ApplyScaleGlitch(UIElement target, double intensity)
        {
            if (target == null) return;

            double scale = 1.0 + (random.NextDouble() - 0.5) * intensity;
            ScaleTransform scaleTransform = new ScaleTransform(scale, scale);

            target.RenderTransform = scaleTransform;
        }

        /// <summary>
        /// Generate a random glitch message
        /// </summary>
        public static string GetRandomGlitchMessage()
        {
            string[] glitchMessages = new string[]
            {
                "▓▓▓ SYSTEM ERROR ▓▓▓",
                "⚠️  CRITICAL INSTABILITY",
                "████ CORE FAILURE ████",
                "▒▒▒ DATA CORRUPTION ▒▒▒",
                "░░░ MEMORY OVERFLOW ░░░",
                "┌─ GLITCH DETECTED ─┐",
                "▄▄▄ ALERT ▄▄▄",
                "!!!UNRECOVER!ABLE!!!",
                "►►►FATAL ERROR◄◄◄",
                "❌ SYSTEM UNSTABLE ❌",
            };

            return glitchMessages[random.Next(glitchMessages.Length)];
        }

        /// <summary>
        /// Generate random corrupted text
        /// </summary>
        public static string GenerateGlitchText(int length)
        {
            string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz█▓░▀▄▌▐├┤┼";
            string result = "";

            for (int i = 0; i < length; i++)
            {
                result += charset[random.Next(charset.Length)];
            }

            return result;
        }
    }
}
