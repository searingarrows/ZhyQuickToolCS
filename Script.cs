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
        private readonly string displayName;
        public string Name { get => name; }
        public string DisplayName { get => displayName; }
        public Script(string name)
        {
            this.name = name;
            for (int i = 0; i < name.Length; i++)
            {
                if (name[i] == '_')
                {
                    this.displayName = name.Substring(i + 1);
                    return;
                }
            }
            this.displayName = name;
        }

        public abstract void Execute();
    }
}
