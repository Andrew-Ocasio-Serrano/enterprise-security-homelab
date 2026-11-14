using System.Text.Json;

namespace WazuhAlertSummarizer
{
    public class Alert
    {
        public string timestamp { get; set; }
        public Rule rule { get; set; }
        public Agent agent { get; set; }
    }
    public class Rule
    {
        public int level { get; set; }
        public string description { get; set; }
        public string id { get; set; }
        public Mitre mitre { get; set; }
    }
    public class Mitre
    {
        public string[] id { get; set; }
        public string[] tactic { get; set; }
        public string[] technique { get; set; }
    }
    public class Agent
    {
        public string id { get; set; }
        public string name { get; set; }
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            string filepath = @"C:\Users\andre\Documents\Cybersecurity Portfolio\enterprise-security-homelab\project-2-detection-engineering\sample-alerts.json";
            string[] jsonString = File.ReadAllLines(filepath);

            var alerts = new List<Alert>();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            foreach (var json in jsonString)
            {
                if (string.IsNullOrEmpty(json)) continue;
                try
                {
                    var alert = JsonSerializer.Deserialize<Alert>(json, options);
                    alerts.Add(alert);
                }
                catch (Exception ex)
                {
                    Console.Write("Skipped a bad line\n");
                }
            }
            Console.WriteLine($"Total alerts: {alerts.Count}");
            var earliest = alerts.Min(a => a.timestamp);
            var latest = alerts.Max(a => a.timestamp);
            Console.WriteLine($"Time range: {earliest} → {latest}\n");
            Console.WriteLine("\n=== By Description ===");
            var byDescription = alerts
                .GroupBy(a => a.rule.description)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count);
            foreach (var item in byDescription)
            {
                Console.WriteLine($"{item.Count,3} {item.Name}");
            }
            Console.WriteLine("\n=== By Severity ===");
            var byLevel = alerts
                .GroupBy(a => a.rule.level)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count);
            foreach(var item in byLevel)
            {
                Console.WriteLine($"{item.Count,3}  level {item.Name}");
            }
            Console.WriteLine("\n=== By Agent ===");
            var byAgent = alerts
                .GroupBy(a => a.agent.name)
                .Select(g => new {Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count);
            foreach (var item in byAgent)
            {
                Console.WriteLine($"{item.Count,3} {item.Name}");
            }
            Console.WriteLine("\n=== By Technique ===");
            var byMitre = alerts
                .Where(a => a.rule.mitre != null)
                .SelectMany(a => a.rule.mitre.technique)
                .GroupBy(t => t)
                .Select(g => new {Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count);
            foreach (var item in byMitre)
            {
                Console.WriteLine($"{item.Count, 3} {item.Name}");
            }
        }
    }
}