using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Patcher.Patching;

namespace Patcher.Tests
{
    public static class PatchNodeSortTest
    {
        #region Class Definitions

        private abstract class DummyPatch : Patch
        {
            public override MethodInfo ModifiedMethod => null!;

            public override MethodInfo ModifyingMethod => null!;

            public override void Apply(IPatchRepository patchRepository)
            {
            }
        }

        private class PatchA : DummyPatch
        {
        }

        private class PatchB : DummyPatch
        {
        }

        private class PatchC : DummyPatch
        {
        }

        private class PatchD : DummyPatch
        {
        }

        private class PatchDependA : DummyPatch
        {
            public override Type? Dependency => typeof(PatchA);
        }

        private class PatchDependDependA1 : DummyPatch
        {
            public override Type? Dependency => typeof(PatchDependA);
        }

        private class PatchDependDependA2 : DummyPatch
        {
            public override Type? Dependency => typeof(PatchDependA);
        }

        #endregion

        [Test]
        public static void SortTest()
        {
            List<Patch> patches = new()
            {
                new PatchA(),
                new PatchB(),
                new PatchDependA(),
                new PatchC(),
                new PatchDependDependA1(),
                new PatchDependDependA2(),
                new PatchD()
            };
            
            PatchNode root = PatchNode.SortPatches(patches);
            RecursiveWriteTree(root);
        }

        private static void RecursiveWriteTree(PatchNode node, int indent = 0)
        {
            for (int i = 1; i < indent; i++)
                Console.Write("  ");
            
            Console.WriteLine(node.ToString());
            
            foreach (PatchNode child in node.Children)
                RecursiveWriteTree(child, indent + 1);
        }
    }
}