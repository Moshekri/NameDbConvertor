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
            dataExtractor = new BinDataExtractor();

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
                    string value = GetValueNormalized(pair.Value);
                    string key = GetKeyNormalized(pair.Key);
                    AddRow(key, value);
                    
                }
                catch (Exception ex)
                {
                    throw;
                }
            dv = new DataView(dt);
            return dv;
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
            try
            {
                string unwantedChars = "/(){}[]\\";
                sb.Clear();
                if (key != "")
                {
                    for (int i = 0; i < key.Length; i++)
                    {
                        if (!unwantedChars.Contains(key[i]))
                        {
                            sb.Append(key[i]);
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
        private string GetValueNormalized(string data)
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
