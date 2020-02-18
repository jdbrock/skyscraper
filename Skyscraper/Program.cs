using Skyscraper.Models;
using Skyscraper.Sources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Skyscraper
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var source = new CloudwaterSource();
            var beers = await source.Fetch();

            var now = DateTime.Now;

            await ExportToCsv(beers, $"CloudwaterFFB_{now:yyyy-MM-dd_HHmmss}.csv");

            Console.WriteLine();
            Console.WriteLine("Press return to exit.");
            Console.ReadLine();
        }

        public static async Task ExportToCsv(IList<Beer> beers, string fileName)
        {
            using (var file = File.OpenWrite(fileName))
            using (var writer = new StreamWriter(file))
            {
                await writer.WriteLineAsync("Brewery,Beer,Description");

                foreach (var beer in beers)
                {
                    await writer.WriteLineAsync($"\"{beer.BreweryName}\",\"{beer.BeerName}\",\"{beer.Description}\"");
                }

                await writer.FlushAsync();
                writer.Close();
            }
        }
    }
}
