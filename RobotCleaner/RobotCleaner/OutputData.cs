using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotCleaner.RobotCleaner
{
    public class OutputData
    {
        public HashSet<ResultPosition> Visited { get; set; }
        public HashSet<ResultPosition> Cleaned { get; set; }
        public Position Final { get; set; }
        public int Battery { get; set; }
    }
}
