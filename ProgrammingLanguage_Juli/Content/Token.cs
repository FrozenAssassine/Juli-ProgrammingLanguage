using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Juli_ProgLang.Content
{
    internal class Token
    {
        public Token(Enum type, object value = null)
        {
            Type = type;
            Value = value;
        }
        public object Value { get; set; }
        public Enum Type { get; set; }
    }
}
