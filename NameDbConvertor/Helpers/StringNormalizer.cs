using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameDbConvertor
{
    public class StringNormalizer
    {
        public static StringBuilder sb = new StringBuilder();
        public static string NormalizeHebrewName(string data)
        {
            try
            {
                string unwantedChars = "/(){}[]\\";
                sb.Clear();
                if (data != "")
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        if (!unwantedChars.Contains(data[i]))
                        {
                            sb.Append(data[i]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }


            return sb.ToString();
        }
        public static string NormalizeEnglishName(string data)
        {
            try
            {
                string unwantedChars = "/(){}[]\\";
                sb.Clear();
                if (data != "")
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        if (!unwantedChars.Contains(data[i]))
                        {
                            sb.Append(data[i]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            data = sb.ToString();
            sb.Clear();
            if (data.Length > 0)
            {
                sb.Append(data[0].ToString().ToUpper());
                sb.Append(data.Substring(1, data.Length - 1).ToLower());
            }
            return sb.ToString();
        }

    }
}
