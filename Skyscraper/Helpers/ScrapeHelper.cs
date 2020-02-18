using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Skyscraper.Helpers
{
    public static class ScrapeHelper
    {
        public const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.106 Safari/537.36";

        public static HtmlNode Parse(String text)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(text);

            var htmlNode = htmlDocument.DocumentNode;
            return htmlNode;
        }

        public static async Task<HtmlNode> FetchParseAsync(String uri, string forceEncoding = null, Func<string, string> processBeforeParse = null)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(USER_AGENT);

            string html;

            if (forceEncoding != null)
            {
                var bytes = await httpClient.GetByteArrayAsync(uri);
                html = Encoding.GetEncoding(forceEncoding).GetString(bytes, 0, bytes.Length);
            }
            else
                html = await httpClient.GetStringAsync(uri);

            if (processBeforeParse != null)
                html = processBeforeParse.Invoke(html);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var htmlNode = htmlDocument.DocumentNode;
            return htmlNode;
        }

        // TODO: De-dup with above.
        public static async Task<HtmlNode> SendAsync(String uri, string forceEncoding = null, Func<string, string> processBeforeParse = null,
            HttpContent content = null, HttpMethod method = null)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(USER_AGENT);

            string html;

            var request = new HttpRequestMessage(method ?? HttpMethod.Get, uri);

            if (content != null)
                request.Content = content;

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new Exception(response.StatusCode.ToString());
                //throw new HttpStatusException(response.StatusCode);

            if (response?.Content?.Headers?.ContentType?.CharSet?.Contains("\"") ?? false)
                response.Content.Headers.ContentType.CharSet = response.Content.Headers.ContentType.CharSet.Replace("\"", ""); // Fix bug where charset is specified as "utf-8" not utf-8.

            if (forceEncoding != null)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                html = Encoding.GetEncoding(forceEncoding).GetString(bytes, 0, bytes.Length);
            }
            else
                html = await response.Content.ReadAsStringAsync();

            if (processBeforeParse != null)
                html = processBeforeParse.Invoke(html);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var htmlNode = htmlDocument.DocumentNode;
            return htmlNode;
        }
    }
}