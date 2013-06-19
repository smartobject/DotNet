using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace WpfSonicTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SonicTestClass sonicTestGroup = new SonicTestClass();
        public SonicResults sr = new SonicResults();
        public BackgroundWorker bgwProgress = new BackgroundWorker();
        public bool cancelTestRun = false;
        private Thread statusThread = null;
        delegate void SetTextCallback(string text);

        public MainWindow()
        {
            InitializeComponent();
            gridNewTest.Visibility = System.Windows.Visibility.Hidden;
            txtSonicTestNameOrig.Visibility = System.Windows.Visibility.Hidden;
            InitializeBackgroundWorker();
        }

        private void InitializeBackgroundWorker()
        {
            bgwProgress.WorkerReportsProgress = true;
            bgwProgress.WorkerSupportsCancellation = true;
            btnCancelTestRun.IsEnabled = false;

            bgwProgress.DoWork +=
                new DoWorkEventHandler(bgwProgress_DoWork);
            bgwProgress.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            bgwProgress_WorkCompleted);
            bgwProgress.ProgressChanged +=
                new ProgressChangedEventHandler(
            bgwProgress_ProgressChanged);
        }

        private void bgwProgress_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progBarTests.Value = e.ProgressPercentage;
            SetText("Progress Bar: " + e.ProgressPercentage.ToString() + "%");

        }

        private void bgwProgress_WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            cancelTestRun = false;
            btnRunTest.IsEnabled = true;
            btnCancelTestRun.IsEnabled = false;
        }

        private void btnAddTest_Click(object sender, RoutedEventArgs e)
        {
            bool itemExists = false;

            if (btnAddTest.Content.ToString() == "Add")
            {
                if (sonicTestGroup.TestCollection == null)
                {
                    sonicTestGroup.TestCollection = new List<SonicTest>();
                }
                SonicTest newTest = new SonicTest();
                newTest.testName = txtSonicTestName.Text;
                newTest.testXML = txtSonicXML.Text;

                sonicTestGroup.TestCollection.Add(newTest);

                foreach (TreeViewItem tv in treeSonicTest.Items)
                {
                    if (tv.Header.ToString() == txtSonicTestName.Text) itemExists = true;
                }

                if (!itemExists)
                {
                    TreeViewItem tv = new TreeViewItem();
                    tv.Header = txtSonicTestName.Text;
                    treeSonicTest.Items.Add(tv);
                }

            }
            else
            {
                // Edit ... 
                foreach (SonicTest st in sonicTestGroup.TestCollection)
                {
                    if (st.testName == txtSonicTestNameOrig.Text)
                    {
                        st.testName = txtSonicTestName.Text;
                        st.testXML = txtSonicXML.Text;
                    }
                }
            }
            gridNewTest.Visibility = System.Windows.Visibility.Hidden; 
        }

        private void btnLoadSonicTest_Click(object sender, RoutedEventArgs e)
        {
           // globalConditionsChanged = false;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "XML documents (.xml)|*.xml"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                txtStatus.Text =  sonicTestGroup.load(dlg.FileName);
            }


            updateTreeSonicTest(); 

        }

        private void updateTreeSonicTest()
        {
            treeSonicTest.Items.Clear(); 

            foreach (SonicTest st in sonicTestGroup.TestCollection)
            {
                TreeViewItem tv = new TreeViewItem();
                tv.Header =  st.testName;
                treeSonicTest.Items.Add(tv);
            }

        }

        private void btnSaveTests_Click(object sender, RoutedEventArgs e)
        {
            string newFileName = "";

            if (sonicTestGroup.SonicTestFileName == null) sonicTestGroup.SonicTestFileName = "*";
            newFileName = getDataFileName(sonicTestGroup.SonicTestFileName);

            sonicTestGroup.SonicTestFileName = newFileName;
            if (!sonicTestGroup.SonicTestFileName.Contains(".xml")) sonicTestGroup.SonicTestFileName = sonicTestGroup.SonicTestFileName + ".xml";
            txtStatus.Text = "Data Schema Name is: " + sonicTestGroup.SonicTestFileName;

            // Process save file to dialog filename 
            sonicTestGroup.save();

            txtStatus.Text = "Saved File name is " + sonicTestGroup.SonicTestFileName;

        }
        private string getDataFileName(string currFileName)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = currFileName; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "XML documents (.xml)|*.xml"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            return dlg.FileName;
        }

        private void treeSonicTest_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem tv = (TreeViewItem)treeSonicTest.SelectedItem;
            txtStatus.Text = "Selected: " + tv.Header; 
        }

        private void btnNewSonicTest_Click(object sender, RoutedEventArgs e)
        {
            gridNewTest.Visibility = System.Windows.Visibility.Visible;
            btnAddTest.Content = "Add";
            txtSonicTestNameOrig.Text = "";
            txtSonicTestName.Text = "";
            txtSonicXML.Text = ""; 
        }

        private void btnRunTest_Click(object sender, RoutedEventArgs e)
        {
            sr.resultCollection = new List<SonicTestResult>();
            cancelTestRun = false;
            btnCancelTestRun.IsEnabled = true;
            btnRunTest.IsEnabled = false;
            SetText("Starting...");
            startAsyncTestRun();
            SetText("Started.");
        }

        private XmlDocument buildSonicMessage(string xmlMessage)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlMessage);

            return doc; 
        }

        private void treeSonicTest_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TreeViewItem tvi = new TreeViewItem();

            tvi = (TreeViewItem)treeSonicTest.SelectedValue;

            gridNewTest.Visibility = System.Windows.Visibility.Visible;
            txtSonicTestName.Text = tvi.Header.ToString();
            txtSonicTestNameOrig.Text = txtSonicTestName.Text; 
            foreach (SonicTest st in sonicTestGroup.TestCollection)
            {
                if (st.testName == txtSonicTestName.Text)
                {
                    txtSonicXML.Text = st.testXML;
                    btnAddTest.Content = "Change";
                }
            }

        }

        private void btnCancelNewTest_Click(object sender, RoutedEventArgs e)
        {
            gridNewTest.Visibility = System.Windows.Visibility.Hidden;
        }

        private void startAsyncTestRun()
        {
            SetText("Tests cancelled.");
            if (bgwProgress.IsBusy != true)
            {
                // Start the asynchronous operation.
                bgwProgress.RunWorkerAsync();
            }
        }


        //----------------------------------------------------------------------------------
        // This is the actual work ... 
        //----------------------------------------------------------------------------------
        private void bgwProgress_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            SetText("Start work");
            string testHostPort = "";
            string testTopic = ""; 
            int totalItems = 0;
            int itemsDone = 0;

            this.txtHostPort.Dispatcher.Invoke(new Action(() => { testHostPort = txtHostPort.Text; }));
            this.txtTopic.Dispatcher.Invoke(new Action(() => { testTopic = txtTopic.Text; }));


            totalItems = sonicTestGroup.TestCollection.Count; 

            try
            {
                foreach (SonicTest st in sonicTestGroup.TestCollection)
                {
                    SetText("Testing: " + st.testName);
                    if (worker.CancellationPending == true || cancelTestRun)
                    {
                        e.Cancel = true;
                        break;
                    }
                    else
                    {
                        // Report progress...
                        itemsDone++;
                        System.Threading.Thread.Sleep(500);
                        (sender as BackgroundWorker).ReportProgress(((itemsDone / totalItems) * 100), null);
                        SetText("Progress: " + (itemsDone).ToString() + " out of " + (totalItems).ToString());
                    }

                    //-------------- Build the Sonic Message Request Doc ------------------
                    //System.IO.File.WriteAllText(st.testName + ".xml", st.testXML);
                    XmlDocument doc = buildSonicMessage(st.testXML);

                    //------------  Request Doc is built -- Now Send on Sonic -----------------------
                    MessageSender sonicSender = new MessageSender(testHostPort, testTopic);
                    XmlDocument response = MessageSender.createDocument();
                    response = sonicSender.send(doc);
                    //------------ Response Received -- Now Process the Response -------------------
                    SonicTestResult str = new SonicTestResult();
                    str.TestTime = DateTime.Now.ToString();
                    str.TestName = st.testName;
                    str.TestResult = response.InnerXml;

                    sr.resultCollection.Add(str);

                    // - TODO: Get the UI thread to update the TreeView ...
                    SetResultTree(str);
                    //TreeViewItem tvi = new TreeViewItem();
                    //tvi.Header = str.TestName;
                    //TreeViewItem tv2 = new TreeViewItem();
                    //tv2.Header = str.TestResult;
                    //tvi.Items.Add(tv2);

                    //this.treeResults.Dispatcher.Invoke(new Action(() => { treeResults.Items.Add(tvi); }));
                    //treeResults.Items.Add(tvi);


                    //txtResponse.Text = response.InnerXml; 

                    //XmlTextWriter writer = new XmlTextWriter(Console.Out);
                    //writer.Formatting = Formatting.Indented;
                    //response.WriteTo(writer);
                    //writer.Flush();
                    //Console.WriteLine();
                    //Console.ReadLine();

                }
            }
            catch (Exception ex)
            {
                SetText(ex.ToString());
            }
            cancelTestRun = false;
            this.btnRunTest.Dispatcher.Invoke(new Action(() => { btnRunTest.IsEnabled = true; }));
            this.btnCancelTestRun.Dispatcher.Invoke(new Action(() => { btnCancelTestRun.IsEnabled = false; }));
        }

        //----------------------------------------------------------------------
        private void btnCancelTestRun_Click(object sender, RoutedEventArgs e)
        {
            if (bgwProgress.WorkerSupportsCancellation == true)
            {
                cancelTestRun = true;
                // Cancel the asynchronous operation.
                bgwProgress.CancelAsync();
            }
        }

        private void SetText(string text)
        {
            //SetTextCallback d = new SetTextCallback(SetText);
            this.txtStatus.Dispatcher.Invoke(new Action(() => { txtStatus.Text = text; }));

        }

        private void SetResultTree(SonicTestResult str)
        {
            this.Dispatcher.Invoke(new System.Action(() => { TreeViewItem tvi = new TreeViewItem();
            tvi.Header = str.TestName;
            TreeViewItem tv2 = new TreeViewItem();
            tv2.Header = str.TestResult;
            tvi.Items.Add(tv2);
            treeResults.Items.Add(tvi);
            }));
        }

        private void btnSaveResults_Click(object sender, RoutedEventArgs e)
        {
            string newFileName = "";
            StringBuilder sb = new StringBuilder();

            newFileName = getDataFileName("*");

            foreach (TreeViewItem tvi in treeResults.Items)
            {
                sb.Append("Test: " + tvi.Header + Environment.NewLine);
                foreach (TreeViewItem tv2 in tvi.Items)
                {
                    sb.Append(tv2.Header + Environment.NewLine);
                }
            }

            System.IO.File.WriteAllText(newFileName, sb.ToString());
            txtStatus.Text = "Saved File name is " + newFileName;
        }


    }
}
