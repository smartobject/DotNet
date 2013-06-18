using System;
using System.Collections.Generic;
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

        public MainWindow()
        {
            InitializeComponent();
            gridNewTest.Visibility = System.Windows.Visibility.Hidden;
            txtSonicTestNameOrig.Visibility = System.Windows.Visibility.Hidden;

        }

        private void btnAddTest_Click(object sender, RoutedEventArgs e)
        {
            bool itemExists = false;

            if (btnAddTest.Content == "Add")
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

            try
            {
                foreach (SonicTest st in sonicTestGroup.TestCollection)
                {
                    //-------------- Build the Sonic Message Request Doc ------------------
                    XmlDocument doc = buildSonicMessage(st.testXML);

                    //------------  Request Doc is built -- Now Send on Sonic -----------------------
                    MessageSender sonicSender = new MessageSender(txtHostPort.Text, txtTopic.Text);
                    XmlDocument response = MessageSender.createDocument();
                    response = sonicSender.send(doc);

                    //------------ Response Received -- Now Process the Response -------------------
                    SonicTestResult str = new SonicTestResult();
                    str.TestTime = DateTime.Now.ToString();
                    str.TestName = st.testName;
                    str.TestResult = response.InnerXml;

                    sr.resultCollection.Add(str);

                    TreeViewItem tvi = new TreeViewItem(); 
                    tvi.Header = st.testName; 
                    TreeViewItem tv2 = new TreeViewItem(); 
                    tv2.Header = str.TestResult; 
                    tvi.Items.Add(tv2);

                    treeResults.Items.Add(tvi);


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
                txtStatus.Text = ex.ToString();
            }

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


    }
}
