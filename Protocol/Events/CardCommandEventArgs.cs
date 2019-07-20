using System;
using System.Collections.Generic;
using System.Text;

namespace Protocol.Events
{
    public class CardCommandEventArgs
    {
        public CardCommandEventArgs(string card, string md5Hash)
        {
            Card = card;
            Md5Hash = md5Hash;
        }

        public readonly string Card;
        public readonly string Md5Hash;
    }
}
