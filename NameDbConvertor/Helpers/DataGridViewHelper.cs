using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace NameDbConvertor
{
    public static class DataGridViewHelper
    {

        static bool isHebrew = false;
        static bool isEnglish = false;
        public static void FilterView(DataView dv, string filter)
        {
            string hebrew = "אבגדהוזחטיכלמנסעפצקרשתץךף";
            string english = "abcdefghijklmnopqrstuvwxyz";
            string englishLower = english.ToLower();
            string englishUpper = english.ToUpper();

            if (filter == "")
            {
                isHebrew = false;
                isEnglish = false;
            }

            else if (isEnglish || englishLower.Contains(filter) || englishUpper.Contains(filter))
            {
                dv.RowFilter = $"EnglishName LIKE '%{filter}%'";
                isEnglish = true;

            }

            else if (isHebrew || hebrew.Contains(filter))
            {
                dv.RowFilter = $"HebrewName LIKE '%{filter}%'";
                isHebrew = true;

            }


        }
        public static DataView GetDataSourceFromTextFile(string filePath)
        {
            return AdtLogDataExtractor.GetDataSourceFromTextFile(filePath);
        }
        public static DataView GetDataSourceFromBinFile(string filePath)
        {
            var b = BinDataExtractor.GetInstance();
            return b.GetDataSourceFromBinFile(filePath);
        }
        public static DataView GetDataSourceFromCsvFile(string filePath)
        {
            return CsvDataExtractor.GetDataSourceFromCsvFile(filePath);
        }
        internal static DataView MergeData(DataView currenatData, DataView newData)
        {
            if (currenatData == null)
            {
                return newData;
            }

            var data = currenatData as DataView;
            foreach (DataRow row in newData.Table.Rows)
            {
                if (data.Table != null)
                {
                    var result = data.Table.Rows.Find(row[0]);
                    if (result == null)
                    {
                        data.Table.Rows.Add(row[0], row[1]);
                    }
                }

            }
            return data;

        }
        public static void SaveBinFile(DataView data, string filePath)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            DataTable dt = data.ToTable();
            foreach (DataRow row in dt.Rows)
            {
                string value;
                if (!dict.TryGetValue(row[0].ToString(), out value))
                {
                    dict.Add(row[0].ToString(), row[1].ToString());
                }

            }

            BinaryFormatter bf = new BinaryFormatter();
            using (var fs = File.Create(filePath))
            {
                bf.Serialize(fs, dict);
            }






        }



    }
}
