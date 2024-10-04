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
        if (Directory.GetFiles(savePath).Length == 0)
        {
            foreach (string file in Directory.GetFiles(path))
            {
                Dictionary<string, int> occurrences = new Dictionary<string, int>();
                
                var text = File.ReadAllText(file);
                text = text.ToLower();
                var words = text.Split(new[] {' ', '\n'});
                foreach (string word in words)
                {
                    Regex rgx = new Regex("[^a-zA-Z0-9]");
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

        foreach (string file in Directory.GetFiles(savePath))
        {
            if (!file.Contains(".txt"))
            {
                continue;
            }
            var text2 = File.ReadAllLines(file.Replace("More Data", "Data"));
            Dictionary<string, int> d = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(file));
            if (File.ReadAllText(file).Contains(query))
            {
                var filename = Path.GetFileName(file);

                results.Add(new Result {Title = text2[2].Substring(7), Season = text2[0].Substring(8), Episode = text2[1].Substring(9), Filename = filename});
            }
        }

        sw.Stop();

        return new SearchResults
        {
            Query = query,
            Results = results,
            Duration = sw.Elapsed
        };
    }
}
