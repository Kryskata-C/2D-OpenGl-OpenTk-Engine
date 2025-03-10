using OpenTK.Windowing.Desktop;
using OpenTKGame;
using System;

namespace OpenTkProject
{
    class Program
    {
        static void Main(string[] args)
        {
            PrintKryskataSignature();

            Console.WriteLine("Starting OpenTK game...");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            using Game game = new Game(800, 600, "LearnOpenTK");
            game.Run();
        }

        static void PrintKryskataSignature()
        {
            string[] K = {
                "██   ██ ",
                "██  ██  ",
                "█████   ",
                "██  ██  ",
                "██   ██ "
            };

            string[] R = {
                "█████  ",
                "██  ██ ",
                "█████  ",
                "██  ██ ",
                "██   ██"
            };

            string[] Y = {
                "██    ██",
                " ██  ██ ",
                "  ████  ",
                "   ██   ",
                "   ██   "
            };

            string[] S = {
                " █████ ",
                "██     ",
                " ████  ",
                "    ██ ",
                "█████  "
            };

            string[] A = {
                "  ███  ",
                " ██ ██ ",
                "██████ ",
                "██   ██",
                "██   ██"
            };

            string[] T = {
                "████████",
                "   ██   ",
                "   ██   ",
                "   ██   ",
                "   ██   "
            };

            string[][] letters = { K, R, Y, S, K, A, T, A };

            ConsoleColor originalForeground = Console.ForegroundColor;

            Console.WriteLine("\n=== OpenTK Project by ===");

            for (int row = 0; row < 5; row++)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.SetCursorPosition(2, Console.CursorTop + 1);

                foreach (var letter in letters)
                {
                    Console.Write(letter[row].Replace('█', '▓') + " ");
                }

                Console.SetCursorPosition(1, Console.CursorTop - 1);
                Console.ForegroundColor = ConsoleColor.Cyan;
                foreach (var letter in letters)
                {
                    Console.Write(letter[row] + " ");
                }

                Console.WriteLine();
            }

            Console.ForegroundColor = originalForeground;
            Console.WriteLine("\n// OpenGL Project by Kryskata");
            Console.WriteLine("// " + DateTime.Now.ToString("yyyy-MM-dd") + "\n");
        }
    }
}