using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main
{
    internal class State
    {
        public enum Level
        {
            Default_Loading,
            Default_Main
        }
        public enum Key
        {
            Released,
            Pressed,
            Held
        }
    }
}
