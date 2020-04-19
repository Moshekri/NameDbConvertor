using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NameDbConvertor
{
    public class BinDataExtractor
    {
        DataTable dt;
        DataView dv;

        private static BinDataExtractor dataExtractor;
        private BinDataExtractor()
        {
            dt = new DataTable("names");
            dt.Columns.Add("HebrewName");
            dt.Columns.Add("EnglishName");
            dt.PrimaryKey = new DataColumn[] { dt.Columns["HebrewName"] };

        }
        public static BinDataExtractor GetInstance()
        {
            if (dataExtractor == null)
            {
                dataExtractor = new BinDataExtractor();
            }
            return dataExtractor;
        }
        private StringBuilder sb = new StringBuilder();
        public DataView GetDataSourceFromBinFile(string filePath)
        {
            var data = GetDataFromDbFile(filePath);
            foreach (var pair in data)
                try
                {
                    if (pair.Value.Contains('-') || pair.Key.Contains('-'))
                    {
                        HandleHyphen(pair);
                    }
                    else
                    {
                        AddNormalEntry(pair);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            dv = new DataView(dt);
            return dv;
        }
        private void AddNormalEntry(KeyValuePair<string, string> pair)
        {
            string value = GetValueNormalized(pair.Value);
            string key = GetKeyNormalized(pair.Key);
            AddRow(key, value);
        }
        private void HandleHyphen(KeyValuePair<string, string> pair)
        {
            string[] keys = pair.Key.Split('-');
            string[] values = pair.Value.Split('-');
            if (keys.Length == values.Length)
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    string key = GetKeyNormalized(keys[i]);
                    string value = GetValueNormalized(values[i]);
                    AddRow(key, value);
                }
            }
        }
        private void AddRow(string key, string value)
        {
            if (dt.Rows.Find(key) == null)
            {
                dt.Rows.Add(new string[] { key, value });
            }
        }
        private string GetKeyNormalized(string key)
        {
            return StringNormalizer.NormalizeHebrewName(key);
        }
        private string GetValueNormalized(string data)
        {
            return StringNormalizer.NormalizeEnglishName(data);
        }
        private Dictionary<string, string> GetDataFromDbFile(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    using (var fs = File.Open(path, FileMode.Open))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        var data = bf.Deserialize(fs) as Dictionary<string, string>;
                        return data;
                    }

                }
                catch (Exception ex)
                {
                    throw;
                }

            }
            else
            {
                throw new FileNotFoundException($"The File {path} was not found");
            }

        }
    }
}
