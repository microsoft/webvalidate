using System.Collections.Generic;

namespace CSE.WebValidate.Model
{
    public class PerfTarget
    {
        public string Category { get; set; }
        public List<double> Quartiles { get; set; }
    }
}
