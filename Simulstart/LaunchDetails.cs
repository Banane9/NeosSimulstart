using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulstart
{
    [JsonObject]
    internal class LaunchDetails
    {
        [JsonProperty]
        public bool Administrator { get; private set; }

        [JsonProperty]
        public string Arguments { get; private set; }

        [JsonProperty]
        public bool KeepOpen { get; private set; }

        [JsonProperty]
        public string Path { get; private set; }

        [JsonProperty]
        public bool Restart { get; private set; }

        public bool UseWorkingDirectory => !string.IsNullOrWhiteSpace(WorkingDirectory) && WorkingDirectory != System.IO.Path.GetDirectoryName(Path);

        [JsonProperty]
        public string WorkingDirectory { get; private set; }

        public override string ToString()
            => $"{(Administrator ? "Admin" : "Regular")} ({(KeepOpen ? "independent of Neos" : "closing with Neos")}): {Path} {Arguments}";
    }
}