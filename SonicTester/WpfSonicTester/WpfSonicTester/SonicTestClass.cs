using System;
using System.Collections.Generic;
using System.Xml;
using XmlSerialize;

namespace WpfSonicTester
{
    [Serializable]
    public class SonicTestClass
    {
        public List<SonicTest> TestCollection;
        public string SonicTestFileName { get; set; }

        public void save()
        {
            string xmlString = XMLUtility.Serialize<SonicTestClass>(this);
            System.IO.File.WriteAllText(this.SonicTestFileName, xmlString);
        }

        public string load(string loadFileName)
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
                SonicTestClass stTemp = new SonicTestClass();
                stTemp = (SonicTestClass)XMLUtility.Deserialize<SonicTestClass>(xmlContents);

                this.TestCollection = stTemp.TestCollection;
                this.SonicTestFileName = loadFileName;
                returnVal = "Records Loaded = " + this.TestCollection.Count.ToString();
            }

            return returnVal;
        }




    }
    public class SonicTest
    {
        public string testName { get; set; }
        public string testXML { get; set; }

    }
}
