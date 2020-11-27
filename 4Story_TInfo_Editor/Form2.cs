using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _4Story_TInfo_Editor
{
    public partial class Form2 : Form
    {
        Form1 mainForm;
        Dictionary<int, List<int>> Items = new Dictionary<int, List<int>>();
        public Form2(Dictionary<int, List<int>> ItemsFound, Form1 Form)
        {
            InitializeComponent();
            mainForm = Form;
            Items = ItemsFound;
            for (int i = 0; i < Items.Count; i++)
            {
                KeyValuePair<int, List<int>> Item = Items.ElementAt(i);
                string strItem = "Item: " + Item.Key + " [";
                for (int j = 0; j < Item.Value.Count; j++)
                    strItem += (j > 0 ? ", " : "") + Item.Value[j];
                strItem += " ]";
                listBox1.Items.Add(strItem);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    if (i == listBox1.SelectedIndex)
                    { 
                        KeyValuePair<int, List<int>> Item = Items.ElementAt(i);
                        mainForm.SelectItem(Item.Key);
                    }
                }
            }
        }
    }
}
