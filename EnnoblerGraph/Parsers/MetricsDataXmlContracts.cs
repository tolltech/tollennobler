using System;
using System.Xml.Serialization;

namespace Tolltech.EnnoblerGraph.Parsers
{
    public class CodeMetricsReport
    {
        public CodeMetricsReport()
        {
            Targets = Array.Empty<Target>();
        }

        public Target[] Targets { get; set; }
    }

    public class Target
    {
        [XmlAttribute]
        public string Name { get; set; }

        public Assembly Assembly { get; set; }
    }

    public class Assembly
    {
        public Assembly()
        {
            Namespaces = Array.Empty<Namespace>();
        }

        [XmlAttribute]
        public string Name { get; set; }

        public Metric[] Metrics { get; set; }

        public Namespace[] Namespaces { get; set; }
    }

    public class Namespace
    {
        public Namespace()
        {
            Types = Array.Empty<NamedType>();
        }

        [XmlAttribute]
        public string Name { get; set; }

        public Metric[] Metrics { get; set; }

        public NamedType[] Types { get; set; }
    }

    public class NamedType
    {
        public NamedType()
        {
            Members = Array.Empty<Member>();
        }

        [XmlAttribute]
        public string Name { get; set; }

        public Metric[] Metrics { get; set; }

        [XmlArrayItem(typeof(Method))]
        [XmlArrayItem(typeof(Field))]
        [XmlArrayItem(typeof(Property))]
        public Member[] Members { get; set; }
    }

    public abstract class Member
    {
        [XmlAttribute]
        public string Name { get; set; }

        public Metric[] Metrics { get; set; }
    }

    public class Method : Member
    {
        
    }

    public class Field : Member
    {
        
    }
    
    public class Property : Member
    {
        
    }

    public class Metric
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public int Value { get; set; }
    }
}