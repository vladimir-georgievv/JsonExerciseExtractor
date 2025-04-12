namespace JsonExerciseExtractor
{
    using System;
    using System.IO;
    using System.Reflection.Metadata;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using HtmlAgilityPack;

    class Program
    {
        public class Exercise
        {
            public string Name { get; set; }
            public List<Set> Sets { get; set; }
            public DateTime? DateTime { get; set; }
            public Exercise(string name, DateTime? dateTime)
            {
                Name = name;
                Sets = new();
                DateTime = dateTime;
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

            //Get the path to the "Keep" folder in the "Downloads" directory
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "Takeout", "Keep");
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("The specified folder does not exist.");
                return;
            }

            string[] jsonFiles = Directory.GetFiles(folderPath, "*.json");

            foreach (string file in jsonFiles)
            {
                string htmlPath = file.Replace(".json", ".html");
                DateTime? creationDate = GetCreatedDate(htmlPath);

                string jsonData = File.ReadAllText(file, System.Text.Encoding.UTF8);
                JObject note = JObject.Parse(jsonData);

                string content = note["textContent"]?.ToString() ?? "No Content";

                MatchCollection matches = Regex.Matches(content, regexPattern);

                foreach (Match match in matches)
                {
                    Exercise exercise = new(match.Groups["Exercise"].Value, creationDate);

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
            string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string outputFilePath = Path.Combine(userDocumentsPath, "exercises_output.json");
            File.WriteAllText(outputFilePath, jsonOuput);
        }

        public static DateTime? GetCreatedDate(string filePath)
        {
            if(!File.Exists(filePath))
            {
               throw new FileNotFoundException("The specified file does not exist.", filePath);
            }

            string heml = File.ReadAllText(filePath);

            HtmlDocument document = new HtmlDocument();

            document.LoadHtml(heml);

            HtmlNode titleNode = document.DocumentNode.SelectSingleNode("//title");

            if(titleNode != null)
            {
                string title = titleNode.InnerText;

                string datePattern = @"(\d{1,2}\.\d{1,2}\.\d{4})";
                Match match = Regex.Match(title, datePattern);

                if (match.Success)
                {
                    string dateString = match.Value;

                    DateTime parsedDate;
                    if (DateTime.TryParseExact(dateString, "d.M.yyyy", null, System.Globalization.DateTimeStyles.None, out parsedDate))
                    {
                        return parsedDate;
                    }
                    else
                    {
                        Console.WriteLine("Failed to parse date.");
                    }
                }
            }
            return null;
        }
    }

}
