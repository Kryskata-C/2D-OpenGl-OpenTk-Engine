using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using OpenTK.Windowing.Desktop;
using OpenTKGame; // Assuming this namespace contains your Game class

namespace KRSTEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            // --- Console Setup ---
            try
            {
                // Added check for non-interactive sessions where size cannot be set
                if (!Console.IsOutputRedirected && !Console.IsInputRedirected)
                {
                    Console.SetWindowSize(90, 35); // Increased height slightly for more info
                    Console.SetBufferSize(90, 35);
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

            // --- Set Working Directory ---
            string exeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(exeDirectory))
            {
                try
                {
                    Directory.SetCurrentDirectory(exeDirectory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Failed to set working directory to '{exeDirectory}'. {ex.Message}");
                    // Decide if you want to exit here or try to continue
                    Console.ReadKey();
                    return;
                }
            }
            else
            {
                Console.WriteLine("[Error] Could not determine executable directory.");
                Console.ReadKey();
                return;
            }

            // --- Run Boot Sequence ---
            KRSTBootConsole.RunFullBootSequence();

            // --- Launch Game ---
            Console.WriteLine("\nLaunching KRST Engine Window...");
            Thread.Sleep(1500); // Pause before window opens
            Console.Clear(); // Clear the boot info before showing the game window

            using (Game game = new Game(900, 720, "KRST Engine"))
            {
                // Optional: Pass any loaded configuration or assets from boot sequence to the game constructor if needed
                game.Run();
            }

            Console.WriteLine("\nKRST Engine exited. Press any key to close console.");
            Console.ReadKey();
        }
    }

    static class KRSTBootConsole
    {
        // --- Colors ---
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

        // --- Asset Lists ---
        static List<string> systems = new List<string>();
        static List<string> shaders = new List<string>();
        static List<string> textures = new List<string>();
        static List<string> maps = new List<string>();

        // --- Animation Settings ---
        const int TypewriterDelayMs = 1;
        const int ProgressBarWidth = 40;
        const int StepDelayMs = 100; // General delay between steps
        const int LoadingSimDelayMs = 50; // Delay per item in loading simulation

        public static void RunFullBootSequence()
        {
            Console.CursorVisible = false;

            AnimatedTitleBar();
            Console.Beep(659, 125); // E5
            Thread.Sleep(StepDelayMs);

            AnimateAsciiWithSpinner();
            Console.Beep(783, 125); // G5
            Thread.Sleep(StepDelayMs);

            Console.Clear();
            DisplaySystemInfo();
            Thread.Sleep(StepDelayMs * 5); // Pause after system info

            LogSeparator("Asset Discovery");
            InitializeDynamicLists(); // Find assets
            Thread.Sleep(StepDelayMs * 2);

            LogSeparator("Loading Core Systems");
            LogItems(systems, SystemColor, "[SYSTEM ]");
            SimulateLoadingWithProgressBar("Systems", systems.Count, SystemColor);
            Console.Beep(523, 80); // C5

            LogSeparator("Compiling Shaders");
            LogItems(shaders, ShaderColor, "[SHADER ]");
            SimulateLoadingWithProgressBar("Shaders", shaders.Count, ShaderColor);
            Console.Beep(587, 80); // D5

            LogSeparator("Loading Textures");
            LogItems(textures, TextureColor, "[TEXTURE]");
            SimulateLoadingWithProgressBar("Textures", textures.Count, TextureColor);
            Console.Beep(659, 80); // E5

            LogSeparator("Loading Maps");
            LogItems(maps, MapColor, "[MAP    ]");
            SimulateLoadingWithProgressBar("Maps", maps.Count, MapColor);
            Console.Beep(783, 150); // G5 long

            LogSeparator("Initialization Complete");
            Console.ForegroundColor = SuccessColor; Console.WriteLine("\n [ OK ] All systems initialized successfully.\n");
            Console.ResetColor();
            Thread.Sleep(StepDelayMs);

            DisplayEngineFeatures();
            DisplayFooter();

            Console.CursorVisible = true;
        }

        static void LogSeparator(string title)
        {
            Console.ForegroundColor = MutedColor;
            int paddingLength = Math.Max(0, Console.BufferWidth - title.Length - 6);
            Console.WriteLine($"\n--- {title} {"-".PadRight(paddingLength, '-')} ");
            Console.ResetColor();
            Thread.Sleep(StepDelayMs / 2);
        }

        static void InitializeDynamicLists()
        {
            Console.ForegroundColor = InfoColor;
            Console.WriteLine(" Searching for assets in subdirectories...");
            Console.ResetColor();
            Thread.Sleep(StepDelayMs);

            LoadFiles("Systems", "*.txt", systems, SystemColor);
            LoadFiles("Shaders", "*.glsl", shaders, ShaderColor);
            LoadFiles("Textures", "*.png", textures, TextureColor);
            LoadFiles("Maps", "*.txt", maps, MapColor); // Assuming maps are txt for now
        }

        static void LoadFiles(string dirName, string searchPattern, List<string> list, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write($"   -> Checking '{dirName}' directory... ");
            try
            {
                if (!Directory.Exists(dirName))
                {
                    Console.ForegroundColor = WarningColor;
                    Console.WriteLine($"[NOT FOUND] Creating '{dirName}' directory.");
                    Directory.CreateDirectory(dirName);
                    Console.ResetColor();
                    return; // No files to add yet
                }

                var files = Directory.GetFiles(dirName, searchPattern);
                list.AddRange(files);
                Console.ForegroundColor = SuccessColor;
                Console.WriteLine($"[ OK ] Found {files.Length} file(s).");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ErrorColor;
                Console.WriteLine($"[ERROR] Could not access/create directory '{dirName}'. {ex.Message}");
            }
            finally
            {
                Console.ResetColor();
                Thread.Sleep(StepDelayMs / 2); 
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
            for (int line = 0; line < titleLines.Length; line++)
            {
                Console.SetCursorPosition(0, startLine + line);
                foreach (char c in titleLines[line])
                {
                    Console.Write(c);
                    if (!char.IsWhiteSpace(c)) // Only delay for visible characters
                        Thread.Sleep(TypewriterDelayMs);
                }
                // Ensure we move to the next line even if the loop finishes early
                if (line < titleLines.Length - 1) Console.WriteLine();
            }
            Console.ResetColor();
            Console.WriteLine("\n"); // Add space after title
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
            char[] spinner = { '|', '/', '-', '\\' };
            int spinnerIndex = 0;
            int startLine = Console.CursorTop;
            // Estimate number of lines used by the animation + spinner line
            int linesToClear = frames[0].Split('\n').Length + 1;

            for (int i = 0; i < frames.Length * 3; i++) // Loop longer for more spinning
            {
                // Clear previous frame area
                Console.SetCursorPosition(0, startLine);
                for (int k = 0; k < linesToClear; k++) // Clear the calculated number of lines
                {
                    // Use the corrected way to generate a blank line
                    Console.WriteLine(new string(' ', Console.BufferWidth > 0 ? Console.BufferWidth - 1 : 1));
                }
                Console.SetCursorPosition(0, startLine); // Reset cursor to the top of animation area


                Console.ForegroundColor = HighlightColor;
                Console.WriteLine(frames[i % frames.Length]); // Cycle through frames
                Console.ForegroundColor = InfoColor;
                Console.Write($" Initializing Core... {spinner[spinnerIndex]}"); // Write spinner on its own line

                spinnerIndex = (spinnerIndex + 1) % spinner.Length;
                Console.ResetColor();
                Thread.Sleep(150); // Animation speed
            }

            // Clear the animation area finally
            Console.SetCursorPosition(0, startLine);
            for (int k = 0; k < linesToClear; k++) // Clear the calculated number of lines
            {
                // Use the corrected way to generate a blank line
                Console.WriteLine(new string(' ', Console.BufferWidth > 0 ? Console.BufferWidth - 1 : 1));
            }
            Console.SetCursorPosition(0, startLine); // Reset cursor
        }


        static void DisplaySystemInfo()
        {
            LogSeparator("System Information");
            try
            {
                Console.ForegroundColor = InfoColor;
                Console.WriteLine($" OS Version:      {Environment.OSVersion}");
                Console.WriteLine($" 64-bit OS:       {Environment.Is64BitOperatingSystem}");
                Console.WriteLine($" Processor Count: {Environment.ProcessorCount}");
                // Getting total physical memory reliably requires platform-specific code or libraries.
                // Environment.WorkingSet gives memory used by this process, not total system RAM.
                Console.WriteLine($" Process Memory:  {(Environment.WorkingSet / 1024.0 / 1024.0):F2} MB");
                Console.WriteLine($" CLR Version:     {Environment.Version}");
                Console.WriteLine($" Machine Name:    {Environment.MachineName}");
                Console.WriteLine($" User Name:       {Environment.UserName}");
                Console.WriteLine($" Current Dir:     {Environment.CurrentDirectory}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ErrorColor;
                Console.WriteLine($" [ERROR] Failed to retrieve some system information: {ex.Message}");
                Console.ResetColor();
            }
        }


        static void LogItems(List<string> items, ConsoleColor color, string tag)
        {
            if (items.Count == 0)
            {
                Console.ForegroundColor = MutedColor;
                Console.WriteLine($" {tag} -> No items found.");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = color;
            foreach (var item in items)
            {
                // Shorten long paths if needed
                string displayPath = item.Length > Console.BufferWidth - tag.Length - 5
                    ? "..." + item.Substring(item.Length - (Console.BufferWidth - tag.Length - 8))
                    : item;
                Console.WriteLine($" {tag} -> {Path.GetFileName(displayPath)}"); // Just show filename
                Thread.Sleep(10); // Tiny delay per item log
            }
            Console.ResetColor();
        }

        static void SimulateLoadingWithProgressBar(string type, int totalItems, ConsoleColor color)
        {
            if (totalItems == 0)
            {
                // Skip progress bar if nothing to load
                Console.ForegroundColor = MutedColor;
                Console.WriteLine($" [SKIP] No {type} to load.");
                Console.ResetColor();
                Thread.Sleep(StepDelayMs / 2);
                return;
            }

            Console.ForegroundColor = color;
            Console.Write($" [LOAD] {type,-10} ");
            int cursorLeft = Console.CursorLeft;
            int cursorTop = Console.CursorTop;

            for (int i = 0; i <= totalItems; i++)
            {
                float progress = totalItems == 0 ? 1.0f : (float)i / totalItems;
                DrawProgressBar(progress, ProgressBarWidth, cursorLeft, cursorTop, ProgressColor);
                // Simulate work being done per item
                Thread.Sleep(LoadingSimDelayMs / (totalItems / 10 + 1)); // Faster progress for more items
            }

            Console.SetCursorPosition(cursorLeft + ProgressBarWidth + 8, cursorTop); // Position after progress bar + percentage
            Console.ForegroundColor = SuccessColor;
            Console.WriteLine("[DONE]");
            Console.ResetColor();
            Thread.Sleep(StepDelayMs / 2);
        }

        static void DrawProgressBar(float progress, int width, int left, int top, ConsoleColor color)
        {
            Console.SetCursorPosition(left, top);
            Console.ForegroundColor = color;
            Console.Write("[");
            int filledWidth = (int)(progress * width);
            Console.Write(new string('█', filledWidth)); // Use block character '█'
            Console.Write(new string('-', width - filledWidth)); // Use '-' for empty part
            Console.Write("]");
            // Display percentage - ensure it fits and overwrite previous %
            Console.Write($" {(int)(progress * 100),3}% ");
            Console.ResetColor();
        }

        static void DisplayEngineFeatures()
        {
            Console.ForegroundColor = HighlightColor; Console.WriteLine(" KRST ENGINE FEATURES:");
            Console.ForegroundColor = InfoColor;
            Console.WriteLine("  - Map Editor [F1], Contextual Placement, Numpad Selection, Quick Save [F9]");
            Console.WriteLine("  - Disable/Enable lighting [L]");
            // Add any other key features here
            Console.WriteLine("  - Basic Console Boot Sequence");
            Console.WriteLine();
            Console.ResetColor();
            Thread.Sleep(StepDelayMs);
        }

        static void DisplayFooter()
        {
            Console.ForegroundColor = MutedColor;
            Console.WriteLine("══════════════════════════════════════════════════════════════════════════════════");
            Console.ForegroundColor = SystemColor;
            Console.WriteLine("                            KRST ENGINE");
            Console.ForegroundColor = InfoColor;
            Console.WriteLine("                       Powering 2D Experiences");
            Console.WriteLine();
            Console.ForegroundColor = MutedColor;
            Console.WriteLine($"             @ {DateTime.Now.Year} KRST Interactive. All rights reserved."); // Use current year
            Console.WriteLine("                Designed & Engineered by Kryskata");
            Console.ForegroundColor = MutedColor;
            Console.WriteLine("══════════════════════════════════════════════════════════════════════════════════");
            Console.ResetColor();
        }
    }
}