using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol
{
    public class Method
    {
        public CommandHeader CommandHeader { get; set; }
        public virtual List<ResponseHeader> ResponseHeaders { get; set; }
        public bool HasResponseHeader { get; set; }
        public bool HasResponseValue { get; set; }
        public bool HasCommandValue { get; set; }
        public bool HasCheckSum { get; set; } = true;
        public  Direction DirectionTo  { get; set; }
    }

    public enum Direction
    {
        NotSet,
        Terminal,
        Controller
    }
}
