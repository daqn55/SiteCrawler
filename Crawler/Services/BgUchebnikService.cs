using CielaCrawler.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace CielaCrawler.Services
{
    class BgUchebnikService
    {
        internal void GetResult(CrawlerDesine c)
        {
            var pageCount = int.Parse(c.tab2_textBox_linkPage.Text);

            for (int i = 1; i <= 200; i++)
            {
                var url = c.tab2_textBox_link.Text;

                if (i > 1)
                {
                    c.tab2_textBox_linkPage.Text = i.ToString();
                    pageCount = i;
                }
                if (pageCount > 1)
                {
                    url += "?p=" + pageCount;
                    i = pageCount;
                }

                var client = new Client();
                var rawHtml = client.GetHtml(url);

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(rawHtml);

                var books = doc.DocumentNode.SelectNodes("//a[contains(@class, 'product-box')]/@href");

                foreach (var b in books)
                {
                    ClearAllTextBoxes(c);
                    var linkBook = b.Attributes[1].Value;

                    var bookHtml = client.GetHtml(linkBook);

                    HtmlAgilityPack.HtmlDocument persBookDoc = new HtmlAgilityPack.HtmlDocument();
                    persBookDoc.LoadHtml(bookHtml);

                    var titleRaw = persBookDoc.DocumentNode.SelectSingleNode("//h1[contains(@itemprop, 'name')]");

                    var attributesRaw = persBookDoc.DocumentNode.SelectNodes("//ul[contains(@class, 'attributes')]/li").ToList();

                    c.BeginInvoke((MethodInvoker)delegate ()
                    {
                        c.tab2_textBox_title.Text = titleRaw.InnerText.Trim();

                        foreach (var attrRaw in attributesRaw)
                        {
                            var attr = attrRaw.InnerText.Split("\n", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => !String.IsNullOrWhiteSpace(x) || !String.IsNullOrEmpty(x)).ToList();

                            switch (attr[0])
                            {
                                case "Тип:":
                                    c.tab2_textBox_type.Text = attr[1];
                                    break;
                                case "Издателство:":
                                    c.tab2_textBox_publisher.Text = attr[1];
                                    break;
                                case "Автори:":
                                    c.tab2_textbox_author.Text = attr[1];
                                    break;
                                case "ISBN номер:":
                                    c.tab2_textBox_isbn.Text = attr[1];
                                    break;
                                case "Страници:":
                                    c.tab2_textBox_pages.Text = attr[1];
                                    break;
                                case "Година:":
                                    c.tab2_textBox_year.Text = attr[1];
                                    break;
                            }
                        }

                        c.imageLink = persBookDoc.DocumentNode.SelectSingleNode("//a[contains(@class, 'main-image')]/img").Attributes[0].Value;

                        c.tab2_image.Load(c.imageLink);
                        c.tab2_image.SizeMode = PictureBoxSizeMode.StretchImage;

                        var priceRaw = persBookDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'product-options')]/div/span");
                        var price = priceRaw.InnerText.Trim().Replace(',', '.').Split("\u00A0")[0];
                        c.tab2_textBox_price.Text = price;

                        var descriptionRaw = persBookDoc.DocumentNode.SelectSingleNode("//div[contains(@itemprop, 'description')]");
                        var description = WebUtility.HtmlDecode(descriptionRaw.InnerText.Trim());
                        c.tab2_textBox_desc.Text = description;

                        var a = 1;
                    });

                    c.signal.Wait();
                }

                pageCount++;
                c.textBox_linkPage.Text = pageCount.ToString();
            }
        }

        public void SaveBook(CrawlerDesine c)
        {
            var id = c.tab2_textBox_id.Text;
            var title = c.tab2_textBox_title.Text;
            var author = c.tab2_textbox_author.Text;
            var publisher = c.tab2_textBox_publisher.Text;
            var isbn = c.tab2_textBox_isbn.Text;
            var coverRaw = c.tab2_textBox_cover.Text;
            var pages = c.tab2_textBox_pages.Text;
            var format = c.tab2_textBox_format.Text;
            var description = c.tab2_textBox_desc.Text;
            var price = c.tab2_textBox_price.Text;
            var discount = c.tab2_textBox_discount.Text;
            var weight = c.tab2_textBox_weight.Text;
            var buyFrom = c.tab2_textBox_buyFrom.Text;
            var category = c.tab2_comboBox_category.Text;
            var year = c.tab2_textBox_year.Text;

            var cover = "Мека";
            if (coverRaw.ToLower() == "твърда")
            {
                cover = "Твърда";
            }

            var cat = "58";
            var catBgbook = "54"; 
            switch (category)
            {
                case "2 клас": cat = "59"; break;
                case "3 клас": cat = "60"; break;
                case "4 клас": cat = "61"; break;
                case "5 клас": cat = "62"; catBgbook = "55"; break;
                case "6 клас": cat = "63"; catBgbook = "55"; break;
                case "7 клас": cat = "64"; catBgbook = "55"; break;
                case "8 клас": cat = "65"; catBgbook = "55"; break;
                case "9 клас": cat = "66"; catBgbook = "56"; break;
                case "10 клас": cat = "67"; catBgbook = "56"; break;
                case "11 клас": cat = "68"; catBgbook = "56"; break;
                case "12 клас": cat = "69"; catBgbook = "56"; break;
            }

            var jsonModel = new JsonModel
            {
                ID = id,
                title = title,
                author = author,
                cover = cover,
                format = format,
                isbn = isbn,
                image = id + ".jpg",
                price1 = price,
                price2 = discount,
                pages = pages,
                description = description,
                vis = "yes",
                weight = weight,
                kupeno = buyFrom,
                publisher = publisher,
                date = year,
                partndescrip = "",
                subcatIDArea = cat,
                subcatID = ""
            };

            c.JsonModels.Add(jsonModel);

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
                
                var result = id + "^" + title + "^" + author + "^" + pages + "^" + format + "^" + publisher + "^" + cover + "^" + priceInNum + "^" + discount + "^" + id + ".jpg" + "^" + weight + "^" + isbn + "^" + buyFrom + "^" + "<img src=\"/anot/anot" + id + ".jpg\">" + "^" + description + "^" + catBgbook +"#";

                Directory.CreateDirectory(".\\Data");

                if (!File.Exists(".\\Data\\UchebnikData.txt"))
                {
                    File.WriteAllText(".\\Data\\UchebnikData.txt", result);
                }
                else
                {
                    File.AppendAllText(".\\Data\\UchebnikData.txt", result);
                }

                c.tab2_label_message.ForeColor = Color.ForestGreen;
                c.tab2_label_message.Text = $"Книгата \"{title.Substring(0, 30)}\" с id={id}, беше успешно записана.";

                //Update book id after saving 
                var newId = int.Parse(id);
                newId++;
                c.tab2_textBox_id.Text = newId.ToString();
            }
            catch (Exception m)
            {
                var msg = m.Message;
                c.tab2_label_message.ForeColor = Color.Red;
                c.tab2_label_message.Text = "Нещо се счупи :O, обърни се към Даян :Д...";

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

        private void ClearAllTextBoxes(CrawlerDesine c)
        {
            c.BeginInvoke((MethodInvoker)delegate ()
            {
                c.tab2_image.Image = null;
                c.tab2_textBox_title.Text = "";
                c.tab2_textbox_author.Text = "";
                c.tab2_textBox_publisher.Text = "";
                c.tab2_textBox_isbn.Text = "";
                //c.tab2_textBox_cover.Text = "";
                c.tab2_textBox_pages.Text = "";
                c.tab2_textBox_format.Text = "";
                c.tab2_textBox_desc.Text = "";
                //c.tab2_textBox_discount.Text = "";
                c.tab2_textBox_price.Text = "";
                c.tab2_textBox_weight.Text = "";
                c.tab2_textBox_year.Text = "";
                c.tab2_textBox_type.Text = "";
                //c.tab2_textBox_buyFrom.Text = "";
            });
        }
    }
}
