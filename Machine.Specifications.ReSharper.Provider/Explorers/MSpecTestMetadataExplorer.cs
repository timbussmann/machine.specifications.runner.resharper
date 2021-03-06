namespace Machine.Specifications.ReSharperProvider.Explorers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using JetBrains.Application;
    using JetBrains.Metadata.Reader.API;
    using JetBrains.ProjectModel;
    using JetBrains.ReSharper.UnitTestFramework;

    [MetadataUnitTestExplorer]
    public partial class MSpecTestMetadataExplorer : IUnitTestMetadataExplorer
    {
        readonly AssemblyExplorer _assemblyExplorer;
        readonly MSpecUnitTestProvider _provider;

        public MSpecTestMetadataExplorer(MSpecUnitTestProvider provider,
                                         AssemblyExplorer assemblyExplorer)
        {
            this._assemblyExplorer = assemblyExplorer;
            this._provider = provider;
        }

        public void ExploreAssembly(IProject project, IMetadataAssembly assembly, UnitTestElementConsumer consumer, ManualResetEvent exitEvent)
        {
            this.ExploreAssembly(project, assembly, consumer);
        }

        public void ExploreAssembly(IProject project, IMetadataAssembly assembly, UnitTestElementConsumer consumer)
        {
            using (ReadLockCookie.Create()) //Get a read lock so that it is safe to read the assembly
            {
                foreach (var metadataTypeInfo in GetTypesIncludingNested(assembly.GetTypes()))
                    this._assemblyExplorer.Explore(project, assembly, consumer, metadataTypeInfo);
            }
        }

        private static IEnumerable<IMetadataTypeInfo> GetTypesIncludingNested(IEnumerable<IMetadataTypeInfo> types)
        {
            foreach (var type in (types ?? Enumerable.Empty<IMetadataTypeInfo>()))
            {
                foreach (var nestedType in GetTypesIncludingNested(type.GetNestedTypes())) //getting nested classes too
                {
                    yield return nestedType;
                }

                yield return type;
            }
        }

        public IUnitTestProvider Provider
        {
            get { return this._provider; }
        }
    }
}