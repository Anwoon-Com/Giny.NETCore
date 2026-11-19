using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giny.World.Web.Controllers
{
    public class NewsItem
    {
        public string Title
        {
            get;
            set;
        }
        public string Content
        {
            get;
            set;
        }
        public DateTime Date
        {
            get;
            set;
        }
        public string Author
        {
            get;
            set;
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class NewsController : ControllerBase
    {
        private const string NewsFile = "news.json";

        [HttpGet]
        public List<NewsItem> GetNews()
        {
            if (!System.IO.File.Exists(NewsFile))
            {
                var defaults = new List<NewsItem>()
                {
                    new NewsItem()
                    {
                        Title = "Bienvenue !",
                        Content = "Le serveur est en ligne.",
                        Date = DateTime.Now,
                        Author = "Serveur",
                    },
                };

                System.IO.File.WriteAllText(NewsFile, JsonConvert.SerializeObject(defaults, Formatting.Indented));
                return defaults;
            }

            return JsonConvert.DeserializeObject<List<NewsItem>>(System.IO.File.ReadAllText(NewsFile));
        }
    }
}
