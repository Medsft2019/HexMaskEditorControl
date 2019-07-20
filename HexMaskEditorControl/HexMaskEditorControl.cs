using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using dnlib.IO;
using dnlib.DotNet;

namespace HexMaskEditorControl
{
    public partial class HexMaskEditorControl : UserControl
    {
        
        private TextBox Editor;
        public HexMaskEditorControl()
        {
            InitializeComponent();
            Editor = new TextBox();
            Editor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            InlistView.HideSelection = true;
            Editor.Multiline = false;
            Editor.Visible = false;
            Editor.MaxLength = 2;
            Editor.KeyPress += Editor_KeyPress;
            this.Controls.Add(Editor);
            this.Editor.Size = new System.Drawing.Size(25, 25);
            this.Editor.Text = "";

        }

        #region SetInitialMaterial
        /// <summary>
        /// Method based on dnLib
        /// </summary>
        /// <param name="md"></param>
        /// <returns></returns>
        public static byte[] SetInputByMethodDef(MethodDef md)
        {
            if (md is null || md.RVA == 0)
                return null;
            var mod = md.Module as ModuleDefMD;
            if (mod is null)
                return null;

            try
            {
                var reader = mod.Metadata.PEImage.CreateReader();
                reader.Position = (uint)mod.Metadata.PEImage.ToFileOffset(md.RVA);
                var start = reader.Position;
                if (!ReadHeader(ref reader, out ushort flags, out uint codeSize))
                    return null;
                start = reader.Position;
                byte[] toOut = new byte[codeSize - reader.Position];
                for (int i = 0; i < codeSize; i++)
                {
                    toOut[i] = reader.ReadByte();
                }
                return toOut;
            }
            catch
            {
                return null;
            }
        }

        static bool ReadHeader(ref DataReader reader, out ushort flags, out uint codeSize)
        {
            byte b = reader.ReadByte();
            switch (b & 7)
            {
                case 2:
                case 6:
                    flags = 2;
                    codeSize = (uint)(b >> 2);
                    return true;

                case 3:
                    flags = (ushort)((reader.ReadByte() << 8) | b);
                    uint headerSize = (byte)(flags >> 12);
                    ushort maxStack = reader.ReadUInt16();
                    codeSize = reader.ReadUInt32();
                    uint localVarSigTok = reader.ReadUInt32();

                    reader.Position = reader.Position - 12 + headerSize * 4;
                    if (headerSize < 3)
                        flags &= 0xFFF7;
                    return true;

                default:
                    flags = 0;
                    codeSize = 0;
                    return false;
            }
        }

