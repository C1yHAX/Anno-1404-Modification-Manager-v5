using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevelopmentTools.Tools.XPath.Classes
{
    public class XPathItem
    {
        public string Axis { get; set; }
        public string NodeTest { get; set; }
        public List<XPathPredicate> Predicates { get; set; }

        public XPathItem()
        {
            Predicates = new List<XPathPredicate>();
        }

        public string ToXPathExpression(Dictionary<string, string> PredicateValueAssigns)
        {
            string output = Axis;
            if (!string.IsNullOrEmpty(NodeTest))
                output += "::" + NodeTest;

            return output;
        }
    }
}
