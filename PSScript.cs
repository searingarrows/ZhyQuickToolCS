using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ZhyQuickToolCS
{
    internal class PSScript : Script
    {
        private readonly string content;
        private PowerShell powerShell;
        private PSScript(string name, string content) : base(name)
        {
            this.content = content;
            powerShell = PowerShell.Create();
            powerShell.AddScript(Content);
        }
        public string? Content
        {
            get { return content; }
        }
        static public async Task<PSScript?> Load(StorageFile file)
        {
            if (file == null)
            {
                return null;
            }
            string text;
            try
            {
                text = await FileIO.ReadTextAsync(file);
            }
            catch (Exception)
            {
                return null;
            }
            return new PSScript(file.DisplayName, text);
        }


        public override void Execute()
        {
            powerShell.Invoke();
        }

    }
}
