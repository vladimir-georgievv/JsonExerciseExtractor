namespace JsonExerciseExtractor
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    class Program
    {
        public class Exercise
        {
            string tet;
            public string Name { get; set; }
            public List<Set> Sets { get; set; }
            public Exercise(string name)
            {
                Name = name;
                Sets = new();
            }
            public override string ToString()
            {
                return Name;
            }
        }
        public class Set
        {
            public int Reps { get; set; }
            public int Weight { get; set; }
            public Set(int reps, int weight)
            {
                Reps = reps;
                Weight = weight;
            }
            public override string ToString()
            {
                return $"{Reps}x{Weight}";
            }
        }
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            const string regexPattern = "(?<Exercise>[A-Z][a-zA-Z]+)(?:\\n(?<Set>\\d+x\\d+))+";

            List<Exercise> exercises = new List<Exercise>();

            string folderPath = @"C:\Users\Vladko\Downloads\Takeout\Keep";
            string[] jsonFiles = Directory.GetFiles(folderPath, "*.json");

            foreach (string file in jsonFiles)
            {
                string jsonData = File.ReadAllText(file, System.Text.Encoding.UTF8);
                JObject note = JObject.Parse(jsonData);

                string content = note["textContent"]?.ToString() ?? "No Content";

                MatchCollection matches = Regex.Matches(content, regexPattern);

                foreach (Match match in matches)
                {
                    Exercise exercise = new(match.Groups["Exercise"].Value);

                    foreach (Capture capture in match.Groups["Set"].Captures)
                    {
                        string[] parts = capture.Value.Split("x");
                        int reps = int.Parse(parts[0]);
                        int weight = int.Parse(parts[1]);
                        Set set = new(reps, weight);
                        exercise.Sets.Add(set);
                    }
                    exercises.Add(exercise);
                }
            }
            foreach (Exercise exercise in exercises)
            {
                Console.WriteLine(exercise.Name);
                foreach (Set set in exercise.Sets)
                {
                    Console.WriteLine($"{set.Reps}x{set.Weight}");
                }
                Console.WriteLine();
            }

            string jsonOuput = JsonConvert.SerializeObject(exercises, Formatting.Indented);
            string outputFilePath = @"C:\Users\Vladko\Documents\exercises_output.json";
            File.WriteAllText(outputFilePath, jsonOuput);
        }
    }

}
