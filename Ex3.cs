using System;
using System.Collections.Generic;
using System.Linq;

namespace Ex3
{
    public record Grade(string Course, int Score);

    public record AddedEntry(string Name, Grade Grade);

    public class Gradebook
    {
        private readonly Dictionary<string, List<Grade>> _grades =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, string> _displayNames =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> _courses =
            new(StringComparer.OrdinalIgnoreCase) { ".NET", "DB", "Math" };

        private readonly Stack<AddedEntry> _undoStack = new();
        private readonly Queue<string> _history = new();
        private const int HistoryLimit = 5;

        public string AddScore(string name, string course, int score)
        {
            name = name.Trim();
            course = course.Trim();

            if (name.Length == 0)
                return "Name cannot be empty";

            if (!_courses.TryGetValue(course, out string? realCourse))
                return $"Unknown course: {course}";

            if (score < 0 || score > 100)
                return $"Invalid score: {score}";

            if (!_grades.ContainsKey(name))
            {
                _grades[name] = new List<Grade>();
                _displayNames[name] = name;
            }

            string displayName = _displayNames[name];
            var grade = new Grade(realCourse, score);

            _grades[name].Add(grade);
            _undoStack.Push(new AddedEntry(displayName, grade));
            AddToHistory($"add {displayName} {realCourse} {score}");

            return $"Added: {displayName} {realCourse} {score}";
        }

        public string Undo()
        {
            if (_undoStack.Count == 0)
                return "Nothing to undo";

            AddedEntry last = _undoStack.Pop();

            List<Grade> list = _grades[last.Name];
            list.RemoveAt(list.Count - 1);

            if (list.Count == 0)
            {
                _grades.Remove(last.Name);
                _displayNames.Remove(last.Name);
            }

            string text = $"{last.Name} {last.Grade.Course} {last.Grade.Score}";
            AddToHistory($"undo {text}");
            return $"Undone: {text}";
        }

        private void AddToHistory(string entry)
        {
            _history.Enqueue(entry);
            if (_history.Count > HistoryLimit)
                _history.Dequeue();
        }

        private IEnumerable<string> StudentsSorted() =>
            _grades.Keys.OrderBy(n => _displayNames[n], StringComparer.OrdinalIgnoreCase);

        private static decimal Average(IEnumerable<int> scores) =>
            (decimal)scores.Sum() / scores.Count();

        public void PrintTable()
        {
            Console.WriteLine($"{"Name",-8}{"Count",6}{"Average",9}{"Max",5}  Last 3");

            foreach (string key in StudentsSorted())
            {
                int[] scores = _grades[key].Select(g => g.Score).ToArray();
                int[] last3 = scores.Length >= 3 ? scores[^3..] : scores;

                string name = _displayNames[key];
                int count = scores.Length;
                decimal avg = Average(scores);
                int max = scores.Max();

                Console.WriteLine($"{name,-8}{count,6}{avg,9:F2}{max,5}  {string.Join(" ", last3)}");
            }
        }

        public void PrintStudent(string name)
        {
            name = name.Trim();
            if (!_grades.TryGetValue(name, out List<Grade>? grades))
            {
                Console.WriteLine($"{name}: not found");
                return;
            }

            var byCourse = new SortedDictionary<string, decimal>(StringComparer.Ordinal);
            foreach (var group in grades.GroupBy(g => g.Course))
                byCourse[group.Key] = Average(group.Select(g => g.Score));

            string parts = string.Join(", ", byCourse.Select(kv => $"{kv.Key}={kv.Value:F2}"));
            Console.WriteLine($"{_displayNames[name]}: {parts}");
        }

        public void PrintTop3()
        {
            var top = _grades
                .Select(kv => new
                {
                    Name = _displayNames[kv.Key],
                    Avg = Average(kv.Value.Select(g => g.Score))
                })
                .OrderByDescending(s => s.Avg)
                .ThenBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
                .Take(3);

            Console.WriteLine("Top 3:");
            foreach (var s in top)
                Console.WriteLine($"  {s.Name,-8}{s.Avg:F2}");
        }

