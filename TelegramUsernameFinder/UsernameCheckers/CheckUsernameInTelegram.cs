using TelegramUsernameFinder.Helpers;
using HtmlAgilityPack;
using System.Net.Http.Headers;
using System.Text;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace TelegramUsernameFinder.UsernameCheckers
{
    public class CheckUsernameInTelegram
    {
        public static async Task<bool> Check(string username)
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                    using (var client = new HttpClient(handler))
                    {
                        client.DefaultRequestVersion = new Version(2, 0);
                        client.BaseAddress = new Uri("https://t.me");

                        var request = new HttpRequestMessage(HttpMethod.Get, $"/{username}")
                        {
                            Version = new Version(1, 1)
                        };
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9));
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("image/avif"));
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("image/apng"));
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/signed-exchange", 0.7));
                        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
                        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("zstd"));
                        request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("ru"));
                        request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-US", 0.9));
                        request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("en", 0.8));
                        request.Headers.Add("Cookie", "stel_on=1; stel_dt=-180; stel_ssid=e639b4b89e14541a2e_73161633941200988");
                        request.Headers.Add("sec-ch-ua", "\"Not/A)Brand\";v=\"8\", \"Chromium\";v=\"126\", \"Google Chrome\";v=\"126\"");
                        request.Headers.Add("sec-ch-ua-mobile", "?0");
                        request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
                        request.Headers.Add("Upgrade-Insecure-Requests", "1");
                        request.Headers.Add("Sec-Fetch-Site", "none");
                        request.Headers.Add("Sec-Fetch-Mode", "navigate");
                        request.Headers.Add("Sec-Fetch-User", "?1");
                        request.Headers.Add("Sec-Fetch-Dest", "document");
                        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");

                        var response = await client.SendAsync(request);


                        byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();
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

                        var htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(responseString);

                        return htmlDoc.DocumentNode.SelectSingleNode("/html/body/div[2]/div[2]/div/div[1]/i") != null;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
