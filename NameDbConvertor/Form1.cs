using Google.Cloud.Translation.V2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Apis.Auth.OAuth2;
using System.Threading;
using NLog;

namespace NameDbConvertor
{
    public partial class Form1 : Form
    {
        DataView dv;
        Logger logger;
        public Form1()
        {
            logger = LogManager.GetCurrentClassLogger();
            InitializeComponent();
            textBox1.Enabled = false;
        }

        private void DataGridView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Link;
        }

        private void SearchTextChanged(object sender, EventArgs e)
        {
            DataGridViewHelper.FilterView(dv, textBox1.Text);
            lblShown.Text = dv.Count.ToString();
        }

        private void DataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            DataView newData = new DataView();
            DataView currenatData = new DataView();

            var filefullname = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            var extention = Path.GetExtension(filefullname[0]);


            foreach (var filename in filefullname)
            {
                if (extention.ToLower().StartsWith(".bin"))
                {
                    try
                    {
                        newData = DataGridViewHelper.GetDataSourceFromBinFile(filename);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
                else if (extention.ToLower().Contains(".txt"))
                {

                    try
                    {
                        newData = DataGridViewHelper.GetDataSourceFromTextFile(filename);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
                else if (extention.ToLower().StartsWith(".csv"))
                {
                    try
                    {
                        newData = DataGridViewHelper.GetDataSourceFromCsvFile(filename);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }


                }
                dataGridView1.DataSource = DataGridViewHelper.MergeData(dataGridView1.DataSource, newData);
            }

            dv = dataGridView1.DataSource as DataView;
            textBox1.Enabled = true;
            lblTotal.Text = dv.Count.ToString();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.DefaultExt = ".bin";
            sfd.Filter = "Bin files | .bin";

            var result = sfd.ShowDialog();
            if (result == DialogResult.OK)
            {
                DataGridViewHelper.SaveBinFile(dataGridView1.DataSource as DataView, sfd.FileName);
            }

        }

        private void Button2_Click(object sender, EventArgs e)
        {
            var data = (dataGridView1.DataSource as DataView);
            data.Table.Clear();
            dataGridView1.DataSource = data;
        }


        bool isInTranslationProcess;
        Thread worker = null;
        private void Button3_Click(object sender, EventArgs e)
        {


            if (!isInTranslationProcess)
            {
                isInTranslationProcess = true;
                btnTranslateAll.Text = "Stop Translating";

            }
            else if (isInTranslationProcess)
            {
                btnTranslateAll.Text = "Re-Translate All";
                isInTranslationProcess = false;
                if (worker.IsAlive)
                {
                    worker.Abort();
                }

            }

            worker = new Thread(new ThreadStart(new Action(() =>
                {
                    var creds = GoogleCredential.FromFile("cred\\cred.json");
                    TranslationClient client = TranslationClient.Create(creds);

                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        try
                        {
                            if (row.Cells[0].Value.ToString() != "")
                            {
                                if (!isInTranslationProcess && worker.IsAlive)
                                {
                                    worker.Abort();
                                }
                                var result = client.TranslateText("מיכאל " + row.Cells[0].Value.ToString(), "en", "he", TranslationModel.NeuralMachineTranslation);
                                var word = result.TranslatedText.Substring(7).Trim();
                                if (word != row.Cells[1].Value.ToString().Trim())
                                {
                                    logger.Info($"Hebrew Name {row.Cells[0].Value.ToString()}:Changed From : {row.Cells[1].Value.ToString()} to : {word}");
                                    row.Cells[1].Value = word;
                                    row.DefaultCellStyle.BackColor = Color.Yellow;
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                            logger.Debug($"Inside worker function , {ex.Message}");
                            client.Dispose();
                        }
                    }

                })));


            worker.Start();

        }

        private void UpdateView(DataGridViewRow row)
        {
            if (dataGridView1.InvokeRequired)
            {
                dataGridView1.Invoke(new Action(() => { UpdateView(row); }));
            }
            else
            {

                dataGridView1.FirstDisplayedScrollingRowIndex = row.Index;

            }
        }
    }
}

