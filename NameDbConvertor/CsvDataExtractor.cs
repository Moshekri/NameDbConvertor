using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameDbConvertor
{
    public class CsvDataExtractor
    {
        public static DataView GetDataSourceFromCsvFile(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            DataTable dt;
            DataView dv;
            dt = new DataTable("names");
            dt.Columns.Add("HebrewName");
            dt.Columns.Add("EnglishName");
            dt.PrimaryKey = new DataColumn[] { dt.Columns[0] };
            foreach (var item in lines)
            {
                var row = item.Split(',');
                var hebrewSplit = row[0].Trim().Split(new char[] { ',', '-', ' ' });
                var englishSplit = row[1].Trim().Split(new char[] { ',', '-', ' ' });
                if (hebrewSplit.Length > 1)
                {
                    try
                    {
                        dt.Rows.Add(new string[] { hebrewSplit[0].Trim(), englishSplit[0] });
                        dt.Rows.Add(new string[] { hebrewSplit[1].Trim(), englishSplit[1] });
                    }
                    catch (Exception ex)
                    {
                        continue;

                    }

                    continue;
                }
                try
                {
                    dt.Rows.Add(new string[] { row[0].Trim(), row[1].Trim() });

                }
                catch (Exception)
                {

                    continue;

                }
            }

            dv = new DataView(dt);
            return dv;
        }
    }
}
