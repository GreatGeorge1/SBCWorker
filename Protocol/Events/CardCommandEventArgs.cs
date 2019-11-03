using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Events
{
    public class CardCommandEventArgs
    {
        public CardCommandEventArgs(byte[] card)
        {
            Card = card;
        }

        public byte[] Card { get; private set; }
    }
}
