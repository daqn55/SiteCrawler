using ReadSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace CielaCrawler.Services
{
    class CielaService
    {
        public void GetResult(CrawlerDesine c)
        {

            var pageCount = int.Parse(c.textBox_linkPage.Text);

            for (int i = 0; i < 150; i++)
            {
                var url = c.textBox_link.Text;

                if (i > 0)
                {
                    c.textBox_linkPage.Text = i.ToString();
                    pageCount = i;
                }
                if (pageCount > 0)
                {
                    url += "?p=" + pageCount;
                    i = pageCount;
                }

                var client = new Client();
                var rawHtml = client.GetHtml(url);

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(rawHtml);

                var books = doc.DocumentNode.SelectNodes("//a[contains(@class, 'productBoxTitle')]/@href");

                foreach (var b in books)
                {
                    ClearAllTextBoxes(c);
                    var linkBook = b.Attributes[0].Value;

                    var bookHtml = client.GetHtml(linkBook);

                    HtmlAgilityPack.HtmlDocument persBookDoc = new HtmlAgilityPack.HtmlDocument();
                    persBookDoc.LoadHtml(bookHtml);

                    var titleRaw = persBookDoc.DocumentNode.SelectNodes("//div[contains(@class, 'productDetailedInfo')]/h1");

                    var attributesRaw = persBookDoc.DocumentNode.SelectNodes("//div[contains(@class, 'productDetailedInformation')]").FirstOrDefault().InnerText.Trim();

                    var attributes = attributesRaw.Split("\n", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => !String.IsNullOrWhiteSpace(x) || !String.IsNullOrEmpty(x)).ToList();

                    c.BeginInvoke((MethodInvoker)delegate ()
                    {
                        c.textBox_title.Text = titleRaw.FirstOrDefault().InnerText.Trim().ToString();

                        for (int a = 1; a <= attributes.Count; a++)
                        {
                            if (a % 2 > 0)
                            {
                                var attribute = attributes[a - 1];
                                switch (attribute)
                                {
                                    case "Автор":
                                        var author = attributes[a];
                                        c.textBox_author.Text = author;
                                        break;
                                    case "Издателство":
                                        var publisher = attributes[a];
                                        c.textBox_publisher.Text = publisher;
                                        break;
                                    case "ISBN":
                                        var isbn = attributes[a];
                                        c.textBox_isbn.Text = isbn;
                                        break;
                                    case "Корица":
                                        var cover = attributes[a];
                                        c.textBox_cover.Text = cover;
                                        break;
                                    case "Страници":
                                        var pages = attributes[a];
                                        c.textBox_pages.Text = pages;
                                        break;
                                    case "Формат":
                                        var format = attributes[a];
                                        c.textBox_format.Text = format;
                                        break;
                                }
                            }

                        }

                        c.imageLink = persBookDoc.DocumentNode.SelectSingleNode("//a[contains(@class, 'productGallery')]").Attributes[2].Value;

                        c.book_image.Load(c.imageLink);
                        c.book_image.SizeMode = PictureBoxSizeMode.StretchImage;

                        var descriptionZeroPart = persBookDoc.DocumentNode.SelectSingleNode("//section[contains(@class, 'shortDescription')]/h2");
                        var descriptionFirstPart = persBookDoc.DocumentNode.SelectSingleNode("//section[contains(@class, 'shortDescription')]/p");
                        var descriptionSecondPart = persBookDoc.DocumentNode.SelectSingleNode("//div[contains(@id, 'tab-1')]//div");

                        var desc0 = String.Empty;
                        var desc1 = String.Empty;
                        var desc2 = String.Empty;
                        if (descriptionZeroPart != null)
                        {
                            desc0 = descriptionZeroPart.InnerHtml.Trim();
                        }
                        if (descriptionFirstPart != null)
                        {
                            desc1 = descriptionFirstPart.InnerHtml.Trim();
                        }
                        if (descriptionSecondPart != null)
                        {
                            desc2 = descriptionSecondPart.InnerHtml.Trim();
                        }

                        var combDesc = WebUtility.HtmlDecode((desc0 + "<br><br>" + desc1 + "<br><br>" +  desc2).Trim());

                        var fullDesc = HtmlUtilities.ConvertToPlainText(combDesc.Replace("<li>", "<br>-"));

                        c.textBox_description.Text = fullDesc.Replace('#', '=').Replace('^', '+').Replace('—', '-').Replace('ѝ', 'й').Replace(".", ". ");

                        //Get price
                        var priceRaw = persBookDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'addToCartSection')]//span[contains(@class, 'regular-price')]/span");
                        if (priceRaw == null)
                        {
                            var priceOldRaw = persBookDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'addToCartSection')]//div[contains(@class, 'price-box')]//span[contains(@id, 'old')]");
                            var priceOld = priceOldRaw.InnerText.Trim().Replace("\u00A0", " ").Split(" ")[0];
                            c.textBox_price.Text = priceOld;
                        }
                        else
                        {
                            var priceDecode = WebUtility.HtmlDecode(priceRaw.InnerText);
                            var price = priceDecode.Replace("\u00A0", " ").Split(" ")[0];
                            c.textBox_price.Text = price;
                        }


                    });

                    c.signal.Wait();
                }

                pageCount++;
                c.textBox_linkPage.Text = pageCount.ToString();
            }
        }

        private void ClearAllTextBoxes(CrawlerDesine c)
        {
            c.BeginInvoke((MethodInvoker)delegate ()
            {
                c.book_image.Image = null;
                c.textBox_title.Text = "";
                c.textBox_author.Text = "";
                c.textBox_publisher.Text = "";
                c.textBox_isbn.Text = "";
                c.textBox_cover.Text = "";
                c.textBox_pages.Text = "";
                c.textBox_format.Text = "";
                c.textBox_description.Text = "";
                c.textBox_dicount.Text = "";
                c.textBox_price.Text = "";
                c.textBox_weight.Text = "";
                c.textBox_buyFrom.Text = "";
            });
        }

        public void SaveBook(CrawlerDesine c)
        {
            var id = c.textBox_id.Text;
            var title = c.textBox_title.Text;
            var author = c.textBox_author.Text;
            var publisher = c.textBox_publisher.Text;
            var isbn = c.textBox_isbn.Text;
            var cover = c.textBox_cover.Text;
            var pages = c.textBox_pages.Text;
            var format = c.textBox_format.Text;
            var description = c.textBox_description.Text.Replace('#', '=').Replace('^', '+').Replace('—', '-').Replace('ѝ', 'й').Replace(".", ". "); ;
            var price = c.textBox_price.Text;
            var discount = c.textBox_dicount.Text;
            var weight = c.textBox_weight.Text;
            var buyFrom = c.textBox_buyFrom.Text;

            try
            {
                //Create Image from text description
                var textToImage = new TextToImage(description);

                var pathStandartFolder = "StandarPictures";
                var pathMobileFolder = "MobilePictures";
                var currentDiretory = AppDomain.CurrentDomain.BaseDirectory;

                if (!string.IsNullOrEmpty(id))
                {
                    DirectoryInfo standartDirectory = Directory.CreateDirectory(currentDiretory + pathStandartFolder);
                    DirectoryInfo mobileDirectory = Directory.CreateDirectory(currentDiretory + pathMobileFolder);

                    if (textToImage.GeneratedImage != null)
                    {
                        textToImage.GeneratedImage.Save(currentDiretory + pathStandartFolder + "\\" + "anot" + id + ".jpg", ImageFormat.Jpeg);
                        textToImage.GeneratedImageMobile.Save(currentDiretory + pathMobileFolder + "\\" + "anot" + id + ".jpg", ImageFormat.Jpeg);
                    }
                }

                //Download Image of book
                var client = new Client();
                client.DownloadFile(c.imageLink, $@".\images\{id}.jpg");

                //Create and save all data for book to txt file
                var priceInNum = price.Replace(',', '.');
                //var discountInNum = int.Parse(discount);

                var result = id + "^" + title + "^" + author + "^" + pages + "^" + format + "^" + publisher + "^" + cover + "^" + priceInNum + "^" + discount + "^" + id + ".jpg" + "^" + weight + "^" + isbn + "^" + buyFrom + "^" + "<img src=\"/anot/anot" + id + ".jpg\">" + "^" + description + "^" + "#";

                Directory.CreateDirectory(".\\Data");

                if (!File.Exists(".\\Data\\bookData.txt"))
                {
                    File.WriteAllText(".\\Data\\bookData.txt", result);
                }
                else
                {
                    File.AppendAllText(".\\Data\\bookData.txt", result);
                }

                c.label_message.ForeColor = Color.ForestGreen;
                c.label_message.Text = $"Книгата \"{title}\" с id={id}, беше успешно записана.";

                //Update book id after saving 
                var newId = int.Parse(id);
                newId++;
                c.textBox_id.Text = newId.ToString();
            }
            catch (Exception m)
            {
                var msg = m.Message;
                c.label_message.ForeColor = Color.Red;
                c.label_message.Text = "Нещо се счупи :O, обърни се към Даян :Д...";

                Directory.CreateDirectory(".\\Log");

                if (!File.Exists(".\\Log\\loger.txt"))
                {
                    File.WriteAllText(".\\Log\\loger.txt", msg);
                }
                else
                {
                    File.AppendAllText(".\\Log\\loger.txt", msg);
                }
            }
        }
    }
}
