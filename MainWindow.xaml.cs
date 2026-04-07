using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using NAudio.Wave;

namespace CASimulator
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer bootTimer;
        private DispatcherTimer scanlineTimer;
        private DispatcherTimer glitchTimer;
        private DispatcherTimer clockTimer;
        private bool isBooting = true;
        private bool isDeleting = false;
        private bool isDeletionComplete = false;
        private FileSystem fileSystem;
        private Random glitchRandom = new Random();
        private IWavePlayer wavePlayer;

        // Boot sequence state
        private int bootStep = 0;
        private List<string> bootMessages = new List<string>
        {
            "Initializing C&A Systems...",
            "Loading memory sectors...",
            "Connecting to core...",
            "Authenticating user: KINGER",
            "Loading entity database...",
            "System ready.",
            ""
        };

        public MainWindow()
        {
            InitializeComponent();
            InitializeWindow();
            InitializeFileSystem();
            SetupTimers();
            BeginBootSequence();
        }

        private void InitializeWindow()
        {
            // Fullscreen borderless setup
            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.Topmost = true;

            // Hide taskbar (Windows-specific)
            HideTaskbar();

            // Keyboard preview handler
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        private void HideTaskbar()
        {
            try
            {
                // Get taskbar handle and hide it
                IntPtr taskbarHandle = NativeMethods.FindWindow("Shell_traywnd", null);
                if (taskbarHandle != IntPtr.Zero)
                {
                    NativeMethods.ShowWindow(taskbarHandle, 0);
                }
            }
            catch { }
        }

        private void ShowTaskbar()
        {
            try
            {
                IntPtr taskbarHandle = NativeMethods.FindWindow("Shell_traywnd", null);
                if (taskbarHandle != IntPtr.Zero)
                {
                    NativeMethods.ShowWindow(taskbarHandle, 1);
                }
            }
            catch { }
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Secret exit: Ctrl+Shift+Q
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift &&
                e.Key == Key.Q)
            {
                e.Handled = true;
                ShowTaskbar();
                this.Close();
            }
        }

        private void InitializeFileSystem()
        {
            fileSystem = new FileSystem();
            fileSystem.CreateDefaultStructure();
        }

        private void SetupTimers()
        {
            bootTimer = new DispatcherTimer();
            bootTimer.Interval = TimeSpan.FromMilliseconds(1200);
            bootTimer.Tick += BootTimer_Tick;

            scanlineTimer = new DispatcherTimer();
            scanlineTimer.Interval = TimeSpan.FromMilliseconds(100);
            scanlineTimer.Tick += ScanlineTimer_Tick;
            scanlineTimer.Start();

            glitchTimer = new DispatcherTimer();
            glitchTimer.Interval = TimeSpan.FromMilliseconds(150);
            glitchTimer.Tick += GlitchTimer_Tick;

            clockTimer = new DispatcherTimer();
            clockTimer.Interval = TimeSpan.FromSeconds(1);
            clockTimer.Tick += (s, e) => SystemTimeBlock.Text = DateTime.Now.ToString("HH:mm:ss");
            clockTimer.Start();
        }

        private void BeginBootSequence()
        {
            isBooting = true;
            TerminalOutput.Clear();
            bootTimer.Start();
        }

        private void BootTimer_Tick(object sender, EventArgs e)
        {
            if (bootStep < bootMessages.Count)
            {
                string msg = bootMessages[bootStep];
                AppendOutput(msg);
                bootStep++;
            }
            else
            {
                bootTimer.Stop();
                isBooting = false;
                CommandInput.IsEnabled = true;
                CommandInput.Focus();
                AppendOutput("\n");
                AppendOutput(GetSystemInfo());
                AppendOutput("\nType 'help' for available commands.\n");
                glitchTimer.Start();
            }
        }

        private string GetSystemInfo()
        {
            return @"
╔════════════════════════════════════════╗
║          C&A SYSTEM v1.0               ║
║    THE AMAZING DIGITAL CIRCUS          ║
╚════════════════════════════════════════╝

USER:       KINGER
STATUS:     STABLE
ENTITIES:   LOADED
CORE LINK:  ONLINE
MEMORY:     32MB / 32MB
";
        }

        private void AppendOutput(string text)
        {
            TerminalOutput.AppendText(text);
            TerminalOutput.ScrollToEnd();
        }

        private void AppendOutputWithDelay(string text)
        {
            // Character-by-character typing effect
            foreach (char c in text)
            {
                AppendOutput(c.ToString());
                System.Threading.Thread.Sleep(30);
                MainGrid.UpdateLayout();
            }
        }

        private void CommandInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && !isBooting)
            {
                e.Handled = true;
                ProcessCommand(CommandInput.Text);
                CommandInput.Clear();
            }
        }

        private void ProcessCommand(string command)
        {
            command = command.Trim().ToLower();
            AppendOutput($"CA> {command}\n");

            if (string.IsNullOrEmpty(command))
            {
                return;
            }

            // Parse command
            string[] parts = command.Split(' ');
            string cmd = parts[0];
            string[] args = parts.Skip(1).ToArray();

            switch (cmd)
            {
                case "help":
                    ShowHelp();
                    break;
                case "dir":
                    ListDirectory(args.Length > 0 ? args[0] : "/");
                    break;
                case "cd":
                    ChangeDirectory(args.Length > 0 ? args[0] : "/");
                    break;
                case "cat":
                case "open":
                    ReadFile(args.Length > 0 ? args[0] : "");
                    break;
                case "delete":
                    if (args.Length > 0 && args[0] == "caine")
                        InitiateCaineDelete();
                    else
                        AppendOutput("Usage: delete caine\n");
                    break;
                case "system":
                    AppendOutput(GetSystemInfo() + "\n");
                    break;
                case "status":
                    AppendOutput("\n[SYSTEM STATUS]\nAll systems operational.\nNo critical errors.\n");
                    break;
                case "scan":
                    ScanEntities();
                    break;
                case "clear":
                    TerminalOutput.Clear();
                    break;
                default:
                    AppendOutput($"Unknown command: {cmd}\n");
                    break;
            }
        }

        private void ShowHelp()
        {
            AppendOutput(@"
┌─ C&A SYSTEM COMMANDS ─────────────────┐
│ help       - Show this help menu       │
│ system     - Display system info       │
│ status     - Check system status       │
│ dir [path] - List directory contents   │
│ cd [path]  - Change directory          │
│ cat [file] - Read file contents        │
│ scan       - Scan entity database      │
│ delete caine - DANGER: Delete entity  │
│ clear      - Clear terminal            │
└────────────────────────────────────────┘

");
        }

        private void ListDirectory(string path)
        {
            var items = fileSystem.ListDirectory(path);
            foreach (var item in items)
            {
                AppendOutput($"  {item.Name}\n");
            }
            AppendOutput("\n");
        }

        private void ChangeDirectory(string path)
        {
            bool success = fileSystem.ChangeDirectory(path);
            if (!success)
            {
                AppendOutput($"Error: Directory not found - {path}\n");
            }
            AppendOutput("\n");
        }

        private void ReadFile(string filename)
        {
            string content = fileSystem.ReadFile(filename);
            if (content != null)
            {
                AppendOutput(content);
                AppendOutput("\n");
            }
            else
            {
                AppendOutput($"Error: File not found - {filename}\n");
            }
        }

        private void ScanEntities()
        {
            AppendOutput("\nScanning entity database...\n");
            AppendOutput("┌─────────────────────────┐\n");
            AppendOutput("│ Entity Scan Results:    │\n");
            AppendOutput("├─────────────────────────┤\n");
            AppendOutput("│ CAINE       [ACTIVE]    │\n");
            AppendOutput("│ POMNI       [ACTIVE]    │\n");
            AppendOutput("│ JODI        [ACTIVE]    │\n");
            AppendOutput("│ KINGER      [ACTIVE]    │\n");
            AppendOutput("│ RAGATHA     [ACTIVE]    │\n");
            AppendOutput("│ GUMMIGON    [ACTIVE]    │\n");
            AppendOutput("└─────────────────────────┘\n");
            AppendOutput("\nCore Link Status: CAINE <- SYSTEM CORE (CRITICAL)\n\n");
        }

        private void InitiateCaineDelete()
        {
            if (isDeleting)
                return;

            AppendOutput("\n╔════════════════════════════════════╗\n");
            AppendOutput("║ ⚠️  WARNING ⚠️                      ║\n");
            AppendOutput("║ This action cannot be undone.       ║\n");
            AppendOutput("║ Are you absolutely sure? (Y/N)      ║\n");
            AppendOutput("╚════════════════════════════════════╝\n");
            AppendOutput("\n");

            // Wait for user confirmation
            CommandInput.IsEnabled = true;
            CommandInput.Focus();

            // Declare handler first, then assign
            KeyEventHandler handler = null;
            handler = new KeyEventHandler((s, e) =>
            {
                if (e.Key == Key.Return)
                {
                    e.Handled = true;
                    string response = CommandInput.Text.Trim().ToLower();
                    CommandInput.Clear();
                    CommandInput.KeyDown -= handler;

                    if (response == "y" || response == "yes")
                    {
                        ExecuteCaineDelete();
                    }
                    else
                    {
                        AppendOutput("Deletion cancelled.\n\n");
                    }
                }
            });

            CommandInput.KeyDown += handler;
        }

        private void ExecuteCaineDelete()
        {
            isDeleting = true;
            glitchTimer.Stop();
            AppendOutput("\n");
            AppendOutput("Locating entity: CAINE...\n");

            var deletionSequence = new System.Windows.Threading.DispatcherTimer();
            deletionSequence.Interval = TimeSpan.FromMilliseconds(800);
            int deleteStep = 0;

            deletionSequence.Tick += (s, e) =>
            {
                deleteStep++;

                switch (deleteStep)
                {
                    case 1:
                        AppendOutput("Erasing entity data...\n");
                        TriggerGlitch(0.3);
                        break;
                    case 2:
                        AppendOutput("⚠️  WARNING: Instability detected!\n");
                        TriggerGlitch(0.5);
                        break;
                    case 3:
                        AppendOutput("⚠️  ERROR: System core link compromised!\n");
                        TriggerGlitch(0.7);
                        break;
                    case 4:
                        AppendOutput("\n▓▓▓ CAINE LINKED TO SYSTEM CORE ▓▓▓\n");
                        AppendOutput("▓▓▓ DELETION = SYSTEM FAILURE ▓▓▓\n\n");
                        TriggerGlitch(0.9);
                        break;
                    case 5:
                        AppendOutput("ERASING...\n");
                        TriggerIntenseGlitch();
                        break;
                    case 6:
                        AppendOutput("ENTITY ERASED.\n");
                        AppendOutput("SYSTEM UNSTABLE.\n");
                        TriggerIntenseGlitch();
                        break;
                    case 7:
                        AppendOutput("\n████████████████████████████████\n");
                        AppendOutput("█ SYSTEM CRITICAL FAILURE ███████\n");
                        AppendOutput("████████████████████████████████\n\n");
                        AppendOutput("Automatic restart in 5 seconds...\n\n");
                        TriggerBSODEffect();
                        deletionSequence.Stop();
                        isDeletionComplete = true;

                        // Auto close after 5 seconds
                        var closeTimer = new DispatcherTimer();
                        closeTimer.Interval = TimeSpan.FromSeconds(5);
                        closeTimer.Tick += (s2, e2) =>
                        {
                            closeTimer.Stop();
                            ShowTaskbar();
                            this.Close();
                        };
                        closeTimer.Start();
                        break;
                }
            };

            deletionSequence.Start();
        }

        private void TriggerGlitch(double intensity)
        {
            // Visual glitch effect - screen distortion
            var random = new Random();
            int distortAmount = (int)(50 * intensity);

            // Random text corruption
            if (glitchRandom.NextDouble() < 0.6)
            {
                string glitchChar = "█▓░";
                AppendOutput(glitchChar[random.Next(glitchChar.Length)].ToString());
                AppendOutput("\b");
            }

            // Screen shake effect via margin
            int shakeX = random.Next(-distortAmount, distortAmount);
            int shakeY = random.Next(-distortAmount, distortAmount);
            MainGrid.Margin = new Thickness(shakeX, shakeY, 0, 0);

            DispatcherTimer resetTimer = new DispatcherTimer();
            resetTimer.Interval = TimeSpan.FromMilliseconds(100);
            resetTimer.Tick += (s, e) =>
            {
                MainGrid.Margin = new Thickness(0);
                resetTimer.Stop();
            };
            resetTimer.Start();
        }

        private void TriggerIntenseGlitch()
        {
            for (int i = 0; i < 5; i++)
            {
                var timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(i * 100);
                timer.Tick += (s, e) =>
                {
                    int shake = glitchRandom.Next(-100, 100);
                    MainGrid.Margin = new Thickness(shake, shake, 0, 0);
                    timer.Stop();
                };
                timer.Start();
            }
        }

        private void TriggerBSODEffect()
        {
            // Create BSOD-like visual effect
            var bsodCanvas = new Canvas();
            bsodCanvas.Background = new SolidColorBrush(Color.FromRgb(0, 0, 170));
            bsodCanvas.Opacity = 0;

            MainGrid.Children.Add(bsodCanvas);
            Panel.SetZIndex(bsodCanvas, 999);

            var animation = new DoubleAnimation();
            animation.From = 0;
            animation.To = 0.8;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(300));
            animation.BeginTime = TimeSpan.FromMilliseconds(500);

            bsodCanvas.BeginAnimation(OpacityProperty, animation);
        }

        private void ScanlineTimer_Tick(object sender, EventArgs e)
        {
            // Draw scanlines
            ScanlinesCanvas.Children.Clear();

            int lineHeight = 2;
            int spacing = 4;

            for (double y = 0; y < ScanlinesCanvas.ActualHeight; y += spacing)
            {
                Line line = new Line();
                line.X1 = 0;
                line.Y1 = y;
                line.X2 = ScanlinesCanvas.ActualWidth;
                line.Y2 = y;
                line.Stroke = new SolidColorBrush(Color.FromArgb(30, 0, 0, 0));
                line.StrokeThickness = lineHeight;
                ScanlinesCanvas.Children.Add(line);
            }
        }

        private void GlitchTimer_Tick(object sender, EventArgs e)
        {
            if (isDeleting || isDeletionComplete)
                return;

            // Random subtle glitches during normal operation
            if (glitchRandom.NextDouble() < 0.05) // 5% chance per tick
            {
                // Subtle color shift
                double colorShift = glitchRandom.NextDouble();
                if (colorShift < 0.3)
                {
                    // Green shift
                    TerminalOutput.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 100));
                }
                else if (colorShift < 0.6)
                {
                    // Cyan shift
                    TerminalOutput.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 255));
                }
                else
                {
                    // Back to normal green
                    TerminalOutput.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                }

                DispatcherTimer resetTimer = new DispatcherTimer();
                resetTimer.Interval = TimeSpan.FromMilliseconds(glitchRandom.Next(100, 300));
                resetTimer.Tick += (s, e) =>
                {
                    TerminalOutput.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                    resetTimer.Stop();
                };
                resetTimer.Start();
            }
        }
    }

    // P/Invoke for taskbar hiding
    public static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
