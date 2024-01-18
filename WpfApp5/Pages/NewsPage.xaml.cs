using Flarial.Launcher.Styles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Flarial.Launcher.Pages
{
    /// <summary>
    /// Interaction logic for NewsPage.xaml
    /// </summary>
    public partial class NewsPage : Page
    {
        NewsRoot deserializedNews;

        public NewsPage()
        {
            InitializeComponent();

            string newsUrl = "https://cdn-c6f.pages.dev/launcher/news.json";

            using (WebClient webClient = new WebClient())
            {
                string text = webClient.DownloadString(newsUrl);
                deserializedNews = JsonConvert.DeserializeObject<NewsRoot>(text);
            }

            foreach (News item in deserializedNews.News)
            {
                sv.Children.Add(
                    new NewsItem
                    {
                        Title = item.Title,
                        Body = item.Body,
                        Author = item.Author,
                        RoleName = item.RoleName,
                        RoleColor = item.RoleColor,
                        BackgroundURL = item.Background,
                        AuthorAvatar = item.AuthorAvatar,
                        Date = item.Date
                    }
                );
            }
        }
    }

    public class News
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public string RoleName { get; set; }
        public string RoleColor { get; set; }
        public string AuthorAvatar { get; set; }
        public string Date { get; set; }
        public string Background { get; set; }
    }

    public class NewsRoot
    {
        public List<News> News { get; set; }
    }
}
