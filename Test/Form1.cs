using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            generateTest1();
        }

        private void generateTest()
        {
            string test = "AABBCCDDEEFF010203040506070809A0B0C0D0E0F0AABBCCDDEEFF";
            hexEditorControl1.SetInputAsString(test);
        }

        private void generateTest1()
        {
            string path = "J:\\Test\\Cyfuscator.exe";
            hexEditorControl1.SetInputByMethodName(path, "Tasks", "Parse");
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(hexEditorControl1.GetOutPutAsString());
        }
    }
}
