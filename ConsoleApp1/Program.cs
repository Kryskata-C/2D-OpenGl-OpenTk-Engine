using System.IO;
using OpenTK.Windowing.Desktop;
using OpenTKGame;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;

namespace KRSTEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(90, 30);
            Console.SetBufferSize(90, 30);
            string exeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(exeDirectory))
            {
                Directory.SetCurrentDirectory(exeDirectory);
            }
            KRSTBootConsole.RunFullBootSequence();
            using Game game = new Game(900, 720, "KRST Engine");
            game.Run();
        }
    }

    static class KRSTBootConsole
    {
        static ConsoleColor InfoColor = ConsoleColor.Gray;
        static ConsoleColor SystemColor = ConsoleColor.Cyan;
        static ConsoleColor ShaderColor = ConsoleColor.Magenta;
        static ConsoleColor TextureColor = ConsoleColor.Yellow;
        static ConsoleColor MapColor = ConsoleColor.Green;
        static List<string> shaders = new List<string>();
        static List<string> textures = new List<string>();
        static List<string> systems = new List<string>();
        static List<string> maps = new List<string>();

        public static void RunFullBootSequence()
        {
            TitleBar();
            AnimateAscii();
            Console.Beep(523, 100);
            InitializeDynamicLists();
            ShowProgressBar();
            LogHeader("Initializing Systems");
            foreach (var system in systems) Log(system, SystemColor, "[SYSTEM]");
            LogHeader("Compiling Shaders");
            foreach (var shader in shaders) Log(shader, ShaderColor, "[SHADER]");
            LogHeader("Loading Textures");
            foreach (var tex in textures) Log(tex, TextureColor, "[TEXTURE]");
            LogHeader("Loading Maps");
            foreach (var map in maps) Log(map, MapColor, "[MAP]");
            Thread.Sleep(300);
            Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("\nAll systems initialized successfully.\n");
            Console.WriteLine("KRST ENGINE FEATURES:");
            Console.WriteLine(" - Map Editor [F1], Contextual Placement, Numpad Selection, Quick Save [F9]");
            Console.WriteLine(" - Disable/Enable lighting [L]");

            Console.WriteLine(); Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("─────────────────────────────────────────────────────────────");
            Console.WriteLine("  KRST ENGINE - Powering Next-Gen Experiences");
            Console.WriteLine("  © 2025 KRST Interactive. All rights reserved.");
            Console.WriteLine("  Designed by Kryskata");
            Console.WriteLine("─────────────────────────────────────────────────────────────");
        }

        static void InitializeDynamicLists()
        {
            if (!Directory.Exists("Systems")) Directory.CreateDirectory("Systems");
            if (!Directory.Exists("Shaders")) Directory.CreateDirectory("Shaders");
            if (!Directory.Exists("Textures")) Directory.CreateDirectory("Textures");
            if (!Directory.Exists("Maps")) Directory.CreateDirectory("Maps");
            systems.AddRange(Directory.GetFiles("Systems", "*.txt"));
            shaders.AddRange(Directory.GetFiles("Shaders", "*.glsl"));
            textures.AddRange(Directory.GetFiles("Textures", "*.png"));
            maps.AddRange(Directory.GetFiles("Maps", "*.txt"));
        }

        static void TitleBar()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(" ___  __    ________  ________  _________        _______   ________   ________  ___  ________   _______      ");
            Console.WriteLine("|\\  \\|\\  \\ |\\   __  \\|\\   ____\\|\\___   ___\\     |\\  ___ \\ |\\   ___  \\|\\   ____\\|\\  \\|\\   ___  \\|\\  ___ \\     ");
            Console.WriteLine("\\ \\  \\/  /|\\ \\  \\|\\  \\ \\  \\___|\\|___ \\  \\_|     \\ \\   __/|\\ \\  \\\\ \\  \\ \\  \\___|\\ \\  \\ \\  \\\\ \\  \\ \\   __/|    ");
            Console.WriteLine(" \\ \\   ___  \\ \\   _  _\\ \\_____  \\   \\ \\  \\       \\ \\  \\_|/_\\ \\  \\\\ \\  \\ \\  \\  __\\ \\  \\ \\  \\\\ \\  \\ \\  \\_|/__  ");
            Console.WriteLine("  \\ \\  \\\\ \\  \\ \\  \\\\  \\\\|____|\\  \\   \\ \\  \\       \\ \\  \\_|\\ \\ \\  \\\\ \\  \\ \\  \\|\\  \\ \\  \\ \\  \\\\ \\  \\ \\  \\_|\\ \\ ");
            Console.WriteLine("   \\ \\__\\\\ \\__\\ \\__\\\\ _\\|____|\\__\\   \\ \\__\\        \\ \\_______\\ \\__\\\\ \\__\\ \\_______\\ \\__\\ \\__\\\\ \\__\\ \\_______\\");
            Console.WriteLine("    \\|__| \\|__|\\|__|\\|__|\\_________\\   \\|__|        \\|_______|\\|__| \\|__|\\|_______|\\|__|\\|__| \\|__|\\|_______|");
            Console.WriteLine("                        \\|_________|                                                                           ");
            Console.ResetColor(); Console.WriteLine();
        }

        static void LogHeader(string title)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n--== " + title + " ==--\n");
        }

        static void Log(string message, ConsoleColor color, string tag)
        {
            Console.ForegroundColor = color; Console.WriteLine($"{tag,-10} -> {Path.GetFileName(message)}");
        }

        static void AnimateAscii()
        {
            string[] frames =
            {
@"        \o/        _______
         |         |Kryskata|
        / \        ---------
",
@"         o/        _______
         |         |Kryskata|
         / \       ---------
",
@"         o         _______
        /|         |Kryskata|
        / \        ---------
",
@"        \o         _______
         |\        |Kryskata|
         / \       ---------
",
@"        \o/        _______
         |         |Kryskata|
        / \        ---------
"
        };
            for (int i = 0; i < frames.Length; i++)
            {
                Console.Clear(); TitleBar();
                Console.WriteLine(frames[i]);
                Thread.Sleep(250);
            }
            Console.Clear(); TitleBar();
        }

        static void ShowProgressBar()
        {
            int total = systems.Count + shaders.Count + textures.Count + maps.Count;
            if (total == 0) return;
            for (int i = 1; i <= total; i++)
            {
                float percent = (float)i / total * 100f;
                int bars = (int)(percent / 5f);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\r[");
                for (int j = 0; j < bars; j++) Console.Write("█");
                for (int j = bars; j < 20; j++) Console.Write("-");
                Console.Write("] " + (int)percent + "%");
                Thread.Sleep(25);
            }
            Console.ResetColor(); Console.WriteLine("\n");
        }
    }
}
