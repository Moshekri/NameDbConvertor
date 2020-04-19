using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameDbConvertor
{
    public class AdtLogDataExtractor
    {
        public static DataView GetDataSourceFromTextFile(string filePath)
        {
            List<string> filterdLines = new List<string>();
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (line.Contains("| TranslationManager.TranslationManger | Debug  |  Hebrew First Name :"))
                {
                    filterdLines.Add(line);
                }
            }

            DataTable dt = new DataTable("names");

            dt.Columns.Add("HebrewName");
            dt.Columns.Add("EnglishName");
            dt.PrimaryKey = new DataColumn[] { dt.Columns[0] };
            foreach (var line in filterdLines)
            {
                // var splittedLine = 

                var data = ParseLine(line);
                string[] splittedHebrew = data[0].Split(new char[] { ',', ' ', '-' });
                string[] splittedEnglish = data[1].Split(new char[] { ',', ' ', '-' });
                if (splittedHebrew.Length > 1)
                {
                    try
                    {
                        dt.Rows.Add(new string[] { splittedHebrew[0].Trim(), splittedEnglish[0].Trim() });
                        if (splittedHebrew.Length > 1 && splittedEnglish.Length > 1)
                        {
                            dt.Rows.Add(new string[] { splittedHebrew[1].Trim(), splittedEnglish[1].Trim() });
                        }
                        continue;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }


                if (dt.Rows.Find(data[0]) == null)
                {
                    try
                    {
                        dt.Rows.Add(ParseLine(line));
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                }
            }
            return new DataView(dt);
        }

        private static string[] ParseLine(string line)
        {
            string data = line.Split('|')[4];
            string hebrewName = data.Split(',')[0].Split(':')[1].Trim();
            string englishNamePart = data.Split(',')[1];
            int last = englishNamePart.IndexOf("Phonetic");
            string temp = englishNamePart.Substring(0, last);
            string englishName = temp.Split(':')[1].Trim();


            return new string[] { hebrewName, englishName };
        }
    }
}
