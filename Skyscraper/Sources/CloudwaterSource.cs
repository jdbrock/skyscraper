using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Skyscraper.Helpers;
using Skyscraper.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Skyscraper.Sources
{
    public class CloudwaterSource
    {
        public async Task<IList<Beer>> Fetch()
        {
            var beers = new List<Beer>();

            Console.WriteLine("Scraping Cloudwater FFB...");
            Console.WriteLine();

            var client = new HttpClient();

            var breweriesPage = await ScrapeHelper.FetchParseAsync("https://www.friendsandfamily.beer/family");
            var breweryNodes = breweriesPage.QuerySelectorAll("h2 > a");
            var breweryCount = breweryNodes.Count();

            Console.WriteLine($"Found {breweryCount} breweries...");
            Console.WriteLine();

            foreach (var breweryNode in breweryNodes)
            {
                var href = breweryNode.Attributes["href"]?.Value;
                if (string.IsNullOrWhiteSpace(href))
                    continue;

                var breweryName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                    breweryNode.InnerText.Trim()?.ToLower()
                );

                Console.WriteLine(breweryName);
                Console.WriteLine("------------------------------");

                var beersPage = await ScrapeHelper.FetchParseAsync(href);
                var beerNodes = beersPage.QuerySelectorAll("ul > li > p");

                if (beerNodes.Count() == 0)
                {
                    Console.WriteLine("No beers found (yet)");
                }
                else
                {
                    foreach (var beerNode in beerNodes)
                    {
                        string beerName = null;
                        string description = null;

                        // Style: <strong>beer name</strong> description <strong>(v)</strong>
                        if (beerNode.ChildNodes[0].Name?.ToLower() == "strong")
                        {
                            beerName = beerNode.ChildNodes[0]?.InnerText?.Trim();
                            description = beerNode.ChildNodes[1]?.InnerText?.Trim();
                        }
                        // Style: beer name, description <strong>(v)</strong>
                        else
                        {
                            var parts = beerNode.ChildNodes[0]?.InnerText?.Trim()?.Split(',');
                            beerName = parts[0]?.Trim();
                            description = string.Join(',', parts.Skip(1));
                        }
                  
                        description = description?.TrimStart(' ', ',');

                        Console.WriteLine($"{beerName} ----- {description}");

                        beers.Add(new Beer
                        {
                            BreweryName = breweryName != null ? HtmlEntity.DeEntitize(breweryName) : null,
                            BeerName    = beerName    != null ? HtmlEntity.DeEntitize(beerName)    : null,
                            Description = description != null ? HtmlEntity.DeEntitize(description) : null
                        });
                    }
                }

                Console.WriteLine();
                await Task.Delay(1000);
            }

            Console.WriteLine($"Found {beers.Count} beers.");

            return beers;
        }
    }
}
