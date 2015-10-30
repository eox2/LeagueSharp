using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YTSharp
{
    class Search
    {
        private string keyword;
        private string link = "https://www.youtube.com/results?search_query=";

        public Search(string keyword)
        {
            this.keyword = keyword;
            var wc = new WebClient{ Encoding = Encoding.UTF8 };
            wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/44.0.2403.157 Safari/537.36");
            var data = wc.DownloadString(link + keyword);

            Console.Write(data);
            MatchCollection results = Regex.Matches(data, "class=\"yt - lockup - thumbnail contains - addto\"><a aria-hidden=\"true\" href=\"(.*?)\" class=\"yt-uix-sessionlink");

            //Console.WriteLine(results);
            foreach (var x in results)
            {
                Console.WriteLine("test");
            }

        }


    }
}
