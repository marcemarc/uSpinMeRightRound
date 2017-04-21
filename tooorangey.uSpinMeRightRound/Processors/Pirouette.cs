using ImageProcessor.Web.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImageProcessor.Processors;
using System.Collections.Specialized;
using ImageProcessor.Web.Helpers;
using ImageProcessor.Imaging;
using System.Web;
using Umbraco.Core;

namespace tooorangey.uSpinMeRightRound.Processors
{
    /// <summary>
    /// rotate but only in blocks of 90 degrees
    /// so this can be enabled on a querystring
    /// and not be exploited like rotate degrees could potentially be
    /// </summary>
    public class Pirouette : IWebGraphicsProcessor
    {
        /// The regular expression to search strings for.
        /// </summary>
        private static readonly Regex QueryRegex = new Regex(@"pirouette=[1|2|3]", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="Rotate"/> class.
        /// </summary>
        public Pirouette()
        {
            this.Processor = new ImageProcessor.Processors.Rotate();
        }

        /// <summary>
        /// Gets the regular expression to search strings for.
        /// </summary>
        public Regex RegexPattern => QueryRegex;

        /// <summary>
        /// Gets the order in which this processor is to be used in a chain.
        /// </summary>
        public int SortOrder
        {
            get;
            private set;
        }

        public ImageProcessor.Processors.Rotate Processor { get; }

        IGraphicsProcessor IWebGraphicsProcessor.Processor
        {
            get
            {
                return this.Processor;
            }
        }

        /// <summary>
        /// The position in the original string where the first character of the captured substring was found.
        /// </summary>
        /// <param name="queryString">
        /// The query string to search.
        /// </param>
        /// <returns>
        /// The zero-based starting position in the original string where the captured substring was found.
        /// </returns>
        public int MatchRegexIndex(string queryString)
        {
            this.SortOrder = int.MaxValue;
            Match match = this.RegexPattern.Match(queryString);

            if (match.Success)
            {
                this.SortOrder = match.Index;
                NameValueCollection queryCollection = HttpUtility.ParseQueryString(queryString);
                //but we don't allow any number here!
                //just a number of pirouette turns, should have called this turns, as pirouette means one full rotation shhsh, don't tell anyone
                // we are expecting either 1, 2 or 3 quarter turns on the querystring
                // need to pass these into underlying image processor rotate as float values
                // assume 90 degrees
                var quarterTurns = QueryParamParser.Instance.ParseValue<int>(queryCollection["pirouette"]);
                //make sure versions not created for other numbers 
                switch (quarterTurns)
                {
                    case 1:
                    case 2:
                    case 3:
                        var rotateDegrees = (float)(quarterTurns * 90);
                        this.Processor.DynamicParameter = rotateDegrees;
                        break;

                    default:
                        //do no rotatings
                        break;
                }
            }
            return this.SortOrder;
        }
    }
}
