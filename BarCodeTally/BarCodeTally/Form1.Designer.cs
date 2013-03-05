namespace BarCodeTally
{
    partial class BarCodeCount
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtBarCode = new System.Windows.Forms.TextBox();
            this.listBarCodes = new System.Windows.Forms.ListBox();
            this.btnSubtract = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtBarCode
            // 
            this.txtBarCode.Location = new System.Drawing.Point(14, 19);
            this.txtBarCode.Name = "txtBarCode";
            this.txtBarCode.Size = new System.Drawing.Size(200, 21);
            this.txtBarCode.TabIndex = 0;
            this.txtBarCode.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtBarCode_KeyPress);
            // 
            // listBarCodes
            // 
            this.listBarCodes.Location = new System.Drawing.Point(14, 68);
            this.listBarCodes.Name = "listBarCodes";
            this.listBarCodes.Size = new System.Drawing.Size(199, 184);
            this.listBarCodes.TabIndex = 1;
            // 
            // btnSubtract
            // 
            this.btnSubtract.Location = new System.Drawing.Point(135, 43);
            this.btnSubtract.Name = "btnSubtract";
            this.btnSubtract.Size = new System.Drawing.Size(76, 23);
            this.btnSubtract.TabIndex = 2;
            this.btnSubtract.Text = "Reduce";
            this.btnSubtract.Click += new System.EventHandler(this.btnSubtract_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(133, 257);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(80, 29);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "Clear";
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // BarCodeCount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 294);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnSubtract);
            this.Controls.Add(this.listBarCodes);
            this.Controls.Add(this.txtBarCode);
            this.Name = "BarCodeCount";
            this.Text = "BarCodeCount";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtBarCode;
        private System.Windows.Forms.ListBox listBarCodes;


        // Used to decrement Item 
        string activeBarCode;

        public class BarCodeCollection : System.Collections.CollectionBase
        {
            public void Push(string thisBarCode)
            {
                int indexCount = 0;
                int activeIndex = -1;
                BarCode tempBarCode = new BarCode();
                string currBarCode;
                int currCount;

                tempBarCode = new BarCode();
                tempBarCode.barCode = thisBarCode;
                tempBarCode.count = 1;

                if (thisBarCode.Length > 0)
                {
                    foreach (BarCode bc in List)
                    {
                        if (bc.barCode == thisBarCode)
                        {
                            bc.incrBarCode();
                            activeIndex = indexCount;
                            break;
                        }
                        indexCount++;
                    }

                    // BarCode found in list ...
                    if (activeIndex >= 0)
                    {
                        tempBarCode = (BarCode)List[activeIndex];
                        currBarCode = tempBarCode.barCode;
                        currCount = tempBarCode.count;
                    }
                    else
                    // Barcode is not found, Add it to bottom 
                    {
                        List.Add(tempBarCode);
                        activeIndex = List.Count - 1;
                    }

                    // Move current entry to top ...
                    for (int iCount = activeIndex; iCount > 0; iCount--)
                    {
                        List[iCount] = List[iCount - 1];
                    }
                    List[0] = tempBarCode;
                }
            }

            public void Add(BarCode newBarCode)
            {
                List.Add(newBarCode);
            }

            //public void Remove(int index)
            //{
            //    // Check to see if there is a widget at the supplied index.
            //    if (index > Count - 1 || index < 0)
            //    // If no widget exists, a messagebox is shown and the operation 
            //    // is cancelled.
            //    {
            //        System.Windows.Forms.MessageBox.Show("Index not valid!");
            //    }
            //    else
            //    {
            //        List.RemoveAt(index);
            //    }
            //}

            public BarCode Item(int Index)
            {
                // The appropriate item is retrieved from the List object and
                // explicitly cast to the Widget type, then returned to the 
                // caller.
                return (BarCode)List[Index];
            }
        }

        BarCodeCollection barCodeColl = new BarCodeCollection();
        private System.Windows.Forms.Button btnSubtract;
        private System.Windows.Forms.Button btnClear;

        //private void txtBarCode_Validating(object sender, CancelEventArgs e)
        //{
        //    setBarCode(txtBarCode.Text);
        //    rePrintList();
        //    txtBarCode.Text = "";
        //    txtBarCode.Focus();
        //}


        }

        public class BarCode
        {
            public string barCode { get; set; }
            public int count { get; set; }

            public BarCode()
            {

            }

            public void decrBarCode()
            {
                count--;
                if (count <= 0)
                {
                    count = 0;
                }
            }

            public void incrBarCode()
            {
                count++;
            }


        }



}

