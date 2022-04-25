using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using CielaCrawler.Model;
using CielaCrawler.Services;
using HtmlAgilityPack;

namespace CielaCrawler
{
    public partial class CrawlerDesine : Form
    {
        private Thread thread;
        private readonly CielaService cielaService = new CielaService();
        private readonly BgUchebnikService bgUchebnikService = new BgUchebnikService();

        public List<JsonModel> JsonModels = new List<JsonModel>();
        public string imageLink = String.Empty;
        public SemaphoreSlim signal = new SemaphoreSlim(0, 1);

        public CrawlerDesine()
        {
            InitializeComponent();
        }

        private void Ciela_Load(object sender, EventArgs e)
        {
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            Application.ExitThread();
            Environment.Exit(0);
            this.Close();
        }

        private void btn_linkStart_Click(object sender, EventArgs e)
        {
            this.thread = new Thread(() => cielaService.GetResult(this));
            this.thread.Start();
        }

        private void btn_skipBook_Click(object sender, EventArgs e)
        {
            signal.Release();
        }

        private void btn_saveBook_Click(object sender, EventArgs e)
        {
            cielaService.SaveBook(this);
        }

        private void tab2_btn_start_Click(object sender, EventArgs e)
        {
            this.thread = new Thread(() => bgUchebnikService.GetResult(this));
            this.thread.Start();
        }

        private void tab2_btn_close_Click(object sender, EventArgs e)
        {
            Application.ExitThread();
            Environment.Exit(0);
            this.Close();
        }

        private void tab2_btn_next_Click(object sender, EventArgs e)
        {
            signal.Release();
        }

        private void tab2_btn_save_Click(object sender, EventArgs e)
        {
            bgUchebnikService.SaveBook(this);
        }

        private void tab2_btn_finish_Click(object sender, EventArgs e)
        {
            var booksJson = JsonSerializer.Serialize(this.JsonModels);

            try
            {
                Directory.CreateDirectory(".\\Data");

                File.WriteAllText(".\\Data\\JsonData.json", booksJson);

                tab2_label_message.ForeColor = Color.ForestGreen;
                tab2_label_message.Text = $"Книгите бяха успешно записани в Json";
            }
            catch (Exception)
            {
                tab2_label_message.ForeColor = Color.Red;
                tab2_label_message.Text = $"Нещо се счупи";
            }
        }
    }
}
