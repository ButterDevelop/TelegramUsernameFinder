using TelegramUsernameFinder.Models;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace TelegramUsernameFinder.Helpers
{
    public class AnalyzeResponse
    {
        public static List<UsernameModel>? ParseHtml(string bodyContent)
        {
            try
            {
                // Оборачиваем содержимое body в полноценный HTML-документ
                string html = "<html><head><title>Document</title></head><body>" + bodyContent + "</body></html>";

                var usernameModels = new List<UsernameModel>();

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                // Проверяем, что HTML загружен
                if (htmlDoc.DocumentNode == null)
                {
                    throw new Exception("HTML document not loaded correctly.");
                }

                // Пробуем найти таблицу
                var table = htmlDoc.DocumentNode.SelectSingleNode("/html/body/div[2]/table");
                if (table == null)
                {
                    if (html.Contains("Auctions not found."))
                    {
                        return [];
                    }
                    else
                    {
                        throw new Exception("Table not found.");
                    }
                }

                // Пробуем найти строки таблицы
                var rows = table.SelectNodes(".//tbody/tr");
                if (rows == null)
                {
                    throw new Exception("Table rows not found.");
                }

                foreach (var row in rows)
                {
                    var usernameModel = new UsernameModel
                    {
                        IsAvailable       = false,
                        NeedTelegramCheck = false
                    };

                    var usernameNode = row.SelectSingleNode(".//td[1]//div[contains(@class, 'table-cell-value')]");
                    if (usernameNode != null)
                    {
                        string usernameFullString = usernameNode.InnerText.Trim();
                        var splitted = usernameFullString.Split(new char[2] {'\n', ' '}, StringSplitOptions.RemoveEmptyEntries);
                        if (splitted.Length == 0) continue;
                        usernameModel.Id = splitted[0][1..];
                    }

                    var valueNode = row.SelectSingleNode(".//td[2]//div[contains(@class, 'table-cell-value')]");
                    if (valueNode != null)
                    {
                        if (int.TryParse(valueNode.InnerText.Trim(), out int value)) usernameModel.Value = value;
                        //usernameModel.SalePrice  = row.SelectSingleNode(".//td[2]//div[@class='table-cell-desc']")?.InnerText.Trim();
                        //usernameModel.MinimumBid = row.SelectSingleNode(".//td[2]//div[@class='table-cell-desc']")?.InnerText.Trim();
                    }

                    var statusNode = row.SelectSingleNode(".//td[3]//div[contains(@class, 'table-cell-value')]");
                    if (statusNode != null)
                    {
                        usernameModel.Status = statusNode.InnerText.Trim();
                    }

                    /*var auctionTimeLeftNode = row.SelectSingleNode(".//td[3]//div[contains(@class, 'tm-timer')]//time");
                    if (auctionTimeLeftNode != null)
                    {
                        usernameModel.AuctionTimeLeft = auctionTimeLeftNode.InnerText.Trim();
                    }*/

                    usernameModels.Add(usernameModel);
                }

                return usernameModels;
            }
            catch
            {
                return null;
            }
        }
    }
}
