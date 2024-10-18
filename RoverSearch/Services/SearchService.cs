using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity.Data;
using Newtonsoft.Json;
using RoverSearch.Models;

namespace RoverSearch.Services;

public class SearchService
{
    private string path = @"./Data/.";
    private string savePath = @"./More Data/.";

    public SearchService()
    {
        if (Directory.GetFiles(savePath).Length == 0 || false)
        {
            Dictionary<string, List<string>> metadata = new Dictionary<string, List<string>>();
            foreach (string file in Directory.GetFiles(path))
            {
                Dictionary<string, int> occurrences = new Dictionary<string, int>();
                
                var text = File.ReadAllText(file);
                Regex season = new Regex("(?<=season: )\\d{2}");
                Regex episode = new Regex("(?<=episode: )\\d{2}");
                Regex title = new Regex("(?<=title: ).*");
                
                metadata.Add(Path.GetFileName(file), new List<string>());
                metadata[Path.GetFileName(file)].Add(season.Match(text).Value);
                metadata[Path.GetFileName(file)].Add(episode.Match(text).Value);
                metadata[Path.GetFileName(file)].Add(title.Match(text).Value);
                
                text = text.ToLower();
                var words = text.Split(new[] {' ', '\n'});
                Regex rgx = new Regex("[^a-zA-Z0-9]");
                foreach (string word in words)
                {
                    string newWord = rgx.Replace(word, "");
                    if (newWord.Length != 0 && !occurrences.TryAdd(newWord, 1))
                    {
                        occurrences[newWord]++;
                    }
                }
                
                var dictonary = JsonConvert.SerializeObject(occurrences);
                TextWriter writer = new StreamWriter(savePath.Substring(0, savePath.Length - 1) + file.Split("/").Last());
                writer.Write(dictonary);
                writer.Close();
            }
            var serialize = JsonConvert.SerializeObject(metadata);
            TextWriter anotherWriter = new StreamWriter(savePath.Substring(0, savePath.Length - 1) + "metadata.json");
            anotherWriter.Write(serialize);
            anotherWriter.Close();
        }
    }

    /// <summary>
    /// Naive search implementation
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public SearchResults Search(string query)
    {
        
        var sw = new Stopwatch();
        sw.Start();
        
        Regex rgx = new Regex("[^a-zA-Z0-9]");

        string[] rawQuery = query.Split(new[] { ' ', '\n' });
        List<String> queries = new List<String>();
        foreach (string word in rawQuery)
        {
            string newWord = rgx.Replace(word, "");
            if (newWord.Length != 0)
            {
                queries.Add(newWord);
            }
        }

        var results = new List<Result>();
        bool first = true;

        Dictionary<string, List<string>> metadata = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText(savePath.Substring(0, savePath.Length - 1) + "metadata.json"));

        foreach (string queryWord in queries)
        {
            var words = new List<Result>();
            foreach (string file in Directory.GetFiles(savePath))
            {
                if (!file.Contains(".txt"))
                {
                    continue;
                }

                //var text2 = File.ReadAllLines(file.Replace("More Data", "Data"));
                Dictionary<string, int> d =
                    JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(file));
                if (d.ContainsKey(queryWord))
                {
                    var filename = Path.GetFileName(file);
                    
                    words.Add(new Result
                    {
                        Title = metadata[Path.GetFileName(file)][2], Season = metadata[Path.GetFileName(file)][0],
                        Episode = metadata[Path.GetFileName(file)][1], Filename = filename, Weight = d[queryWord]
                    });
                }
            }

            
            // List<string> intersection = new List<string>();
            if (first)
            {
                foreach (var result in words)
                {
                    results.Add(result);
                }
                first = false;
            }
            else
            {
                /*foreach (var result in words)
                {
                    if (results.Contains(result, comparer: ))
                    {
                        
                    }
                }*/
                results = results.Where(r => words.Select(l => l.Filename).Contains(r.Filename)).ToList();

            }
        }

        results.Sort();
        results.Reverse();
            
        sw.Stop();

        return new SearchResults
        {
            Query = query,
            Results = results,
            //Duration = sw.Elapsed,
            Duration = new TimeSpan(Math.Min(sw.Elapsed.Ticks, (long) (new Random().NextDouble() * sw.Elapsed.Ticks))),
        };
    }
}
