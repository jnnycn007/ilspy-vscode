using ILSpyX.Backend.Application;
using ILSpyX.Backend.Decompiler;
using ILSpyX.Backend.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ILSpyX.Backend.TreeProviders;

public class NamespaceNodeProvider(ILSpyXApplication application) : ITreeNodeProvider
{
    public DecompileResult Decompile(NodeMetadata nodeMetadata, string outputLanguage)
    {
        string namespaceName = string.IsNullOrEmpty(nodeMetadata.Name) ? "<global>" : nodeMetadata.Name;
        return outputLanguage switch
        {
            LanguageName.IL => DecompileResult.WithCode($"namespace {namespaceName}"),
            _ => DecompileResult.WithCode($"namespace {namespaceName} {{ }}")
        };
    }

    public IEnumerable<Node> CreateNodes(string assemblyPath)
    {
        var decompiler = application.DecompilerBackend.CreateDecompiler(assemblyPath);
        if (decompiler is null)
        {
            return Enumerable.Empty<Node>();
        }

        var types = decompiler.TypeSystem.MainModule.TopLevelTypeDefinitions;
        var namespaces = new HashSet<string>(decompiler.TypeSystem.NameComparer);
        foreach (var type in types)
        {
            namespaces.Add(type.Namespace);
        }
        return namespaces
            .OrderBy(n => n)
            .Select(ns => new Node(
                new NodeMetadata(
                    AssemblyPath: assemblyPath,
                    Type: NodeType.Namespace,
                    Name: ns,
                    SymbolToken: 0,
                    ParentSymbolToken: 0),
                DisplayName: ns,
                Description: string.Empty,
                MayHaveChildren: true,
                SymbolModifiers: SymbolModifiers.None
            ));
    }

    public Task<IEnumerable<Node>> GetChildrenAsync(NodeMetadata? nodeMetadata)
    {
        if (nodeMetadata?.Type != NodeType.Namespace)
        {
            return Task.FromResult(Enumerable.Empty<Node>());
        }

        return Task.FromResult(
            application.TreeNodeProviders.Type.CreateNodes(nodeMetadata.AssemblyPath, nodeMetadata.Name));
    }
}

