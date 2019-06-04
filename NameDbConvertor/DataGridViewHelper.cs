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
                var data = ParseLine(line);
                if (dt.Rows.Find(data[0]) == null)
                {
                    dt.Rows.Add(ParseLine(line));

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

        internal static DataView MergeData(object currenatData, DataView newData)
        {
            if (currenatData == null)
            {
                return newData;
            }

            var data = currenatData as DataView;
            foreach (DataRow row in newData.Table.Rows)
            {
                var result = data.Table.Rows.Find(row[0]);
                if (result == null)
                {
                    data.Table.Rows.Add(row[0],row[1]);
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

        public static void SaveBinFile(DataView data,string filePath)
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