        public void PrintCourseReport()
        {
            var usedCourses = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var list in _grades.Values)
                foreach (var g in list)
                    usedCourses.Add(g.Course);

            foreach (string course in usedCourses)
            {
                var allScores = new List<int>();
                var perStudent = new List<(string Name, decimal Avg)>();

                foreach (var (key, list) in _grades)
                {
                    int[] scores = list.Where(g => g.Course == course).Select(g => g.Score).ToArray();
                    if (scores.Length == 0)
                        continue;

                    allScores.AddRange(scores);
                    perStudent.Add((_displayNames[key], Average(scores)));
                }

                var best = perStudent
                    .OrderByDescending(s => s.Avg)
                    .ThenBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
                    .First();

                Console.WriteLine(
                    $"{course,-7}students: {perStudent.Count}, average: {Average(allScores):F2}, best: {best.Name} ({best.Avg:F2})");
            }
        }

        public void PrintHistory()
        {
            Console.WriteLine("History:");
            foreach (string entry in _history)
                Console.WriteLine($"  {entry}");
        }

        public void PrintDistribution()
        {
            var dist = new SortedDictionary<char, int>
            {
                ['A'] = 0, ['B'] = 0, ['C'] = 0, ['D'] = 0, ['E'] = 0, ['F'] = 0
            };

            foreach (var list in _grades.Values)
                foreach (var g in list)
                    dist[Letter(g.Score)]++;

            Console.WriteLine("Distribution: " + string.Join(", ", dist.Select(kv => $"{kv.Key}={kv.Value}")));
        }

        private static char Letter(int score) => score switch
        {
            >= 91 => 'A',
            >= 81 => 'B',
            >= 71 => 'C',
            >= 61 => 'D',
            >= 51 => 'E',
            _ => 'F'
        };
    }

    public static class Demo
    {
        public static void Run()
        {
            var book = new Gradebook();

            string[] seed =
            [
                "Ana;.NET;95", "Ana;DB;88", "Ana;Math;100", "Gio;.NET;72", "Gio;DB;81",
                "luka;.NET;65", "Nino;Math;91", "Nino;.NET;67", "Nino;DB;79", "Ana;.NET;98"
            ];
            foreach (string line in seed)
            {
                string[] p = line.Split(';');
                book.AddScore(p[0], p[1], int.Parse(p[2]));
            }

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("1. Add score");
                Console.WriteLine("2. Table");
                Console.WriteLine("3. Search student");
                Console.WriteLine("4. Top 3");
                Console.WriteLine("5. Undo");
                Console.WriteLine("6. Course report");
                Console.WriteLine("7. History");
                Console.WriteLine("0. Exit");
                Console.Write("Choice: ");

                string? choice = Console.ReadLine()?.Trim();
                if (choice == null)
                    choice = "0";

                switch (choice)
                {
                    case "1":
                        Console.Write("Name: ");
                        string name = Console.ReadLine() ?? "";
                        Console.Write("Course: ");
                        string course = Console.ReadLine() ?? "";
                        Console.Write("Score: ");
                        string scoreText = Console.ReadLine() ?? "";

                        if (int.TryParse(scoreText, out int score))
                            Console.WriteLine(book.AddScore(name, course, score));
                        else
                            Console.WriteLine($"Invalid number: {scoreText}");
                        break;
                    case "2":
                        book.PrintTable();
                        break;
                    case "3":
                        Console.Write("Name: ");
                        book.PrintStudent(Console.ReadLine() ?? "");
                        break;
                    case "4":
                        book.PrintTop3();
                        break;
                    case "5":
                        Console.WriteLine(book.Undo());
                        break;
                    case "6":
                        book.PrintCourseReport();
                        break;
                    case "7":
                        book.PrintHistory();
                        break;
                    case "0":
                        book.PrintDistribution();
                        return;
                    default:
                        Console.WriteLine("Unknown option");
                        break;
                }
            }
        }
    }
}