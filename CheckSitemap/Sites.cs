using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckSitemap
{
    class Sites
    {
        public static string[] Odor
        {
            get
            {
                string[] links = new string[] { "https://www.odor.sk/sitemap.xml" };
                return links;
            }
            internal set { }
        }
        public static string[] Komvak
        {
            get
            {
                string[] links = new string[] { "http://komvak2.dev.gls.sk/sitemap_20200902.xml" };
                return links;
            }
            internal set { }
        }
    }
}