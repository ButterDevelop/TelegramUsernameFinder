using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace TelegramUsernameFinder.Helpers
{
    public class MakeRequest
    {
        private readonly HttpClientHandler handler;
        private readonly HttpClient        client;

        public MakeRequest(string proxy = "")
        {
            handler = new()
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                UseCookies                                = false, // Не использовать автоматическое управление куками
                UseProxy                                  = !string.IsNullOrEmpty(proxy), // Настроим прокси при необходимости
                AutomaticDecompression                    = DecompressionMethods.None // Отменить автоматическую декомпрессию
            };

            if (!string.IsNullOrEmpty(proxy))
            {
                throw new Exception("Proxy problem is here.");
            }

            client = new(handler)
            {
                BaseAddress           = new Uri("https://fragment.com"),
                DefaultRequestVersion = new Version(2, 0)
            };

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/javascript"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.01));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("zstd"));
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("ru"));
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-US", 0.9));
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en", 0.8));
            client.DefaultRequestHeaders.Add("Cookie", "stel_ssid=4a861d9ef84868e6f1_13979275404572446652; stel_dt=-180; __cf_bm=kx958_cJ2xlveIAWUAxEQpSFH3LbB.jcNgzzmp5kmao-1720648449-1.0.1.1-bzgR.4vGa88F5NBqI278KVyGyexJVIMZg2dP2nEfixKZhwzHRyO8EZE1nLe8BbkNpnuxZ8wdCXC85u0CLbAh3w");
            client.DefaultRequestHeaders.Add("Origin", "https://fragment.com");
            client.DefaultRequestHeaders.Add("Priority", "u=1, i");
            client.DefaultRequestHeaders.Add("Sec-CH-UA", "\"Not/A)Brand\";v=\"8\", \"Chromium\";v=\"126\", \"Google Chrome\";v=\"126\"");
            client.DefaultRequestHeaders.Add("Sec-CH-UA-Arch", "\"x86\"");
            client.DefaultRequestHeaders.Add("Sec-CH-UA-Bitness", "\"64\"");
            client.DefaultRequestHeaders.Add("Sec-CH-UA-Full-Version", "\"126.0.6478.127\"");
            client.DefaultRequestHeaders.Add("Sec-CH-UA-Full-Version-List", "\"Not/A)Brand\";v=\"8.0.0.0\", \"Chromium\";v=\"126.0.6478.127\", \"Google Chrome\";v=\"126.0.6478.127\"");
            client.DefaultRequestHeaders.Add("Sec-CH-UA-Mobile", "?0");
            client.DefaultRequestHeaders.Add("Sec-CH-UA-Model", "\"\"");
            client.DefaultRequestHeaders.Add("Sec-CH-UA-Platform", "\"Windows\"");
            client.DefaultRequestHeaders.Add("Sec-CH-UA-Platform-Version", "\"15.0.0\"");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        }

        public async Task<string?> Work(string username)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"/api?hash=4841aaccbef0ccf5dc")
                {
                    Version = new Version(2, 0),
                    Content = new StringContent($"type=usernames&query={username}&filter=&sort=price_desc&method=searchAuctions", Encoding.UTF8, "application/x-www-form-urlencoded")
                };

                request.Headers.Add("Referer", $"https://fragment.com/?query={username}&sort=price_asc");

                var response = await client.SendAsync(request);
                var responseBytes = await response.Content.ReadAsByteArrayAsync();

                string responseString;
                if (response.Content.Headers.ContentEncoding.Contains("gzip"))
                {
                    responseString = Decompressors.DecompressGzip(responseBytes);
                }
                else if (response.Content.Headers.ContentEncoding.Contains("deflate"))
                {
                    responseString = Decompressors.DecompressDeflate(responseBytes);
                }
                else if (response.Content.Headers.ContentEncoding.Contains("br"))
                {
                    responseString = Decompressors.DecompressBrotli(responseBytes);
                }
                else
                {
                    responseString = Encoding.UTF8.GetString(responseBytes);
                }

                try
                {
                    dynamic json = JObject.Parse(responseString.Replace("'", "\\'").Replace("\"", "'"));
                    return json.html;
                }
                catch
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
