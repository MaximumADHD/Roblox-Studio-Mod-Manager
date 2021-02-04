using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobloxStudioModManager
{
    public struct Package
    {
        public string Name { get; set; }
        public string Signature { get; set; }
        public int PackedSize { get; set; }
        public int Size { get; set; }
    }
}