        /// <summary>
        /// Method based on System.Reflection
        /// </summary>
        /// <param name="pathToAssembly" descrition ="Full path to assembly"></param>
        /// <param name="className" descrition ="Simple type name"></param>
        /// <param name="methodName" descrition ="Simple methodInfo name"></param>
        public void SetInputByMethodName(string pathToAssembly, string className, string methodName)
        {
            try
            {
                Assembly assembly = Assembly.Load(System.IO.File.ReadAllBytes(pathToAssembly));
                string fileName = assembly.ManifestModule.ScopeName.Substring(0, assembly.ManifestModule.ScopeName.Length-4);
                MethodInfo mi = assembly.GetType(string.Format("{0}.{1}", fileName, className)).GetMethod(methodName);
                if (mi != null)
                {
                    byte[] msil = mi.GetMethodBody().GetILAsByteArray();
                    SetInputAsByteArray(mi.GetMethodBody().GetILAsByteArray());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error assembly loading");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inArray" descrition ="ByteArray as stringArray"></param>
        public void SetInputAsStringArray(string[] inArray)
        {
            this.InlistView.Items.Clear();
            this.OutListView.Items.Clear();
            int counterColumn = -1;
            ListViewItem lvi = new ListViewItem();
            bool addItem = false;
            for (int i = 0; i < inArray.Length; i++)
            {
                addItem = false;
                counterColumn++;
                if (counterColumn > 16)
                {
                    counterColumn = 0;
                    lvi.UseItemStyleForSubItems = false;
                    InlistView.Items.Add(lvi);
                    ListViewItem lviclone = (ListViewItem)lvi.Clone();
                    OutListView.Items.Add(lviclone);
                    lvi = new ListViewItem(inArray[i]);
                    addItem = true;
                }
                else
                {
                    if (counterColumn == 0)
                    {
                        lvi.Text = inArray[i];
                    }
                    else
                    {
                        lvi.SubItems.Add(inArray[i]);
                    }
                } 
            }
            if (!addItem && lvi.Text != "")
            {
                lvi.UseItemStyleForSubItems = false;
                InlistView.Items.Add(lvi);
                ListViewItem lviclone = (ListViewItem)lvi.Clone();
                OutListView.Items.Add(lviclone);
            }
            InlistView.Items[0].SubItems[0].BackColor = Color.YellowGreen;
            OutListView.Items[0].SubItems[0].BackColor = Color.YellowGreen;
            InlistView.Refresh();
            OutListView.Refresh();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="instring" descrition ="ByteArray as string"></param>
        public void SetInputAsString(string instring)
        {
            char[] preArray1 = instring.Trim().ToArray();
            List<string> preList = new List<string>();
            for (int i = 0; i < preArray1.Length; i+=2)
            {
                preList.Add(new string(new char[2] { preArray1[i], preArray1[i + 1] }));
            }
            SetInputAsStringArray(preList.ToArray());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inbytes" descrition ="Initial ByteArray"></param>
        public void SetInputAsByteArray(byte[] inbytes)
        {
            List<string> preList = new List<string>();
            for (int i = 0; i < inbytes.Length; i++)
            {
                preList.Add(inbytes[i].ToString("x2"));
            }
            SetInputAsStringArray(preList.ToArray());
        }
        #endregion

        #region GetOutPutMaterial
        public string[] GetOutPutAsStringArray()
        {
            List<string> preOut = new List<string>();
            for (int i = 0; i < OutListView.Items.Count; i++)
            {
                preOut.Add(OutListView.Items[i].Text);
                for (int x = 1; x < 16; x++)
                {
                    preOut.Add(OutListView.Items[i].SubItems[x].Text);
                }
            }
            return preOut.ToArray();
        }
        public string GetOutPutAsString()
        {
            string preOut = string.Empty;
            for (int i = 0; i < OutListView.Items.Count; i++)
            {
                preOut += OutListView.Items[i].Text;
                for (int x = 1; x < OutListView.Items[i].SubItems.Count; x++)
                {
                    preOut += OutListView.Items[i].SubItems[x].Text;
                }
            }
            return preOut;
        }
        #endregion

        #region ControlGuiLogic
        Point locationCursor = new Point(0, 0);
        private void InlistView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
            {
                if (locationCursor.X != 0 && InlistView.Items[locationCursor.Y].SubItems[locationCursor.X - 1].Text != string.Empty)
                {
                    InlistView.Items[locationCursor.Y].SubItems[locationCursor.X].BackColor = DefaultBackColor;
                    OutListView.Items[locationCursor.Y].SubItems[locationCursor.X].BackColor = DefaultBackColor;
                    locationCursor.X -= 1;
                }
            }
            if (e.KeyCode == Keys.Right)
            {
                if (locationCursor.X+1<16 && locationCursor.X < InlistView.Items[locationCursor.Y].SubItems.Count-1 && InlistView.Items[locationCursor.Y].SubItems[locationCursor.X + 1].Text != string.Empty)
                {
                    InlistView.Items[locationCursor.Y].SubItems[locationCursor.X].BackColor = DefaultBackColor;
                    OutListView.Items[locationCursor.Y].SubItems[locationCursor.X].BackColor = DefaultBackColor;
                    locationCursor.X += 1;
                }
            }
            if (e.KeyCode == Keys.Up)
            {
                if (locationCursor.Y != 0 && InlistView.Items[locationCursor.Y - 1].SubItems[locationCursor.X].Text != string.Empty)
                {
                    InlistView.Items[locationCursor.Y].SubItems[locationCursor.X].BackColor = DefaultBackColor;
                    OutListView.Items[locationCursor.Y].SubItems[locationCursor.X].BackColor = DefaultBackColor;
                    locationCursor.Y -= 1;
                }
            }
            if (e.KeyCode == Keys.Down)
            {
                if (locationCursor.Y + 1 < InlistView.Items.Count &&
                    InlistView.Items[locationCursor.Y + 1].SubItems.Count > locationCursor.X &&
                    InlistView.Items[locationCursor.Y + 1].SubItems[locationCursor.X].Text != string.Empty)
                {
                    InlistView.Items[locationCursor.Y].SubItems[locationCursor.X].BackColor = DefaultBackColor;
                    OutListView.Items[locationCursor.Y].SubItems[locationCursor.X].BackColor = DefaultBackColor;
                    locationCursor.Y += 1;
                }
            }

            InlistView.Items[locationCursor.Y].SubItems[locationCursor.X].BackColor = Color.YellowGreen;
            OutListView.Items[locationCursor.Y].SubItems[locationCursor.X].BackColor = Color.YellowGreen;
        }

        private void InlistView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            ((ListView)sender).SelectedItems.Clear();
            ((ListView)sender).FocusedItem = null;
            //InlistView.Refresh();
        }

        private void InlistView_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hti = ((ListView)sender).HitTest(e.Location);
            if (hti.Item == null) return;
            InlistView.Items[locationCursor.Y].SubItems[locationCursor.X].BackColor = DefaultBackColor;
            OutListView.Items[locationCursor.Y].SubItems[locationCursor.X].BackColor = DefaultBackColor;
            ListViewItem item = hti.Item;
            locationCursor.Y = item.Index;
            locationCursor.X = item.SubItems.IndexOf(hti.SubItem);
            InlistView.Items[locationCursor.Y].SubItems[locationCursor.X].BackColor = Color.YellowGreen;
            OutListView.Items[locationCursor.Y].SubItems[locationCursor.X].BackColor = Color.YellowGreen;
        }

        private void OutListView_SubItemClicked(object sender, SubItemEventArgs e)
        {
            InlistView.Items[e.Item.Index].SubItems[e.SubItem].BackColor = Color.YellowGreen;
            OutListView.Items[e.Item.Index].SubItems[e.SubItem].BackColor = Color.YellowGreen;
            OutListView.StartEditing(Editor, e.Item, e.SubItem);
        }

        List<int> allowKeyKodes = new List<int> { 8, 63, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 65, 66, 67, 68, 69, 70, 97, 98, 99, 100, 101, 102 };
        private void Editor_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!allowKeyKodes.Contains(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        #endregion
    }
}
