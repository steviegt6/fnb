using System.Collections.Generic;

namespace Patcher.Patching
{
    public interface IPatchNode
    {
        IPatch? Patch { get; }

        IPatchNode? Parent { get; set; }
        
        List<IPatchNode> Children { get; }
    }
}