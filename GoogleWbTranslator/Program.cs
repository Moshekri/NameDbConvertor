using System;
using System.Net;
using System.Text;

namespace GoogleWbTranslator
{
    class Program
    {
        static void Main(string[] args)
        {

            WebClient web = new WebClient();
            web.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0");
            web.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");

            // Make sure we have response encoding to UTF-8
            web.Encoding = Encoding.UTF8;
            var response =   web.DownloadString($"https://translate.google.com/#view=home&op=translate&sl=iw&tl=en&text=%D7%A9%D7%9C%D7%95%D7%9D&client=j");



            string word = "למה";
            WebRequest req = WebRequest.Create($"https://translate.google.com/#view=home&op=translate&sl=iw&tl=en&text=למה");
            WebResponse res =  req.GetResponse();
            

            
        }
    }
}
