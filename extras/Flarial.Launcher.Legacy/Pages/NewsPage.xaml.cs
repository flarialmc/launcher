using Flarial.Launcher.Styles;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Flarial.Launcher.Pages;

/// <summary>
/// Interaction logic for NewsPage.xaml
/// This should never fire since News is broken for some reason, goofy ah ah launcher.
/// </summary>
public partial class NewsPage : Page
{
    // readonly NewsRoot deserializedNews;

    public NewsPage()
    {
        InitializeComponent();

        /*
        Task.Run(() =>
        {
            string newsUrl = "https://raw.githubusercontent.com/flarialmc/newcdn/main/launcher/news.json";

            using (WebClient webClient = new WebClient())
            {
                string text = webClient.DownloadString(newsUrl);
                deserializedNews = JsonConvert.DeserializeObject<NewsRoot>(text);
            }

            // Use the Dispatcher to run the UI update code on the UI thread
            Dispatcher.Invoke(() =>
            {
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
            });
        });
        */
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
