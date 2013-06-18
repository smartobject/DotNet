using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using XmlUtil;

namespace wpfRules
{

    public class IDCounter
    {
        public int NextRuleMasterID { get; private set; }
        public int NextRuleDetailID { get; private set; }
        public int NextRuleConditionID { get; private set; }
        public IDCounter()
        {
          NextRuleDetailID = 0;
          NextRuleMasterID = 0;
          NextRuleConditionID = 0;
        }

        public int getNextRuleMasterID()
        {
            NextRuleMasterID++;
            return NextRuleMasterID;
        }

        public int getNextRuleDetailID()
        {
            NextRuleDetailID++;
            return NextRuleDetailID;
        }

        public int getNextRuleConditionID()
        {
            NextRuleConditionID++;
            return NextRuleConditionID;
        }

        public void setHighRuleMasterID(int hiVal)
        {
            if (hiVal > NextRuleMasterID) NextRuleMasterID = hiVal;
        }
        public void setHighRuleDetailID(int hiVal)
        {
            if (hiVal > NextRuleDetailID) NextRuleDetailID = hiVal;
        }
        public void setHighRuleConditionID(int hiVal)
        {
            if (hiVal > NextRuleConditionID) NextRuleConditionID = hiVal;
        }
    }

    [Serializable]
    public class RuleDataSchema
    {
        public string DataSchemaName { get; set; }

        public List<DataObject> DataObjects;
        public string getFieldName(int thisFieldNum)
        {
            foreach( DataObject thisDO in DataObjects )
                if (thisDO.FieldNumber == thisFieldNum)
                {
                    return thisDO.FieldName;
                }
            return "";
        }


        public void save()
        {
            string xmlString = XMLUtility.Serialize<RuleDataSchema>(this);
            System.IO.File.WriteAllText(this.DataSchemaName, xmlString);
        }

    }

    [Serializable]
    public class DataObject
    {
        public int FieldNumber { get; set; }
        public string TableName { get; set; }
        public string FieldName {get; set;}
        public string DataType { get; set; }
        public string FieldLabel {get; set;}
        public string FieldDescr { get; set; }    
    }

    [Serializable]
    public class RuleConditionSet
    {
        public string ConditionName { get; set; }
        public List<RuleCondition> RuleConditions;

        public int getFieldNumber(string thisConditionID)
        {
            foreach (RuleCondition thisCondition in this.RuleConditions)
            {
                if (thisCondition.RuleConditionID == thisConditionID)
                    return thisCondition.FieldNumber; 
            }
            return 0; 
        }

        public void save()
        {
            string xmlString = XMLUtility.Serialize<RuleConditionSet>(this);
            System.IO.File.WriteAllText(this.ConditionName, xmlString);
        }
    }

    [Serializable]
    public class RuleCondition
    {
        public string RuleConditionID { get; set; }
        public string DisplayValue { get; set; }
        public int FieldNumber { get; set; }
        public string OperatorList { get; set; }
        public string SelectorList { get; set; }
        public string ValueList { get; set; }
    }


    [Serializable]
    public class RuleMasterList
    {

        public string DataModel { get; set; }
        public string RuleListFileName { get; set; }
        public string RuleListOwner { get; set; }

        public List<RuleMaster> ruleMasters;

        public void save()
        {
            string xmlString = XMLUtility.Serialize<RuleMasterList>(this);
            System.IO.File.WriteAllText(this.RuleListFileName, xmlString);
        }

        public string load( string loadFileName )
        {
            string returnVal = "";
            string xmlContents = ""; 
            // Get the File and Load it to XML Doc
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(loadFileName);
                xmlContents = doc.InnerXml;
            }
            catch (Exception ex)
            {
                returnVal = "Error loading file: " + loadFileName;
            }

            if (returnVal == "")
            {
                // Convert the XML Doc to the Class
                RuleMasterList rmTemp = new RuleMasterList();
                rmTemp = (RuleMasterList)XMLUtility.Deserialize<RuleMasterList>(xmlContents);

                this.DataModel = rmTemp.DataModel;
                this.RuleListFileName = loadFileName;
                this.RuleListOwner = rmTemp.RuleListOwner;
                this.ruleMasters = rmTemp.ruleMasters;
                returnVal = "Rule Masters Loaded = " + this.ruleMasters.Count().ToString(); 
            }

            return returnVal;
        }
    }

    [Serializable]
    public class RuleMaster
    {
        public string RuleMasterID { get; set; }
        public string RuleName { get; set; }
        public string RuleDescr { get; set; }
        public string DateBegin { get; set; }
        public string DateEnd { get; set; }
        public List<RuleDetail> RuleDetails ;
    }

    [Serializable]
    public class RuleDetail
    {
        public string RuleDetailID { get; set; }
        public string RuleConditionID  { get; set; }
        public string ConditionOperator { get; set; }
        public string ConditionValue { get; set; }
        public string WhichType { get; set; }
    }

    class RuleClass
    {


    }
}
