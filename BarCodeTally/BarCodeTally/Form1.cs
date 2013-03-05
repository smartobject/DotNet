using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BarCodeTally
{
    public partial class BarCodeCount : Form
    {
        public BarCodeCount()
        {
            InitializeComponent();
        }
        private void rePrintList()
        {
            listBarCodes.Items.Clear();
            foreach (BarCode bc in barCodeColl)
            {
                listBarCodes.Items.Add(bc.count.ToString() + " : " + bc.barCode);
            }

        }

        private void txtBarCode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                activeBarCode = txtBarCode.Text;
                barCodeColl.Push(txtBarCode.Text);
                rePrintList();
                txtBarCode.Text = "";
                txtBarCode.Focus();
            }
        }

        private void btnSubtract_Click(object sender, EventArgs e)
        {
            foreach (BarCode bc in barCodeColl)
            {
                if (bc.barCode == activeBarCode)
                {
                    bc.decrBarCode();
                    break;
                }

            }
            rePrintList();
            txtBarCode.Focus();
        }

        private void listBarCodes_GotFocus(object sender, EventArgs e)
        {
            txtBarCode.Focus();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            barCodeColl.Clear();
            listBarCodes.Items.Clear();
            txtBarCode.Focus();
            activeBarCode = "";
        }


    }
}