using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using XmlUtil; 

namespace wpfRules
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string dataSchemaName = "";
        public static string ruleConditionName = "";
        public static bool globalDataChanged = false;
        public static bool globalConditionsChanged = false;
        IDCounter idCounter = new IDCounter(); 
        RuleDataSchema currSchema = new RuleDataSchema();
        RuleConditionSet ruleConditionSet = new RuleConditionSet();
        RuleMasterList ruleMasterList = null;
        RuleMaster currentRuleMaster = null; 

        string[] dataTypes = { "CHAR", "NUMBER", "DATE", "BOOL" };
        string[] operTypes = { "CHAR", "NUMERIC", "DATE", "BOOL" };
        string[] charOperators = { "=", "<>", "IN_LIST", "NOT_IN_LIST", "IS_BLANK", "IS_NOT_BLANK" };
        string[] numericOperators = { "=", "<>", ">", "<", ">=", "<=", "BETWEEN" };
        string[] dateOperators = { "=", "<>", ">", "<", ">=", "<=", "AGE", "BETWEEN", "IS_BLANK", "IS_NOT_BLANK" };
        string[] boolOperators = { "true", "false", "IS_BLANK", "IS_NOT_BLANK" };
        string[] whichTypes = { "FIRST", "EVERY", "FIRSTN" };
        string[] valueListTypes = { "FREEFORM", "LIST" };
        RuleCondition workingCondition = null;

         public MainWindow()
        {
            InitializeComponent();
            currSchema.DataSchemaName = "";
            currSchema.DataObjects = new List<DataObject>();
            gridDataObject.Visibility = System.Windows.Visibility.Hidden;
            gridConditions.Visibility = System.Windows.Visibility.Hidden;
            GridRuleMaster.Visibility = System.Windows.Visibility.Hidden;


            foreach (string thisValue in dataTypes)
            {
                cmbDataType.Items.Add(thisValue);
            }
            foreach (string thisValue in whichTypes)
            {
                cmbNewWhich.Items.Add(thisValue);
            }

             
        }

        private void btnLoadDataSchema_Click(object sender, RoutedEventArgs e)
        {
            globalDataChanged = false; 
            string xmlcontents = ""; 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Schema"; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "XML documents (.xml)|*.xml"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                dataSchemaName = dlg.FileName;
                currSchema.DataSchemaName = dataSchemaName;
                txtDataSchemaName.Text = dataSchemaName;
                txtConditionsDataSchema.Text = dataSchemaName;
            }

            // Get the File and Load it to XML Doc
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load( dataSchemaName );
                xmlcontents = doc.InnerXml;
                txtStatus.Text = "File: " + dataSchemaName + " loaded.";
            }
            catch (Exception ex)
            {
                txtStatus.Text = "Error loading file: " + dataSchemaName;
            }

            // Convert the XML Doc to the Class
            currSchema = (RuleDataSchema)XMLUtility.Deserialize<RuleDataSchema>(xmlcontents);

            // Display the Class in the Tree View 
            updateTreeView(); 

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            bool thisCancel = false; 

            // --  New Data Schema --
            if (globalDataChanged)
            {
                MessageBoxResult mbr = MessageBox.Show("Data Schema has changed -> Save Changes? ", "Data Changed!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);
                if (mbr == MessageBoxResult.Cancel)
                {
                    thisCancel = true; 
                }
                else
                {
                    if (mbr == MessageBoxResult.Yes)
                    {
                        RoutedEventArgs e2 = new RoutedEventArgs();
                        btnSaveDataSchema_Click(sender, e2);
                    }
                }
            }

            if (thisCancel == false)
            {
                currSchema.DataObjects.Clear();
                currSchema.DataSchemaName = "";
                updateTreeView();
                txtConditionSetName.Text = "";
                txtDataSchemaName.Text = "";
                ruleConditionSet.RuleConditions.Clear();
                ruleConditionSet.ConditionName = "";
                updateTreeViewConditions(); 
            }
        }

        private void btnNewField_Click(object sender, RoutedEventArgs e)
        {
            gridDataObject.Visibility = System.Windows.Visibility.Visible;

            LabelEditFunction.Content = "New Field ..."; 

            // -- Clear fields: 
            Visual thisGrid = (Visual) gridDataObject; 
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(thisGrid); i++)
             {
                // Retrieve child visual at specified index value.
                Visual childVisual = (Visual)VisualTreeHelper.GetChild(thisGrid, i);

                // Do processing of the child visual object.
                 if (childVisual is TextBox)
                 {
                    TextBox tb = childVisual as TextBox;
                    tb.Text = "";
                 }
                 if (childVisual is ComboBox)
                 {
                     ComboBox cb = childVisual as ComboBox;
                     cb.SelectedIndex = -1;
                 }

             }
        }

        private void btnCancelDataObject_Click(object sender, RoutedEventArgs e)
        {
            // Check if data was entered and confirm cancel 
            gridDataObject.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btnOKDataObject_Click(object sender, RoutedEventArgs e)
        {
            if (LabelEditFunction.Content.ToString().StartsWith("Edit" ))
            {
                string origFieldNum = "";
                foreach (System.Collections.DictionaryEntry rd in txtFieldNumber.Resources)
                {
                    if (rd.Key == "Orig" ) origFieldNum = rd.Value.ToString();
                }


                DataObject thisDO = currSchema.DataObjects.Find(DataObject => DataObject.FieldNumber == Convert.ToInt32(origFieldNum));
                if (thisDO == null)
                    txtStatus.Text = "Cannot get field: '" + origFieldNum + "' ";
                else
                {
                    globalDataChanged = origTextChanged(txtFieldNumber, txtFieldNumber.Text);
                    globalDataChanged = origTextChanged(txtFieldName, txtFieldName.Text);
                    globalDataChanged = origTextChanged(txtFieldDescr, txtFieldDescr.Text);
                    globalDataChanged = origTextChanged(txtFieldLabel, txtFieldLabel.Text);
                    globalDataChanged = origTextChanged(txtTableName, txtTableName.Text);
                    globalDataChanged = origTextChanged(cmbDataType, cmbDataType.SelectedValue.ToString());
                }
            }
            else
            {
                globalDataChanged = true; 
                DataObject newDO = new DataObject();
                newDO.FieldNumber = Convert.ToInt32(txtFieldNumber.Text);
                newDO.FieldName = txtFieldName.Text;
                newDO.FieldLabel = txtFieldLabel.Text;
                newDO.DataType = cmbDataType.SelectedItem.ToString(); 
                newDO.FieldDescr = txtFieldDescr.Text;
                newDO.TableName = txtTableName.Text;

                currSchema.DataObjects.Add(newDO);
            }

            updateTreeView();

            gridDataObject.Visibility = System.Windows.Visibility.Hidden;

        }

        private bool origTextChanged(Control thisControl, string currValue )
        {
            foreach (System.Collections.DictionaryEntry rd in thisControl.Resources)
            {
                if (rd.Key == "Orig" && rd.Value != currValue) return true;
            }
         return false; 
        }

        private void updateTreeView()
        {
            treeDataModel.Items.Clear();

            if (currSchema != null && currSchema.DataObjects != null )
            {
                foreach (DataObject thisDO in currSchema.DataObjects)
                {
                    TreeViewItem dataFieldItem = new TreeViewItem() { Header = thisDO.FieldNumber + "| " + thisDO.FieldName };
                    TreeViewItem dataFieldNumber = new TreeViewItem() { Header = "Field Number: " + thisDO.FieldNumber };
                    TreeViewItem dataFieldLabel = new TreeViewItem() { Header = "Field Label: " + thisDO.FieldLabel };
                    TreeViewItem dataFieldDescr = new TreeViewItem() { Header = "Field Descr: " + thisDO.FieldDescr };
                    TreeViewItem dataFieldDataType = new TreeViewItem() { Header = "Data Type: " + thisDO.DataType };
                    TreeViewItem dataFieldTableName = new TreeViewItem() { Header = "Table Name: " + thisDO.TableName };

                    dataFieldItem.Items.Add(dataFieldNumber);
                    dataFieldItem.Items.Add(dataFieldLabel);
                    dataFieldItem.Items.Add(dataFieldDescr);
                    dataFieldItem.Items.Add(dataFieldDataType);
                    dataFieldItem.Items.Add(dataFieldTableName);

                    treeDataModel.Items.Add(dataFieldItem);
                }
            }
        }

        private void btnSaveDataSchema_Click(object sender, RoutedEventArgs e)
        {
            string newFileName = "";

            if (currSchema.DataSchemaName == null) currSchema.DataSchemaName = "*";
            newFileName = getDataFileName(currSchema.DataSchemaName);

            dataSchemaName = newFileName;
            currSchema.DataSchemaName = dataSchemaName;
            if (!currSchema.DataSchemaName.Contains(".xml")) currSchema.DataSchemaName = currSchema.DataSchemaName + ".xml";
            txtStatus.Text = "Data Schema Name is: " + dataSchemaName;

            // Process save file to dialog filename 
            currSchema.save();
                
            txtStatus.Text = "Saved File name is " + currSchema.DataSchemaName;
        }

        private string getDataFileName(string currFileName )
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = currFileName; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "XML documents (.xml)|*.xml"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            return dlg.FileName;
        }

        private void treeDataModel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string fieldValue = "";
            string thisFieldName = ""; 
            string[] fieldItems;
            string[] fieldSubItems = {" "};
            string thisFieldAttr = "";
            if (treeDataModel.SelectedValue != null)
            {
                fieldValue = ((TreeViewItem)treeDataModel.SelectedItem).Header.ToString();
                if (fieldValue.Contains('|'))
                {
                   gridDataObject.Visibility = System.Windows.Visibility.Visible;
                   LabelEditFunction.Content = "Editing...";

                   fieldItems = fieldValue.Split('|');
                   if (fieldItems.Length > 1)
                   {
                      thisFieldName = fieldItems[1].Trim();
                      txtFieldName.Text = thisFieldName;
                      txtStatus.Text = " FieldName -> " + thisFieldName;
                   }

                   TreeViewItem thisTVItem = treeDataModel.SelectedItem as TreeViewItem;
                   foreach (TreeViewItem tv in thisTVItem.Items)
                   {
                      fieldValue = tv.Header.ToString();
                      fieldItems = fieldValue.Split(':');
                      if (fieldItems.Length > 1)
                      {
                        thisFieldAttr = fieldItems[1].Trim();
                      }

                        switch (fieldItems[0])
                        {
                            case "Field Name":
                                txtFieldName.Text = thisFieldName;
                                txtFieldName.Resources.Remove("Orig");
                                txtFieldName.Resources.Add("Orig", thisFieldAttr);
                                break;
                            case "Field Number":
                                txtFieldNumber.Text = thisFieldAttr;
                                txtFieldNumber.Resources.Remove("Orig");
                                txtFieldNumber.Resources.Add("Orig", thisFieldAttr);
                                break;
                            case "Field Label":
                                txtFieldLabel.Text = thisFieldAttr;
                                txtFieldLabel.Resources.Remove("Orig");
                                txtFieldLabel.Resources.Add("Orig", thisFieldAttr);
                                break;
                            case "Field Descr":
                                txtFieldDescr.Text = thisFieldAttr;
                                txtFieldDescr.Resources.Remove("Orig");
                                txtFieldDescr.Resources.Add("Orig", thisFieldAttr);
                                break;
                            case "Table Name":
                                txtTableName.Text = thisFieldAttr;
                                txtFieldName.Resources.Remove("Orig");
                                txtFieldName.Resources.Add("Orig", thisFieldAttr);
                                break;
                            case "Data Type":
                                cmbDataType.SelectedItem = thisFieldAttr;
                                cmbDataType.SelectedIndex = cmbDataType.Items.IndexOf(thisFieldAttr.ToUpper());
                                cmbDataType.Resources.Remove("Orig");
                                cmbDataType.Resources.Add("Orig", thisFieldAttr);
                                break;
                        }
                    }
                }
            }
        }

        private void Rules_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (globalDataChanged)
            {
               MessageBoxResult mbr = MessageBox.Show("Data Schema has changed -> Save Changes? ", "Data Changed!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);
               if (mbr == MessageBoxResult.Cancel)
               {
                   e.Cancel = true;
               }
               if (mbr == MessageBoxResult.Yes)
               {
                   RoutedEventArgs e2 = new RoutedEventArgs();
                   btnSaveDataSchema_Click(sender,  e2); 
               }
            }
        }

        private void btnLoadConditions_Click(object sender, RoutedEventArgs e)
        {
            globalConditionsChanged = false;
            string xmlcontents = "";
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
                ruleConditionName = dlg.FileName;
                ruleConditionSet.ConditionName = ruleConditionName;
            }

            // Get the File and Load it to XML Doc
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(ruleConditionName);
                xmlcontents = doc.InnerXml;
                txtStatus.Text = "File: " + ruleConditionName + " loaded.";
                txtConditionSetName.Text = ruleConditionName;
            }
            catch (Exception ex)
            {
                txtStatus.Text = "Error loading file: " + ruleConditionName;
            }

            // Convert the XML Doc to the Class
            ruleConditionSet = (RuleConditionSet)XMLUtility.Deserialize<RuleConditionSet>(xmlcontents);

            // Display the Class in the Tree View 
            updateTreeViewConditions();

           txtStatus.Text = "Max is '" + idCounter.getNextRuleConditionID().ToString() + "'"; 

        }

        private void updateTreeViewConditions()
        {
            treeRules.Items.Clear(); 

            if (ruleConditionSet != null && ruleConditionSet.RuleConditions != null)
            {
                foreach (RuleCondition thisRC in ruleConditionSet.RuleConditions)
                {
                    TreeViewItem displayValue = new TreeViewItem() { Header = thisRC.DisplayValue };
                    TreeViewItem conditionConditionID = new TreeViewItem() { Header = "Condition ID: " + thisRC.RuleConditionID };
                    TreeViewItem conditionFieldNumber = new TreeViewItem() { Header = "Field Number: " + thisRC.FieldNumber };
                    TreeViewItem conditionOperatorList = new TreeViewItem() { Header = "Operators: " + thisRC.OperatorList };
                    TreeViewItem conditionSelectorList = new TreeViewItem() { Header = "Selectors: " + thisRC.SelectorList };
                    TreeViewItem conditionValueList = new TreeViewItem() { Header = "Value List: " + thisRC.ValueList };

                    displayValue.Items.Add(conditionConditionID);
                    displayValue.Items.Add(conditionFieldNumber);
                    displayValue.Items.Add(conditionOperatorList);
                    displayValue.Items.Add(conditionSelectorList);
                    displayValue.Items.Add(conditionValueList);
                    //displayValue.Items.Add(conditionWhich);

                    treeRules.Items.Add(displayValue);
                    idCounter.setHighRuleConditionID(Convert.ToInt32(thisRC.RuleConditionID));
                }
            }
        }

        private void btnNewCondition_Click(object sender, RoutedEventArgs e)
        {
            fillConditionCombos();
            int nextRuleConditionID = idCounter.getNextRuleConditionID();
            txtRuleConditionID.Text = nextRuleConditionID.ToString();
            txtStatus.Text = "This RuleConditionID is: '" + nextRuleConditionID.ToString() + "' While the screen value is showing: '" + txtRuleConditionID.Text + "' ";
            txtRuleConditionID.Text = txtRuleConditionID.Text + "***"; 
            gridConditions.Visibility = System.Windows.Visibility.Visible;
            txtConditionField_Orig.Visibility = System.Windows.Visibility.Hidden;
            labelConditionEditing.Content = "New Rule Condition...";

            // -- Clear fields: 
            Visual thisGrid = (Visual) gridConditions;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(thisGrid); i++)
            {
                // Retrieve child visual at specified index value.
                Visual childVisual = (Visual)VisualTreeHelper.GetChild(thisGrid, i);

                // Do processing of the child visual object.
                if (childVisual is TextBox)
                {
                    TextBox tb = childVisual as TextBox;
                    tb.Text = "";
                }
                if (childVisual is ComboBox)
                {
                    ComboBox cb = childVisual as ComboBox;
                    cb.SelectedIndex = -1;
                }

            }

        }

        private void fillConditionCombos()
        {
            if (cmbOperatorType.Items.Count == 0)
            {
                foreach (string thisValue in operTypes)
                {
                    cmbOperatorType.Items.Add(thisValue);
                }
            }
            if (cmbOperatorType.Items.Count == 0)
            {
                foreach (string thisValue in operTypes)
                {
                    cmbOperatorType.Items.Add(thisValue);
                }
            }
            if (cmbConditionField.Items.Count == 0)
            {
                foreach (DataObject thisValue in currSchema.DataObjects)
                {
                    cmbConditionField.Items.Add(thisValue.FieldNumber + " | " + thisValue.FieldName);
                }
            }

        }

        private void btnOkCondition_Click(object sender, RoutedEventArgs e)
        {
            if (labelConditionEditing.Content.ToString().StartsWith("Edit"))
            {
                if (ruleConditionSet.RuleConditions == null)
                {
                    ruleConditionSet.RuleConditions = new List<RuleCondition>(); 
                }
                RuleCondition thisRC = ruleConditionSet.RuleConditions.Find(RuleCondition => RuleCondition.RuleConditionID == txtRuleConditionID.Text.Trim());
                if (thisRC == null)
                    txtStatus.Text = "Cannot get field: " + txtConditionField_Orig.Text;
                else 
                {
                    thisRC.FieldNumber = Convert.ToInt32(cmbConditionField.SelectedItem.ToString().Substring(0,cmbConditionField.SelectedItem.ToString().IndexOf('|')).Trim()) ;
                    thisRC.OperatorList = cmbOperatorType.SelectedItem.ToString();
                    thisRC.SelectorList = txtSelector.Text;
                    thisRC.DisplayValue = txtDisplayValue.Text;
                    thisRC.ValueList = txtValueList.Text;
                    txtStatus.Text = " ";
                    globalDataChanged = origTextChanged(cmbConditionField, thisRC.FieldNumber.ToString());
                    globalDataChanged = origTextChanged(txtSelector, txtSelector.Text);
                    globalDataChanged = origTextChanged(cmbOperatorType, cmbOperatorType.SelectedValue.ToString());
                    globalDataChanged = origTextChanged(txtDisplayValue, txtDisplayValue.Text);
                    globalDataChanged = origTextChanged(txtValueList, txtValueList.Text);
                }
            }
            else //---- New Item ----
            {
                if (ruleConditionSet.RuleConditions == null)
                {
                    ruleConditionSet.RuleConditions = new List<RuleCondition>();
                }
                globalDataChanged = true;
                RuleCondition newRC = new RuleCondition();
                newRC.FieldNumber = Convert.ToInt32(cmbConditionField.SelectedItem.ToString().Substring(0, cmbConditionField.SelectedItem.ToString().IndexOf('|')).Trim());
                newRC.OperatorList = cmbOperatorType.SelectedItem.ToString();
                newRC.SelectorList = txtSelector.Text;
                newRC.DisplayValue = txtDisplayValue.Text;
                newRC.ValueList = txtValueList.Text;
                newRC.RuleConditionID = txtRuleConditionID.Text;

                ruleConditionSet.RuleConditions.Add(newRC);
            }

            updateTreeViewConditions();
            gridConditions.Visibility = System.Windows.Visibility.Hidden;

        }

        private void btnSaveCondition_Click(object sender, RoutedEventArgs e)
        {
            string newFileName = "";

            if (ruleConditionSet.ConditionName == null) ruleConditionSet.ConditionName = "*";
            newFileName = getDataFileName(ruleConditionSet.ConditionName);

            ruleConditionName = newFileName;
            ruleConditionSet.ConditionName = ruleConditionName;
            if (!ruleConditionSet.ConditionName.Contains(".xml")) ruleConditionSet.ConditionName = ruleConditionSet.ConditionName + ".xml";
            txtStatus.Text = "Rule Condition Set Name is: " + ruleConditionName;

            // Process save file to dialog filename 
            ruleConditionSet.save();
            txtStatus.Text = "Saved File name is " + ruleConditionSet.ConditionName;
        }

        private void btnCancelCondtion_Click(object sender, RoutedEventArgs e)
        {
            gridConditions.Visibility = System.Windows.Visibility.Hidden;
        }

        private void treeRules_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            txtStatus.Text = " ";
            string displayValue = "";
            string thisFieldName = "";
            string[] fieldItems;
            string[] fieldSubItems = { " " };
            string thisFieldAttr = "";

            if (cmbConditionField.Items.Count == 0 )
                fillConditionCombos();

            if (treeRules.SelectedValue != null)
            {
                displayValue = treeRules.SelectedValue.ToString().Substring(44);
                fieldItems = displayValue.Split(':');
                if (fieldItems.Length > 1) fieldSubItems = fieldItems[1].Split(' ');

              //  txtStatus.Text = "fieldItems: '" + fieldItems[0] + "' , '" + fieldItems[1] + "'" ; 

                if (fieldSubItems.Length > 1) txtStatus.Text = txtStatus.Text + " - " + fieldItems[0] + " / " + fieldSubItems[1];
                else
                {
                    // -- Main Field Name Node -- 
                    displayValue = fieldItems[0].Substring(0, fieldItems[0].IndexOf("Items.Count"));
                    //txtStatus.Text = txtStatus.Text + " Display Value -> " + displayValue;
                    TreeViewItem thisTVItem = treeRules.SelectedItem as TreeViewItem;
                    foreach (TreeViewItem tv in thisTVItem.Items)
                    {

                        thisFieldName = tv.ToString().Substring(44);
                        //txtStatus.Text = "Split Tree View Item: " + thisFieldName;
                        fieldItems = thisFieldName.Split(':');
                        if (fieldItems.Length > 1) thisFieldAttr = fieldItems[1].Substring(0, fieldItems[1].IndexOf("Items.Count")).Trim();

                        gridConditions.Visibility = System.Windows.Visibility.Visible;

                        labelConditionEditing.Content = "Editing Rule Condition...";
                        txtDisplayValue.Text = displayValue;
                        txtDisplayValue.Resources.Remove("Orig");
                        txtDisplayValue.Resources.Add("Orig", displayValue);
                        //txtStatus.Text = txtStatus.Text + " / thisFieldAttr = " + thisFieldAttr; 

                        switch (fieldItems[0])
                        {
                            case "Field Number":
                                foreach (string thisField in cmbConditionField.Items)
                                {
                                    if (thisField.StartsWith(thisFieldAttr.Trim()))
                                    {
                                        cmbConditionField.SelectedItem = thisField;
                                        txtConditionField_Orig.Text = thisFieldAttr.Trim();
                                    }
                                }
                                break;
                            case "Operators":
                                cmbOperatorType.SelectedItem = thisFieldAttr;
                                cmbOperatorType.Resources.Remove("Orig");
                                cmbOperatorType.Resources.Add("Orig", thisFieldAttr);
                                break;
                            case "Selectors":
                                txtSelector.Text = thisFieldAttr;
                                txtSelector.Resources.Remove("Orig");
                                txtSelector.Resources.Add("Orig", thisFieldAttr);
                                break;
                            case "Value List":
                                txtValueList.Text = thisFieldAttr;
                                txtValueList.Resources.Remove("Orig"); 
                                txtValueList.Resources.Add("Orig", thisFieldAttr);
                                break;
                            case "Condition ID":
                                txtRuleConditionID.Text = thisFieldAttr;
                                //txtStatus.Text = " / thisConditionID = '" + txtRuleConditionID.Text + "'" ; 
                                break;
                        }
                    }
                }
            }

        }


        private void btnOKRuleMaster_Click(object sender, RoutedEventArgs e)
        {
            // Hide the Rule Master Grid and refresh the RuleMaster tree view
            GridRuleMaster.Visibility = System.Windows.Visibility.Hidden;
            updateTreeRuleMaster(); 
        }

        private void btnNewRuleMaster_Click(object sender, RoutedEventArgs e)
        {
            GridRuleMaster.Visibility = System.Windows.Visibility.Visible;
            LabelNewCondition.Visibility = System.Windows.Visibility.Hidden;
            btnAddNewCondition.Visibility = System.Windows.Visibility.Hidden;
            cmbNewWhich.Visibility = System.Windows.Visibility.Hidden;
            cmbNewCondition.Visibility = System.Windows.Visibility.Hidden;
            cmbNewOperator.Visibility = System.Windows.Visibility.Hidden;
            txtNewRuleValue.Visibility = System.Windows.Visibility.Hidden;
            btnAddConditionCompleted.Visibility = System.Windows.Visibility.Hidden;
            txtConditionsDataSchema.Text = txtDataSchemaName.Text;
            int newRuleMasterID = idCounter.getNextRuleMasterID();

            //currentRuleMaster = new RuleMaster();
            //currentRuleMaster.RuleDetails = new List<RuleDetail>(); 

            txtRuleMasterID.Text = newRuleMasterID.ToString();
            txtStatus.Text = "Rule Master ID = " + newRuleMasterID.ToString(); 
            if (cmbNewCondition.Items.Count == 0)
            {
                foreach (RuleCondition thisCondition in ruleConditionSet.RuleConditions)
                {
                    cmbNewCondition.Items.Add(thisCondition.FieldNumber + "|" + thisCondition.DisplayValue);
                }
            }

            if (cmbNewWhich.Items.Count == 0)
            {
                foreach (string thisValue in whichTypes)
                {
                    cmbNewWhich.Items.Add(thisValue);
                }
            }
        }

        private void cmbNewCondition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmbNewOperator.Items.Clear();

            foreach (RuleCondition thisCondition in ruleConditionSet.RuleConditions)
            {

                if (cmbNewCondition.SelectedItem.ToString().StartsWith(thisCondition.FieldNumber + "|" ))
                {
                    switch (thisCondition.OperatorList)
                    {
                        case "CHAR":
                            foreach (string thisOper in charOperators)
                            {
                                cmbNewOperator.Items.Add(thisOper);
                            }
                            break;
                        case "DATE":
                            foreach (string thisOper in dateOperators)
                            {
                                cmbNewOperator.Items.Add(thisOper);
                            }
                            break;
                        case "NUMERIC":
                            foreach (string thisOper in numericOperators)
                            {
                                cmbNewOperator.Items.Add(thisOper);
                            }
                            break;
                        case "BOOL":
                            foreach (string thisOper in boolOperators)
                            {
                                cmbNewOperator.Items.Add(thisOper);
                            }
                            break;
                    }
                }
            }
        }

        private void btnAddNewCondition_Click(object sender, RoutedEventArgs e)
        {
            btnAddNewCondition.Visibility = System.Windows.Visibility.Hidden;
            LabelNewCondition.Visibility = System.Windows.Visibility.Visible;
            cmbNewWhich.Visibility = System.Windows.Visibility.Visible;
            cmbNewCondition.Visibility = System.Windows.Visibility.Visible;
            cmbNewOperator.Visibility = System.Windows.Visibility.Visible;
            txtNewRuleValue.Visibility = System.Windows.Visibility.Visible;
            btnAddConditionCompleted.Visibility = System.Windows.Visibility.Visible;

            workingCondition = new RuleCondition(); 
        }

        private void btnCancelRuleMaster_Click(object sender, RoutedEventArgs e)
        {
            // Hide Rule Master Grid and Refresh the RuleMaster treeview
            GridRuleMaster.Visibility = System.Windows.Visibility.Hidden;
            updateTreeRuleMaster(); 

        }

        private void btnAddConditionComplete_Click(object sender, RoutedEventArgs e)
        {
            string[] strArray; 

            // Insert the values into the classes
            RuleDetail newRuleDetail = new RuleDetail();
            strArray = cmbNewCondition.SelectedValue.ToString().Split('|'); 

            newRuleDetail.RuleConditionID = strArray[0];
            newRuleDetail.ConditionOperator = cmbNewOperator.SelectedValue.ToString();
            newRuleDetail.ConditionValue = txtValueList.Text;
            newRuleDetail.WhichType = cmbNewWhich.SelectedValue.ToString();

            RuleMaster thisRM = ruleMasterList.ruleMasters.Find(rm => rm.RuleMasterID == txtRuleMasterID.Text);
            if (thisRM != null)
            {
                // Add a new RuleDetail
                if (thisRM.RuleDetails == null)
                    thisRM.RuleDetails = new List<RuleDetail>(); 
                int newRuleDetailID = idCounter.getNextRuleDetailID();
                newRuleDetail.RuleDetailID = newRuleDetailID.ToString(); 
                if (btnAddConditionCompleted.Content.ToString() == "Add")
                {
                    thisRM.RuleDetails.Add(newRuleDetail);
                }
                else
                {
                    RuleDetail changeDetail = thisRM.RuleDetails.Find(ch => ch.RuleDetailID == txtRuleDetailID.Text);
                    if (changeDetail != null)
                    {
                        changeDetail.RuleConditionID = newRuleDetail.RuleConditionID;
                        changeDetail.ConditionOperator = newRuleDetail.ConditionOperator;
                        changeDetail.ConditionValue = newRuleDetail.ConditionValue;
                        changeDetail.WhichType = newRuleDetail.WhichType;
                    }
 
                }
            }

        }

        private void updateTreeRuleMaster()
        {
            treeRuleMasters.Items.Clear();

            txtStatus.Text = "ruleMasterList.Count = " + ruleMasterList.ruleMasters.Count;

            if (ruleMasterList.ruleMasters.Count > 0)
            {
                foreach (RuleMaster thisRM in ruleMasterList.ruleMasters)
                {
                    TreeViewItem ruleMasterItem = new TreeViewItem() { Header = thisRM.RuleMasterID + "| " + thisRM.RuleName };
                    TreeViewItem dataFieldNumber = new TreeViewItem() { Header = "Owner: " + ruleMasterList.RuleListOwner };
                    TreeViewItem dataFieldLabel = new TreeViewItem() { Header = "Active Dates: " + thisRM.DateBegin + " to " + thisRM.DateEnd };
                    TreeViewItem dataFieldDescr = new TreeViewItem() { Header = "Description: " + thisRM.RuleDescr};
                    
                    ruleMasterItem.Items.Add(dataFieldNumber);
                    ruleMasterItem.Items.Add(dataFieldLabel);
                    ruleMasterItem.Items.Add(dataFieldDescr);
                    idCounter.setHighRuleMasterID(Convert.ToInt32(thisRM.RuleMasterID));

                    if (thisRM.RuleDetails!= null && thisRM.RuleDetails.Count > 0)
                    {
                        foreach (RuleDetail thisDetail in thisRM.RuleDetails)
                        {
                            StringBuilder strRuleText = new StringBuilder();

                            strRuleText.Append(whichText(thisDetail.WhichType) + " " + currSchema.getFieldName(ruleConditionSet.getFieldNumber(thisDetail.RuleConditionID)));
                            strRuleText.Append(" " + thisDetail.ConditionOperator + " " + thisDetail.ConditionValue);
                            TreeViewItem ruleDetailItem = new TreeViewItem() { Header = strRuleText.ToString() };
                            ruleMasterItem.Items.Add(ruleDetailItem);
                        }
                    }

                   treeRuleMasters.Items.Add(ruleMasterItem);
                }
            }
        }

        private void tabRuleMasters_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ruleMasterList == null)
            {
                ruleMasterList = new RuleMasterList();
                ruleMasterList.ruleMasters = new List<RuleMaster>();
            }
               

            if (ruleMasterList.ruleMasters != null && ruleMasterList.ruleMasters.Count > 0)
            {
                //-- set the treeview 
                updateTreeRuleMaster();
            }
        }


        string whichText(string whichWord)
        {
            if (whichWord == "FIRST")
                return "The first time ";
            if (whichWord == "FIRSTN")
                return "The first 5 times " ;
            if (whichWord == "EVERY")
                return "Every time the " ;
            return ""; 
        }

        private void btnSaveRuleMaster1_Click(object sender, RoutedEventArgs e)
        {
            if (btnSaveRuleMaster.Content.ToString().StartsWith("Edit"))
            {
                txtRuleMasterName.IsEnabled = true;
                //txtRuleMasterOwner.IsEnabled = true;
                txtRuleMasterDateBegin.IsEnabled = true;
                txtRuleMasterDateEnd.IsEnabled = true;
                txtRuleMasterDescr.IsEnabled = true;
                btnSaveRuleMaster.Content = "Save Rule Master";
            }
            else
            {
                currentRuleMaster = new RuleMaster();
                currentRuleMaster.DateBegin = txtRuleMasterDateBegin.Text;
                currentRuleMaster.DateEnd = txtRuleMasterDateEnd.Text;
                currentRuleMaster.RuleDescr = txtRuleMasterDescr.Text;
                currentRuleMaster.RuleMasterID = txtRuleMasterID.Text;
                currentRuleMaster.RuleName = txtRuleMasterName.Text;
                ruleMasterList.ruleMasters.Add(currentRuleMaster);
                updateTreeRuleMaster();
                btnAddNewCondition.Visibility = System.Windows.Visibility.Visible;
                txtRuleMasterName.IsEnabled = false;
                //txtRuleMasterOwner.IsEnabled = false;
                txtRuleMasterDateBegin.IsEnabled = false;
                txtRuleMasterDateEnd.IsEnabled = false;
                txtRuleMasterDescr.IsEnabled = false;
                btnSaveRuleMaster.Content = "Edit Rule Master";
            }

        }

        private void btnSaveMyRules_Click(object sender, RoutedEventArgs e)
        {
            string newFileName = "";
            if (ruleMasterList.RuleListFileName == null) ruleMasterList.RuleListFileName = "*";
            newFileName = getDataFileName(ruleMasterList.RuleListFileName);

            ruleMasterList.RuleListFileName = newFileName;
            if (!ruleMasterList.RuleListFileName.Contains(".xml")) ruleMasterList.RuleListFileName = ruleMasterList.RuleListFileName + ".xml";
            txtStatus.Text = "Rule List File Name is: " + ruleMasterList.RuleListFileName;

            // Process save file to dialog filename 
            ruleMasterList.save(); 
            txtStatus.Text = "Saved File name is " + ruleMasterList.RuleListFileName;
        }

        private void btnLoadRuleMaster_Click(object sender, RoutedEventArgs e)
        {
            globalConditionsChanged = false;
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
                txtStatus.Text = ruleMasterList.load(dlg.FileName);
            }
            updateTreeRuleMaster(); 
        }

        private void treeRuleMasters_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            txtStatus.Text = " ";
            string tempValue = "";
            string ruleMasterID = "";
            string ruleMasterName = "";
            string thisFieldName = "";
            string tmpBegDate = "";
            string tmpEndDate = ""; 

            string[] fieldItems;
            string[] fieldSubItems = { " " };
            string thisFieldAttr = "";

            if (treeRuleMasters.SelectedValue != null)
            {
                // If not clicked on a top level, then ignore the click 
                if (treeRuleMasters.SelectedValue.ToString().Contains('|'))
                {
                    tempValue = treeRuleMasters.SelectedValue.ToString().Substring(44);
                    fieldItems = tempValue.Split('|');
                    if (fieldItems.Length > 1)
                    {
                        ruleMasterID = fieldItems[0];
                        ruleMasterName = fieldItems[1];
                    }

                    GridRuleMaster.Visibility = System.Windows.Visibility.Visible;
                    labelRuleMasterEditing.Content = "Editing Rule Master...";
                    txtRuleMasterID.Text = ruleMasterID;
                    txtRuleMasterName.Text = ruleMasterName;
                    txtRuleMasterName.Resources.Remove("Orig");
                    txtRuleMasterName.Resources.Add("Orig", ruleMasterName);
                    btnSaveRuleMaster.Content = "Edit Rule Master";
                    txtRuleMasterName.IsEnabled = false; 

                    
                    TreeViewItem thisTVItem = treeRuleMasters.SelectedValue as TreeViewItem;
                    foreach (TreeViewItem tv in thisTVItem.Items)
                    {
                        if (tv.Header.ToString().Contains(':'))
                        {
                            thisFieldName = tv.Header.ToString();
                            fieldItems = thisFieldName.Split(':');
                            thisFieldAttr = fieldItems[1].Trim();
                            // Owner - from, to dates, Description 

                            switch (fieldItems[0])
                            {
                                case "Owner":
                                    //txtRuleMasterOwner.Text = thisFieldAttr.Trim();
                                    //txtRuleMasterOwner.Resources.Remove("Orig");
                                    //txtRuleMasterOwner.Resources.Add("Orig", thisFieldAttr);
                                    //txtRuleMasterOwner.IsEnabled = false; 
                                    break;
                                case "Active Dates":
                                    tmpBegDate = thisFieldAttr.Substring(0, thisFieldAttr.IndexOf(" to "));
                                    tmpEndDate = thisFieldAttr.Substring(tmpBegDate.Length + 4);
                                    txtRuleMasterDateBegin.Text = tmpBegDate;
                                    txtRuleMasterDateBegin.Text = tmpBegDate;
                                    txtRuleMasterDateBegin.Resources.Remove("Orig");
                                    txtRuleMasterDateBegin.Resources.Add("Orig", tmpBegDate);
                                    txtRuleMasterDateBegin.IsEnabled = false;
                                    txtRuleMasterDateEnd.Text = tmpEndDate;
                                    txtRuleMasterDateEnd.Text = tmpEndDate;
                                    txtRuleMasterDateEnd.Resources.Remove("Orig");
                                    txtRuleMasterDateEnd.Resources.Add("Orig", tmpEndDate);
                                    txtRuleMasterDateEnd.IsEnabled = false;
                                    break;
                                case "Description":
                                    txtRuleMasterDescr.Text = thisFieldAttr;
                                    txtRuleMasterDescr.Resources.Remove("Orig");
                                    txtRuleMasterDescr.Resources.Add("Orig", thisFieldAttr);
                                    txtRuleMasterDescr.IsEnabled = false;
                                    break;
                            }
                        }
                    }
                    
                }
            }

        }
 
    }
}
