using OpenTK.Windowing.Desktop;
using OpenTKGame;
using System;
using System.Collections.Generic;
using System.Threading;

namespace OpenTkProject
{
    class Program
    {
        static void Main(string[] args)
        {
            KryskataBootConsole.RunFullBootSequence();

            using Game game = new Game(800, 600, "LearnOpenTK");
            game.Run();
        }
    }

    static class KryskataBootConsole
    {
        static readonly ConsoleColor InfoColor = ConsoleColor.Gray;
        static readonly ConsoleColor SystemColor = ConsoleColor.Cyan;
        static readonly ConsoleColor ShaderColor = ConsoleColor.Magenta;
        static readonly ConsoleColor TextureColor = ConsoleColor.Yellow;
        static readonly ConsoleColor MapColor = ConsoleColor.Green;

        static List<string> shaders = new() {
            "VertexShader.glsl", "FragmentShader.glsl", "DebugShader.glsl"
        };

        static List<string> textures = new() {
            "tileable_grass.png", "box.png", "GreenSquare.png", "SoliderRotated.png", "shotgun2.png"
        };

        static List<string> systems = new() {
            "Input System", "Shader Compiler", "Texture Loader", "Map Parser", "Collision Engine",
            "Editor Toolchain", "Memory Manager", "Asset Pipeline", "GL Device Interface"
        };

        static List<string> maps = new() {
            "MainMap.txt"
        };

        public static void RunFullBootSequence()
        {
            TitleBar();

            LogHeader("Initializing Systems");
            foreach (var system in systems)
                Log(system, SystemColor, "[SYSTEM]");

            LogHeader("Compiling Shaders");
            foreach (var shader in shaders)
                Log(shader, ShaderColor, "[SHADER]");

            LogHeader("Loading Textures");
            foreach (var tex in textures)
                Log(tex, TextureColor, "[TEXTURE]");

            LogHeader("Loading Maps");
            foreach (var map in maps)
                Log(map, MapColor, "[MAP]");

            Thread.Sleep(300);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\nAll systems initialized successfully.\n");
            PrintKryskataSignature();

            Console.WriteLine("\nKRST ENGINE FEATURES:");
            Console.WriteLine(" - Map Editor [F1]: Toggle in-game map edit mode");
            Console.WriteLine(" - Contextual Placement: Right-click to choose blocks, Left-click to place, Middle-click to remove");
            Console.WriteLine(" - Numpad Selection: Choose objects easily with numbers");
            Console.WriteLine(" - Save to MainMap with F9");
            Console.WriteLine("\nPress any key to continue...");
            Console.ResetColor();
            Console.ReadKey(true);
        }

        static void LogHeader(string title)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n--== " + title + " ==--\n");
        }

        static void Log(string message, ConsoleColor color, string tag)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"{tag,-10} -> {message}");
        }

        static void TitleBar()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│                   KRST ENGINE v1.0 BOOTING                   │");
            Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
            Console.ResetColor();
        }

        static void PrintKryskataSignature()
        {
            string[] K = { "██   ██ ", "██  ██  ", "█████   ", "██  ██  ", "██   ██ " };
            string[] R = { "█████  ", "██  ██ ", "█████  ", "██  ██ ", "██   ██" };
            string[] Y = { "██    ██", " ██  ██ ", "  ████  ", "   ██   ", "   ██   " };
            string[] S = { " █████ ", "██     ", " ████  ", "    ██ ", "█████  " };
            string[] A = { "  ███  ", " ██ ██ ", "██████ ", "██   ██", "██   ██" };
            string[] T = { "████████", "   ██   ", "   ██   ", "   ██   ", "   ██   " };
            string[][] letters = { K, R, Y, S, K, A, T, A };

            Console.WriteLine();
            for (int row = 0; row < 5; row++)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                foreach (var letter in letters)
                    Console.Write(letter[row].Replace('█', '▓') + " ");
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Cyan;
                foreach (var letter in letters)
                    Console.Write(letter[row] + " ");
                Console.WriteLine();
            }
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}