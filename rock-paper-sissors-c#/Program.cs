using System;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace RockPaperScissors
{
    public class HMACHelper
    {
        public static string GenerateKey()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[32];
                rng.GetBytes(bytes);
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        public static string CalculateHMAC(string key, string message)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }

    public class Rules
    {
        private readonly string[] _moves;
        private readonly int _n;
        private readonly int _p;

        public Rules(string[] moves)
        {
            _moves = moves;
            _n = moves.Length;
            _p = _n / 2;
        }

        public string DetermineOutcome(string computerMove, string userMove)
        {
            var a = Array.IndexOf(_moves, computerMove);
            var b = Array.IndexOf(_moves, userMove);
            var result = Math.Sign((a - b + _p + _n) % _n - _p);
            if (result == 0) return "Draw";
            return result > 0 ? "Computer wins" : "You win";
        }
    }

    public class HelpTable
    {
        private readonly string[] _moves;

        public HelpTable(string[] moves)
        {
            _moves = moves;
        }

        public string GenerateTable()
        {
            var table = new System.Text.StringBuilder();
            table.AppendLine("PC\\User\t" + string.Join("\t", _moves));
            var rules = new Rules(_moves);
            foreach (var move in _moves)
            {
                table.Append(move + "\t");
                foreach (var userMove in _moves)
                {
                    table.Append(rules.DetermineOutcome(move, userMove) + "\t");
                }
                table.AppendLine();
            }
            return table.ToString();
        }
    }

    public class Game
    {
        private readonly string[] _moves;
        private readonly Rules _rules;
        private readonly string _key;
        private readonly string _computerMove;
        private readonly string _hmac;

        public Game(string[] moves)
        {
            _moves = moves;
            _rules = new Rules(moves);
            _key = HMACHelper.GenerateKey();
            var random = new Random();
            _computerMove = _moves[random.Next(0, _moves.Length)];
            _hmac = HMACHelper.CalculateHMAC(_key, _computerMove);
        }

        public void Play()
        {
            Console.WriteLine($"HMAC: {_hmac}");
            Console.WriteLine("Available moves:");
            for (int i = 0; i < _moves.Length; i++)
            {
                Console.WriteLine($"{i + 1} - {_moves[i]}");
            }
            Console.WriteLine("0 - exit");
            Console.WriteLine("? - help");

            while (true)
            {
                Console.Write("Enter your move: ");
                var input = Console.ReadLine();
                if (input == "0")
                {
                    Console.WriteLine("Exiting the game.");
                    break;
                }
                else if (input == "?")
                {
                    var helpTable = new HelpTable(_moves);
                    Console.WriteLine(helpTable.GenerateTable());
                }
                else if (int.TryParse(input, out int moveIndex) && moveIndex > 0 && moveIndex <= _moves.Length)
                {
                    var userMove = _moves[moveIndex - 1];
                    Console.WriteLine($"Your move: {userMove}");
                    Console.WriteLine($"Computer move: {_computerMove}");
                    Console.WriteLine(_rules.DetermineOutcome(_computerMove, userMove));
                    Console.WriteLine($"HMAC key: {_key}");
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid Move! Please try again");
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Error: No moves provided. Please provide an odd number of non-repeating moves.");
            }
            else if (args.Length == 1)
            {
                Console.WriteLine("Error: Only one move provided. Please provide an odd number of non-repeating moves greater than 1.");
            }
            else if (args.Length % 2 == 0)
            {
                Console.WriteLine("Error: Even number of moves provided. Please provide an odd number of non-repeating moves.");
            }
            else if (args.Distinct().Count() != args.Length)
            {
                Console.WriteLine("Error: Duplicate moves found. Please provide non-repeating moves.");
            }
            else
            {
                var game = new Game(args);
                game.Play();
            }
        }
    }
}