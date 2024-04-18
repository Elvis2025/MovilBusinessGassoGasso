using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal
{
    public class KV
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public KV() { }

        public KV(string Key, string Value)
        {
            this.Key = Key;
            this.Value = Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
