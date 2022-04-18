using System;
using System.Collections.Generic;
using System.Linq;

namespace Patcher.Patching
{
    public class PatchNode : IPatchNode
    {
        public IPatch? Patch { get; }
        
        public IPatchNode? Parent { get; set; }

        public List<IPatchNode> Children { get; } = new();
        
        public PatchNode(IPatch? patch)
        {
            Patch = patch;
        }

        public override string ToString() =>
            $"{Patch?.GetType().Name ?? "NULL"} ({(Children.Count == 1 ? "1 child" : Children.Count + " children")})";

        public static PatchNode SortPatches(IEnumerable<IPatch> patches)
        {
            Dictionary<Type, PatchNode> nodesByType = patches.ToDictionary(
                patch => patch.GetType(),
                patch => new PatchNode(patch)
            );
            
            PatchNode root = new(null);

            foreach (PatchNode node in nodesByType.Values.OrderBy(x => x.GetType().Name))
            {
                Type? depType = node.Patch?.Dependency;

                if (depType is null)
                {
                    root.Children.Add(node);
                    continue;
                }

                PatchNode dep = nodesByType[depType];
                node.Parent = dep;
                dep.Children.Add(node);
            }

            return root;
        }
    }
}