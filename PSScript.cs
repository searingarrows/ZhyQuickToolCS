using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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


        public override async Task<string> Execute()
        {
            string ret = "";
            var results = await powerShell.InvokeAsync();
            if (powerShell.HadErrors)
            {
                foreach (var result in powerShell.Streams.Error)
                {
                    ret += result.ToString() + Environment.NewLine;
                }
            }
            else
            {
                foreach (var result in results)
                {
                    ret += result.ToString() + Environment.NewLine;
                }
            }
            powerShell.Streams.ClearStreams();
            return ret;
        }

    }
}
