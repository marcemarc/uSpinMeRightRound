using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tooorangey.uSpinMeRightRound
{
    public static class StringExtensions
    {
        //in 7.6 this is in the core as DetectIsJson
        public static bool DetectIsJsonish(this string input)
        {
            input = input.Trim();
            return (input.StartsWith("{") && input.EndsWith("}"))
                   || (input.StartsWith("[") && input.EndsWith("]"));
        }
    }
}
