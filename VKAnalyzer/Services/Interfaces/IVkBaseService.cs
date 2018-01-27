using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKAnalyzer.Services.Interfaces
{
    interface IVkBaseService
    {
        IEnumerable<string> ConvertstringToList(string input);
    }
}
