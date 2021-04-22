using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Apprentice.Tools {
    public static class WebScraper {

        public static async Task<string> DownloadUrl(string url) {
            try {
                WebClient wc = new WebClient();
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                return await wc.DownloadStringTaskAsync(url);
            } catch {
                return null;
            }
        }
    }
}
