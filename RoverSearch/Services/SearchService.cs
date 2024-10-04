using System.Diagnostics;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RoverSearch.Models;

namespace RoverSearch.Services;

public class SearchService
{
    private string path = @"./Data/.";
    private string savePath = @"./More Data/.";

    public SearchService()
    {
        if (Directory.GetFiles(savePath).Length == 0 || true)
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
                    if (!occurrences.TryAdd(newWord, 1))
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

        var results = new List<Result>();

        Dictionary<string, List<string>> metadata = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(File.ReadAllText(savePath.Substring(0, savePath.Length - 1) + "metadata.json"));

        
        foreach (string file in Directory.GetFiles(savePath))
        {
            if (!file.Contains(".txt"))
            {
                continue;
            }
            //var text2 = File.ReadAllLines(file.Replace("More Data", "Data"));
            Dictionary<string, int> d = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(file));
            if (d.ContainsKey(query))
            {
                var filename = Path.GetFileName(file);

                results.Add(new Result {Title = metadata[Path.GetFileName(file)][2], Season = metadata[Path.GetFileName(file)][0], Episode = metadata[Path.GetFileName(file)][1], Filename = filename, Weight = d[query]});
            }
        }

        results.Sort();
        results.Reverse();
            
        sw.Stop();

        return new SearchResults
        {
            Query = query,
            Results = results,
            Duration = sw.Elapsed
        };
    }
}
