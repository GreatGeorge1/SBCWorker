using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol
{
    public class Method
    {
        public CommandHeader CommandHeader { get; set; }
        public bool HasResponseValue { get; set; }
        public bool HasCommandValue { get; set; }
        public  Direction DirectionTo  { get; set; }
    }

    [AttributeUsage(AttributeTargets.Enum)]
    public class ByteAttribute : Attribute
    {
        public int ByteValue { get; private set; }
        public ByteAttribute(int value) 
        {
            ByteValue = value;
        }
    }

    public enum Direction
    {
        NotSet,
        Terminal,
        Controller
    }
}
