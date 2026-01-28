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
using System.Text.Json;
using Crawler.JsonModel;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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

                var books = doc.DocumentNode.SelectNodes("//a[contains(@class, 'product-item-link')]/@href");

                foreach (var b in books)
                {
                    ClearAllTextBoxes(c);
                    var linkBook = b.Attributes[1].Value;

                    var bookHtml = client.GetHtml(linkBook);

                    HtmlAgilityPack.HtmlDocument persBookDoc = new HtmlAgilityPack.HtmlDocument();
                    persBookDoc.LoadHtml(bookHtml);

                    var titleRaw = persBookDoc.DocumentNode.SelectNodes("//h1[contains(@class, 'page-title')]");

                    var attributesRaw = persBookDoc.DocumentNode.SelectNodes("//table[contains(@class, 'data table')]/tbody/tr/td");

                    c.BeginInvoke((MethodInvoker)delegate ()
                    {
                        c.textBox_title.Text = titleRaw.FirstOrDefault().InnerText.Trim().ToString();

                        foreach (var atr in attributesRaw)
                        {
                            var atrName = WebUtility.HtmlDecode(atr.Attributes[1].Value);

                            switch (atrName)
                            {
                                case "Автор":
                                    var author = atr.InnerText.Trim();
                                    c.textBox_author.Text = author;
                                    break;
                                case "Издателство":
                                    var publisher = atr.InnerText.Trim();
                                    c.textBox_publisher.Text = publisher;
                                    break;
                                case "ISBN":
                                    var isbn = atr.InnerText.Trim();
                                    c.textBox_isbn.Text = isbn;
                                    break;
                                case "Корица":
                                    var cover = atr.InnerText.Trim();
                                    c.textBox_cover.Text = cover;
                                    break;
                                case "Страници":
                                    var pages = atr.InnerText.Trim();
                                    c.textBox_pages.Text = pages;
                                    break;
                                case "Формат":
                                    var format = atr.InnerText.Trim();
                                    c.textBox_format.Text = format;
                                    break;
                            }
                        }


                        var allScripts = persBookDoc.DocumentNode.SelectNodes("//script[@type='text/x-magento-init']");
                        ImageModel imageRaw = new ImageModel();

                        foreach (var script in allScripts)
                        {
                            try
                            {
                                var decodedJson = WebUtility.HtmlDecode(script.InnerText.Trim());

                                imageRaw = JsonConvert.DeserializeObject<ImageModel>(decodedJson);

                                if (imageRaw.Gallery != null)
                                {
                                    break;
                                }
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }

                        c.imageLink = imageRaw.Gallery.Mage.Data.First().ImageLink;

                        //c.imageLink = persBookDoc.DocumentNode.SelectSingleNode("//a[contains(@class, 'productGallery')]").Attributes[2].Value;

                        c.book_image.Load(c.imageLink);
                        c.book_image.SizeMode = PictureBoxSizeMode.StretchImage;

                        var descriptionZeroPart = persBookDoc.DocumentNode.SelectSingleNode("//div[contains(@itemprop, 'description')]");
                        var descriptionFirstPart = persBookDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'product attribute description')]");

                        var desc0 = String.Empty;
                        var desc1 = String.Empty;
                        //var desc2 = String.Empty;
                        if (descriptionZeroPart != null)
                        {
                            desc0 = descriptionZeroPart.InnerHtml.Trim();
                        }
                        if (descriptionFirstPart != null)
                        {
                            desc1 = descriptionFirstPart.InnerHtml.Trim();
                        }

                        var combDesc = WebUtility.HtmlDecode((desc0 + "<br><br>" + desc1).Trim());

                        var fullDesc = HtmlUtilities.ConvertToPlainText(combDesc.Replace("<li>", "<br>-"));

                        c.textBox_description.Text = fullDesc.Replace('#', '=').Replace('^', '+').Replace('—', '-').Replace('ѝ', 'й').Replace(".", ". ");

                        //Get price
                        var priceRaw = persBookDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'product-info-main')]/div[contains(@class, 'product-info-price')]//span[contains(@id, 'price')]");

                        var priceDecode = WebUtility.HtmlDecode(priceRaw.InnerText);
                        var price = priceDecode.Replace("\u00A0", " ").Split(" ")[0];
                        c.textBox_price.Text = price;
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

                var result = id + "^" + title + "^" + author + "^" + pages + "^" + format + "^" + publisher + "^" + cover + "^" + priceInNum + "^" + discount + "^" + id + ".jpg" + "^" + weight + "^" + isbn + "^" + buyFrom + "^" + "<img src=\"/2023anot/anot" + id + ".jpg\">" + "^" + description + "^" + "#";

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
