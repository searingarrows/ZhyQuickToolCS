using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZhyQuickToolCS
{
    public abstract class Script
    {
        private readonly string name;
        public string Name { get { return name; } }
        public Script(string name) { this.name = name; }

        public abstract void Execute();
    }
}
