using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfSonicTester
{
    public class SonicResults
    {
        public List<SonicTestResult> resultCollection; 
    }
    public class SonicTestResult
    {
        public string TestName { get; set; }
        public string TestTime { get; set; }
        public string TestResult { get; set; }
    }
}
