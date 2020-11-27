using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;

namespace _4Story_TInfo_Editor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            StatusLabel.Text = "Status";
        }

        public List<DetailInfo> Data = new List<DetailInfo>();

        public class DetailInfo
        {
            public uint m_dwID { get; set; }
            public byte m_bCount { get; set; }
            public List<string> m_pTEXT = new List<string>();
        }

        public void WriteUInt16Length(BinaryWriter bw, int length)
        {
            bw.Write(byte.MaxValue);
            bw.Write((ushort)length);
        }

        public void WriteByteLength(BinaryWriter bw, int length)
        {
            bw.Write((byte)length);
        }

        public void ReadBinary(Stream stream)
        {
            Data.Clear();
            listBox1.Items.Clear();
            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();

            StatusLabel.Text = "Loading data...";
            Application.DoEvents();

            BinaryReader reader = new BinaryReader(stream, Encoding.Default);
            ushort wCount = reader.ReadUInt16();
            for (ushort i = 0; i < wCount; i++)
            {
                DetailInfo info = new DetailInfo();
                info.m_dwID = reader.ReadUInt32();
                info.m_bCount = reader.ReadByte();
                for (byte j = 0; j < info.m_bCount; j++)
                {
                    ushort length = reader.ReadByte();
                    if (length >= byte.MaxValue)
                        length = reader.ReadUInt16();
                    info.m_pTEXT.Add(Encoding.Default.GetString(reader.ReadBytes(length)));
                }
                Data.Add(info);
                listBox1.Items.Add(i);
            }

            StatusLabel.Text = "Data loaded.";
            stream.Close();
        }

        public void SaveBinary(Stream stream)
        {
            StatusLabel.Text = "Saving data...";
            Application.DoEvents();

            BinaryWriter writer = new BinaryWriter(stream, Encoding.Default);
            writer.Write((ushort)Data.Count);
            for(int i = 0; i < Data.Count; i++)
            {
                writer.Write(Data[i].m_dwID);
                writer.Write(Data[i].m_bCount);
                for (int j = 0; j < Data[i].m_pTEXT.Count; j++)
                {
                    int length = Data[i].m_pTEXT[j].Length;
                    if (length >= byte.MaxValue)
                        WriteUInt16Length(writer, length);
                    else
                        WriteByteLength(writer, length);
                    writer.Write(Encoding.Default.GetBytes(Data[i].m_pTEXT[j]));
                }
            }

            StatusLabel.Text = "Data saved.";
            stream.Close();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All files (*.*)|*.*";
            ofd.FilterIndex = 2;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ReadBinary(ofd.OpenFile());
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "All files (*.*)|*.*";
            sfd.FilterIndex = 2;
            sfd.RestoreDirectory = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                SaveBinary(sfd.OpenFile());
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                dataGridView1.Rows.Clear();
                dataGridView1.Refresh();
                for (int i = 0; i < Data[listBox1.SelectedIndex].m_pTEXT.Count; i++)
                {
                    int index = dataGridView1.Rows.Add();
                    dataGridView1.Rows[index].Cells[dataGridView1.Columns["TextColumn"].Index].Value = Data[listBox1.SelectedIndex].m_pTEXT[i];
                    dataGridView1.Rows[i].HeaderCell.Value = i.ToString();
                }
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            for (int i = 0; i < Data[listBox1.SelectedIndex].m_pTEXT.Count; i++)
            {
                Data[listBox1.SelectedIndex].m_pTEXT[i] = (string)dataGridView1.Rows[i].Cells[dataGridView1.Columns["TextColumn"].Index].Value;
            }
        }

        private void dataGridView1_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                Data[listBox1.SelectedIndex].m_pTEXT.Add("");
                Data[listBox1.SelectedIndex].m_bCount += 1;
            }
        }

        private void dataGridView1_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                Data[listBox1.SelectedIndex].m_pTEXT.RemoveAt(dataGridView1.CurrentCell.RowIndex);
                Data[listBox1.SelectedIndex].m_bCount -= 1;
            }
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DetailInfo info = new DetailInfo();
            info.m_dwID = Data.Count > 0 ? Data.Last().m_dwID + 1 : 0;
            info.m_bCount = 0;
            Data.Add(info);
            listBox1.Items.Clear();
            for (int i = 0; i < Data.Count; i++)
                listBox1.Items.Add(i);  
        }

        private void removeItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Data.RemoveAt(listBox1.SelectedIndex);
            listBox1.Items.Clear();
            dataGridView1.Rows.Clear();
            dataGridView1.Refresh();
            for (int i = 0; i < Data.Count; i++)
                listBox1.Items.Add(i);
        }

        private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (int)Keys.Enter)
            { 
                if (toolStripTextBox1.TextLength > 0)
                {
                    Dictionary<int, List<int>> ItemsFound = new Dictionary<int, List<int>>();
                    ItemsFound.Clear();
                    for (int i = 0; i < Data.Count; i++)
                    {
                        List<int> TableItems = new List<int>();
                        bool bContains = false;
                        for (int j = 0; j < Data[i].m_pTEXT.Count; j++)
                        {
                            if (Data[i].m_pTEXT[j].Contains(toolStripTextBox1.Text))
                            { 
                                TableItems.Add(j);
                                bContains = true;
                            }
                        }
                        if(bContains)
                            ItemsFound.Add(i, TableItems);
                    }

                    Form2 Results = new Form2(ItemsFound, this);
                    Results.Show();
                }
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            int width = ClientRectangle.Width / 2;
            listBox1.Left = 0;
            listBox1.Width = width / 2;
            dataGridView1.Left = 0;
            dataGridView1.Width = width + width / 2;
        }

        public void SelectItem(int index)
        {
            listBox1.SelectedIndex = index;
        }
    }
}
