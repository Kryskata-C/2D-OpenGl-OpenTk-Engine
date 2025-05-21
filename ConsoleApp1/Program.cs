using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using OpenTK.Windowing.Desktop;
using OpenTKGame;
using System.Threading;

namespace OpenTK_Project
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (!Console.IsOutputRedirected && !Console.IsInputRedirected)
                {
                    Console.SetWindowSize(Math.Min(100, Console.LargestWindowWidth), Math.Min(40, Console.LargestWindowHeight));
                    Console.SetBufferSize(Math.Min(100, Console.LargestWindowWidth), Math.Min(4000, (int)short.MaxValue));
                }
                Console.Title = "KRST Engine Booting...";
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[Warning] Could not set console size. Maybe running in an unsupported terminal? {ex.Message}");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"[Warning] Console size might be too large for the screen. {ex.Message}");
            }

            string exeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(exeDirectory))
            {
                Console.WriteLine("[Critical Error] Could not determine executable directory. Press any key to exit.");
                Console.ReadKey();
                return;
            }

            KRSTBootConsole.RunFullBootSequence(exeDirectory);

            Console.WriteLine("\nLaunching KRST Engine Window...");
            Thread.Sleep(1500);
            Console.Clear();

            using (Game game = new Game(900, 720, "KRST Engine"))
            {
                game.Run();
            }

            Console.WriteLine("\nKRST Engine exited. Press any key to close console.");
            Console.ReadKey();
        }
    }

    static class KRSTBootConsole
    {
        static ConsoleColor TitleColor = ConsoleColor.DarkCyan;
        static ConsoleColor InfoColor = ConsoleColor.Gray;
        static ConsoleColor SystemColor = ConsoleColor.Cyan;
        static ConsoleColor ShaderColor = ConsoleColor.Magenta;
        static ConsoleColor TextureColor = ConsoleColor.Yellow;
        static ConsoleColor MapColor = ConsoleColor.Green;
        static ConsoleColor ProgressColor = ConsoleColor.DarkGreen;
        static ConsoleColor WarningColor = ConsoleColor.DarkYellow;
        static ConsoleColor ErrorColor = ConsoleColor.Red;
        static ConsoleColor SuccessColor = ConsoleColor.Green;
        static ConsoleColor HighlightColor = ConsoleColor.White;
        static ConsoleColor MutedColor = ConsoleColor.DarkGray;
        static ConsoleColor AccentColor1 = ConsoleColor.Blue;

        static List<string> systems = new List<string>();
        static List<string> shaders = new List<string>();
        static List<string> textures = new List<string>();
        static List<string> maps = new List<string>();

        const int TypewriterDelayMs = 0;
        const int ProgressBarWidth = 45;
        const int StepDelayMs = 80;
        const int LoadingSimDelayMs = 30;

        static string projectRootPath;
        static string exePath;
        static string projectName = "KRST Engine 2D";

        public static void RunFullBootSequence(string executablePath)
        {
            Console.CursorVisible = false;
            exePath = executablePath;
            projectRootPath = Path.GetFullPath(Path.Combine(exePath, "..", "..", ".."));
            projectName = Path.GetFileName(projectRootPath);

            AnimatedTitleBar();
            Console.Beep(659, 100);
            Thread.Sleep(StepDelayMs);

            AnimateAsciiWithSpinner();
            Console.Beep(783, 100);
            Thread.Sleep(StepDelayMs);

            Console.Clear();
            DisplaySystemInfo();
            Thread.Sleep(StepDelayMs * 3);

            LogSeparator("Asset Discovery & Initialization", AccentColor1);
            InitializeDynamicLists();
            Thread.Sleep(StepDelayMs * 2);

            LogSeparator("Loading Core Systems", SystemColor);
            LogItems(systems, SystemColor, "[SYSTEM ]", false);
            SimulateLoadingWithProgressBar("Systems", systems.Count, SystemColor);
            Console.Beep(523, 70);

            LogSeparator("Compiling Shaders", ShaderColor);
            LogItems(shaders, ShaderColor, "[SHADER ]");
            SimulateLoadingWithProgressBar("Shaders", shaders.Count, ShaderColor);
            Console.Beep(587, 70);

            LogSeparator("Loading Textures", TextureColor);
            LogItems(textures, TextureColor, "[TEXTURE]");
            SimulateLoadingWithProgressBar("Textures", textures.Count, TextureColor);
            Console.Beep(659, 70);

            LogSeparator("Loading Maps", MapColor);
            LogItems(maps, MapColor, "[MAP    ]");
            SimulateLoadingWithProgressBar("Maps", maps.Count, MapColor);
            Console.Beep(783, 120);

            LogSeparator("Finalizing Setup", SuccessColor);
            Console.ForegroundColor = SuccessColor; Console.WriteLine("\n [ OK ] All systems initialized successfully.\n");
            Console.ResetColor();
            Thread.Sleep(StepDelayMs);

            DisplayEngineFeatures();
            DisplayFooter();

            Console.CursorVisible = true;
        }

        static void LogSeparator(string title, ConsoleColor color)
        {
            Console.WriteLine();
            Console.ForegroundColor = color;
            int paddingLength = Math.Max(0, (Console.BufferWidth - title.Length - 2) / 2);
            string line = new string('═', paddingLength);
            Console.WriteLine($"{line} {title} {line}".Substring(0, Math.Min(Console.BufferWidth - 1, $"{line} {title} {line}".Length)));
            Console.ResetColor();
            Console.WriteLine();
            Thread.Sleep(StepDelayMs / 2);
        }

        static void InitializeDynamicLists()
        {
            Console.ForegroundColor = InfoColor;
            Console.WriteLine($" Searching for assets in project: '{projectName}' (Path: {projectRootPath})");
            Console.ResetColor();
            Thread.Sleep(StepDelayMs);

            string shadersDir = Path.Combine(exePath, "Shaders");
            string texturesProjectDir = Path.Combine(projectRootPath, "ConsoleApp1", "Textures");
            string mapsProjectDir = Path.Combine(projectRootPath, "ConsoleApp1", "Maps");
            string systemsProjectDir = Path.Combine(projectRootPath, "ConsoleApp1");

            LoadFiles("Core Systems", systemsProjectDir, "*.cs", systems, SystemColor, false);
            LoadFiles("Shaders", shadersDir, "*.glsl", shaders, ShaderColor, false);
            LoadFiles("Textures", texturesProjectDir, "*.png", textures, TextureColor, true);
            LoadFiles("Maps", mapsProjectDir, "*.txt", maps, MapColor, false);
        }

        static void LoadFiles(string dirDisplayName, string directoryPath, string searchPattern, List<string> list, ConsoleColor color, bool recursive)
        {
            Console.ForegroundColor = color;
            Console.Write($"   -> Scanning '{dirDisplayName}' ({Path.GetFileName(directoryPath)})... ");
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Console.ForegroundColor = WarningColor;
                    Console.WriteLine($"[NOT FOUND] Directory '{directoryPath}' does not exist.");
                    Console.ResetColor();
                    return;
                }

                var files = Directory.GetFiles(directoryPath, searchPattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                list.AddRange(files.Select(f => Path.GetFileName(f)));

                Console.ForegroundColor = SuccessColor;
                Console.WriteLine($"[ OK ] Found {files.Length} file(s).");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ErrorColor;
                Console.WriteLine($"[ERROR] Could not access directory '{directoryPath}'. {ex.Message.Split('\n')[0]}");
            }
            finally
            {
                Console.ResetColor();
                Thread.Sleep(StepDelayMs / 3);
            }
        }

        static void AnimatedTitleBar()
        {
            string[] titleLines = {
                @" ___  __    ________  ________  _________     _______   ________   ________  ___  ________   _______    ",
                @"|\  \|\  \ |\   __  \|\   ____\|\___   ___\   |\  ___ \ |\   ___  \|\   ____\|\  \|\   ___  \|\  ___ \   ",
                @"\ \  \/  /|\ \  \|\  \ \  \___|\|___ \  \_|   \ \   __/|\ \  \\ \  \ \  \___|\ \  \ \  \\ \  \ \   __/|  ",
                @" \ \   ___  \ \   _  _\ \_____  \   \ \  \     \ \  \_|/_\ \  \\ \  \ \  \  __\ \  \ \  \\ \  \ \  \_|/__ ",
                @"  \ \  \\ \  \ \  \\  \\|____|\  \   \ \  \     \ \  \_|\ \ \  \\ \  \ \  \|\  \ \  \ \  \\ \  \ \  \_|\ \",
                @"   \ \__\\ \__\ \__\\ _\|____|\__\   \ \__\      \ \_______\ \__\\ \__\ \_______\ \__\ \__\\ \__\ \_______\",
                @"    \|__| \|__|\|__|\|__|\_________|   \|__|      \|_______|\|__| \|__|\|_______|\|__|\|__| \|__|\|_______|",
                @"                         \|_________|                                                                    "
            };

            Console.ForegroundColor = TitleColor;
            int startLine = Console.CursorTop;
            if (startLine + titleLines.Length >= Console.BufferHeight) Console.Clear();
            startLine = Console.CursorTop;

            for (int line = 0; line < titleLines.Length; line++)
            {
                Console.SetCursorPosition(0, startLine + line);
                string currentLine = titleLines[line];
                if (currentLine.Length >= Console.BufferWidth)
                {
                    currentLine = currentLine.Substring(0, Console.BufferWidth - 1);
                }
                Console.Write(currentLine);
                Thread.Sleep(TypewriterDelayMs * 5);
            }
            Console.ResetColor();
            Console.WriteLine("\n");
        }

        static void AnimateAsciiWithSpinner()
        {
            string[] frames =
            {
@"
         \o/       _______
          |        |KRST   |
         / \       ---------
",
@"
          o/       _______
          |        | KRST  |
         / \       ---------
",
@"
           o       _______
          /|       |  KRST |
          / \      ---------
",
@"
         \o        _______
          |\       |   KRST|
         / \       ---------
",
@"
         \o/       _______
          |        |KRST   |
         / \       ---------
"
            };
            char[] spinnerChars = { '|', '/', '-', '\\', '*', '+' };
            int spinnerIndex = 0;
            int startLine = Console.CursorTop;
            int frameHeight = frames[0].Split('\n').Length;

            if (startLine + frameHeight + 1 >= Console.BufferHeight) Console.Clear();
            startLine = Console.CursorTop;

            for (int i = 0; i < frames.Length * 2; i++)
            {
                Console.SetCursorPosition(0, startLine);
                string currentFrame = frames[i % frames.Length];
                using (StringReader reader = new StringReader(currentFrame))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Console.WriteLine(line.PadRight(Console.BufferWidth - 1));
                    }
                }

                Console.ForegroundColor = InfoColor;
                Console.Write($" Initializing Core... {spinnerChars[spinnerIndex]}".PadRight(Console.BufferWidth - 1));

                spinnerIndex = (spinnerIndex + 1) % spinnerChars.Length;
                Console.ResetColor();
                Thread.Sleep(120);
            }
            Console.SetCursorPosition(0, startLine);
            for (int i = 0; i < frameHeight + 1; ++i) Console.WriteLine(new string(' ', Console.BufferWidth - 1));
            Console.SetCursorPosition(0, startLine);
        }

        static void DisplaySystemInfo()
        {
            LogSeparator("System & Environment", AccentColor1);
            try
            {
                Console.ForegroundColor = InfoColor;
                string GetBuildConfiguration()
                {
#if DEBUG
                    return "DEBUG";
#else
                    return "RELEASE";
#endif
                }

                Console.WriteLine($" OS Version:      {Environment.OSVersion.VersionString.Split(new[] { "Microsoft " }, StringSplitOptions.None).LastOrDefault()}");
                Console.WriteLine($" Platform:        {(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")} OS, {(Environment.Is64BitProcess ? "64-bit" : "32-bit")} Process");
                Console.WriteLine($" Processor Count: {Environment.ProcessorCount}");
                Console.WriteLine($" Process Memory:  {(Environment.WorkingSet / 1024.0 / 1024.0):F2} MB");
                Console.WriteLine($" CLR Version:     {Environment.Version}");
                Console.WriteLine($" Machine Name:    {Environment.MachineName}");
                Console.WriteLine($" User Name:       {Environment.UserName}");
                Console.WriteLine($" Project Path:    {projectRootPath}");
                Console.WriteLine($" Executable Path: {exePath}");
                Console.ForegroundColor = HighlightColor;
                Console.WriteLine($" Build Mode:      {GetBuildConfiguration()}");

                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ErrorColor;
                Console.WriteLine($" [ERROR] Failed to retrieve some system information: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void LogItems(List<string> items, ConsoleColor color, string tag, bool showFullPath = true)
        {
            if (items.Count == 0)
            {
                Console.ForegroundColor = MutedColor;
                Console.WriteLine($" {tag} -> No items found.");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = color;
            int maxItemsToShow = 10;
            foreach (var item in items.Take(maxItemsToShow))
            {
                string displayPath = showFullPath ? item : Path.GetFileName(item);
                if (displayPath.Length > Console.BufferWidth - tag.Length - 5)
                {
                    displayPath = "..." + displayPath.Substring(Math.Max(0, displayPath.Length - (Console.BufferWidth - tag.Length - 8)));
                }
                Console.WriteLine($" {tag} -> {displayPath}");
                Thread.Sleep(5);
            }
            if (items.Count > maxItemsToShow)
            {
                Console.ForegroundColor = MutedColor;
                Console.WriteLine($" {tag} -> ...and {items.Count - maxItemsToShow} more.");
            }
            Console.ResetColor();
        }

        static void SimulateLoadingWithProgressBar(string type, int totalItems, ConsoleColor color)
        {
            if (totalItems == 0)
            {
                Console.ForegroundColor = MutedColor;
                Console.WriteLine($" [SKIP] No {type} to load.");
                Console.ResetColor();
                Thread.Sleep(StepDelayMs / 2);
                return;
            }

            Console.ForegroundColor = color;
            Console.Write($" [LOAD] {type,-10} ");
            int barLeft = Console.CursorLeft;
            int barTop = Console.CursorTop;

            for (int i = 0; i <= totalItems; i++)
            {
                float progress = totalItems == 0 ? 1.0f : (float)i / totalItems;
                DrawProgressBar(progress, ProgressBarWidth, barLeft, barTop, ProgressColor, color);
                Thread.Sleep(Math.Max(5, LoadingSimDelayMs / (totalItems / 5 + 1)));
            }

            Console.SetCursorPosition(barLeft + ProgressBarWidth + 2 + 6, barTop);
            Console.ForegroundColor = SuccessColor;
            Console.WriteLine("[DONE]");
            Console.ResetColor();
            Thread.Sleep(StepDelayMs / 2);
        }

        static void DrawProgressBar(float progress, int width, int left, int top, ConsoleColor progressFillColor, ConsoleColor barFrameColor)
        {
            Console.SetCursorPosition(left, top);
            Console.ForegroundColor = barFrameColor;
            Console.Write("[");

            Console.ForegroundColor = progressFillColor;
            int filledWidth = (int)(progress * width);
            Console.Write(new string('█', filledWidth));

            Console.ForegroundColor = MutedColor;
            Console.Write(new string('░', width - filledWidth));

            Console.ForegroundColor = barFrameColor;
            Console.Write("]");

            Console.ForegroundColor = HighlightColor;
            Console.Write($" {(int)(progress * 100),3}% ");
            Console.ResetColor();
        }

        static void DisplayEngineFeatures()
        {
            Console.ForegroundColor = HighlightColor; Console.WriteLine("\n KRST ENGINE FEATURES:");
            Console.ForegroundColor = InfoColor;
            Console.WriteLine("  - In-Game Map Editor [F1]");
            Console.WriteLine("    - Contextual Tile Placement (Left Mouse)");
            Console.WriteLine("    - Tile Deletion (Middle Mouse)");
            Console.WriteLine("    - Object Palette Selection (Right Mouse + Numpad)");
            Console.WriteLine("  - Quick Save Map [F9]");
            Console.WriteLine("  - Toggle Lighting Effects [L]");
            Console.WriteLine("  - Player Animations (Idle, Walk, Reload [R])");
            Console.WriteLine("  - Dynamic Console Boot Sequence");
            Console.ResetColor();
            Thread.Sleep(StepDelayMs);
        }

        static void DisplayFooter()
        {
            Console.WriteLine();
            Console.ForegroundColor = MutedColor;
            Console.WriteLine("═════════════════════════════════════════════════════════════════════════════════════════".Substring(0, Math.Min(Console.BufferWidth - 1, 80)));
            Console.ForegroundColor = SystemColor;
            Console.WriteLine($"                            KRST ENGINE ({projectName})");
            Console.ForegroundColor = InfoColor;
            Console.WriteLine("                       Powering 2D Experiences");
            Console.WriteLine();
            Console.ForegroundColor = MutedColor;
            Console.WriteLine($"             © {DateTime.Now.Year} KRST Interactive. All rights reserved.");
            Console.WriteLine("                Designed & Engineered by Kryskata");
            Console.WriteLine("═════════════════════════════════════════════════════════════════════════════════════════".Substring(0, Math.Min(Console.BufferWidth - 1, 80)));
            Console.ResetColor();
        }
    }
}