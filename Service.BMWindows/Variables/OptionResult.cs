using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.BMWindows.Variables
{
    public class OptionResult
    {
        public int? Page { get; set; }
        public int? Limit { get; set; }
        public bool Unlimited { get; set; }
        public bool? HasCount { get; set; }
        public string OrderType { get; set; }
        public string OrderBy { get; set; }
        public int? Skip { get; set; }
    }
}
