using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AnnoModificationManager4.Misc.Serializers
{
    public class XSerializer
    {
        public XSerializer()
        {
        }

        /*public XDocument Serialize(object obj)
        {
        }

        public object Deserialize(XDocument doc)
        {
        }*/

        #region TypeSerializers
        #endregion
        #region TypeDeserializers
        private string Deserialize_String(XElement element)
        {
            return element.Nodes().OfType<XText>().First().Value;
        }

        private int Deserialize_Int(XElement element)
        {
            return int.Parse(element.Nodes().OfType<XText>().First().Value);
        }

        private int Deserialize_Label(XElement element)
        {
            return int.Parse(element.Nodes().OfType<XText>().First().Value);
        }

        private List<string> Deserialize_List__String(XElement element)
        {
            List<string> i = new List<string>();
            foreach (XElement e in element.Nodes().OfType<XElement>())
            {
                i.Add(Deserialize_String(e));
            }
            return i;
        }
        #endregion
    }
}
