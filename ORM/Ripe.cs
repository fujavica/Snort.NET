using System;
using System.Collections.Generic;

namespace snortdb
{
    [Serializable]
    public class Attributes
    {
        public List<Attribute> attributes { get; set; }
    }

    [Serializable]
    public class Attribute
    {
        public Attribute(string name, string value)
        {
            this.name = name;
            this.value = value;

        }
        public string name { get; set; }
        public string value { get; set; }
    }

    public class AttributeOutput
    {
        public AttributeOutput(string name, string value)
        {
            this.names = name;
            this.values = value;

        }
        public string names { get; set; }
        public string values { get; set; }
    }


}