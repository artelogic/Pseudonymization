using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConnectionStringsCom.GetAllSamples
{
    // this is just temp helper to look through all pages per words
    class Program
    {
        static void Main(string[] args)
        {
            string requested = "Integrated Security";

            var urls = new List<string>();

            var request = WebRequest.Create("https://www.connectionstrings.com/");
            using (var reader = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                var text = reader.ReadToEnd();

                //var sele

                foreach (var reg in Regex.Matches(text, "<a href=\".+?\" class=\"relevance1\">").Cast<Match>().Select(m => m.Value))//.SelectMany(g => g.Groups.Cast<Group>()).Select(g => g.Value))
                {
                    urls.Add(reg.Split(new[] { '"' }, StringSplitOptions.RemoveEmptyEntries)[1]);
                }
            }

            foreach (var url in urls)
            {
                request = WebRequest.Create("https://www.connectionstrings.com" + url);
                using (var reader = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    var text = reader.ReadToEnd();
                    if (text.ToLower().Contains(requested.ToLower()))
                    {
                        Console.WriteLine($"{url} contains {requested}");
                    }
                }
            }

            Console.WriteLine("All processed");
            Console.ReadKey();
        }

    }
}
