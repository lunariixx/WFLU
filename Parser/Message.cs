using System.Collections.Generic;

namespace WFLU
{
    public class Message
    {
        public string RawValue { get; set; } = "";
        public bool IsChoice { get; set; } = false;
        public Dictionary<string, string> Choices { get; set; } = new();
    }
}
