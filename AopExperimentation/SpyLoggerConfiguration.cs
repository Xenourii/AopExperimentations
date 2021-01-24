using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AopExperimentation
{

    public sealed class SpyLoggerConfiguration
    {
        public List<string> MethodNamesToExclude { get; set; } = new List<string>();
        public bool UseJsonSerializer { get; set; }
        public string? CustomMessage { get; set; }
    }
}
