using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInTheShell.Modules.Script
{
    public interface IScriptService
    {

    }

    public interface IScriptFilterProcessor
    {
        string Process(string script);
    }
}
