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

        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hwnd, IntPtr proccess);
        [DllImport("user32.dll")] static extern IntPtr GetKeyboardLayout(uint thread);
        private static CultureInfo GetCurrentKeyboardLayout()
        {
            try
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                uint foregroundProcess = GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);
                int keyboardLayout = GetKeyboardLayout(foregroundProcess).ToInt32() & 0xFFFF;
                return new CultureInfo(keyboardLayout);
            }
            catch (Exception ex)
            {
                return new CultureInfo(1033); // Assume English if something went wrong.
            }
        }
        public static void FilterView(DataView dv, string filter)
        {
            var ci = GetCurrentKeyboardLayout();

            if (ci.KeyboardLayoutId == 1033)
            {
                dv.RowFilter = $"EnglishName LIKE '%{filter}%'";
            }
            else if (ci.KeyboardLayoutId == 1037)
            {
                dv.RowFilter = $"HebrewName LIKE '%{filter}%'";
            }

        }


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
        public static DataView GetDataSourceFromBinFile(string filePath)
        {
            var data = GetDataFromDbFile(filePath);
            DataTable dt;
            DataView dv;
            dt = new DataTable("names");
            dt.Columns.Add("HebrewName");
            dt.Columns.Add("EnglishName");
            dt.PrimaryKey = new DataColumn[] { dt.Columns[0] };
            foreach (var item in data)
            {
                dt.Rows.Add(new string[] { item.Key, item.Value });
            }

            dv = new DataView(dt);
            return dv;
        }
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

        internal static DataView MergeData(object currenatData, DataView newData)
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
        public static Dictionary<string, string> GetDataFromDbFile(string path)
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
