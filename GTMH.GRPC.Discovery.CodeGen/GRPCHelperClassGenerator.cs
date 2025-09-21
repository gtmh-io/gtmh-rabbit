using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GTMH.GRPC.Discovery.CodeGen
{
  [Generator]
  internal class GRPCHelperClassGenerator : IIncrementalGenerator
  {
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
      //System.Diagnostics.Debugger.Launch();
      IncrementalValuesProvider<DiscoverableDefn?> defns = context.SyntaxProvider.CreateSyntaxProvider(
        predicate: (node, cancelToken)=> FastFilterTarget(node),
        transform: (ctx, cancelToken)=> DeepSeekTarget(ctx)
      ).Where( _=> ! ( _ is null )  );
      context.RegisterSourceOutput(defns, (spc, source) => Write((DiscoverableDefn)source, spc)); // filtered for null above
    }

    static Regex REDiscoverableBase= new Regex(@"GTMH.GRPC.Discovery.Discoverable<(.*)Client>");
    private static bool FastFilterTarget(SyntaxNode node)
    {
      var cls = node as ClassDeclarationSyntax;
      if ( cls == null ) return false;
      if ( cls.BaseList == null ) return false;
      foreach (var baseType in cls.BaseList.Types)
      {
          // Check for direct generic name: Discoverable<T>
          if(baseType.Type is GenericNameSyntax genericName)
          {
            if(genericName.Identifier.Text == "Discoverable" && // may get false positives
                genericName.TypeArgumentList.Arguments.Count == 1)
            {
              return true;
            }
          }
          // Check for qualified name: GTMH.GRPC.Discovery.Discoverable<T>
          else if(baseType.Type is QualifiedNameSyntax qualified)
          {
            // Get the rightmost part which should be Discoverable<T>
            if(qualified.Right is GenericNameSyntax rightGeneric &&
                rightGeneric.Identifier.Text == "Discoverable" &&
                rightGeneric.TypeArgumentList.Arguments.Count == 1)
            {
              // Optionally check if the full name contains expected parts
              var fullName = qualified.ToString();
              if(fullName.Contains("GTMH") ||
                  fullName.Contains("Discovery"))
              {
                return true;
              }
            }
          }
      }
    
      return false;
    }

    private static DiscoverableDefn? DeepSeekTarget(GeneratorSyntaxContext ctx)
    {
      var cls = (ClassDeclarationSyntax)ctx.Node;
      var classSymbol = ctx.SemanticModel.GetDeclaredSymbol(cls);
      if ( classSymbol == null ) return null;
      var nts = classSymbol as INamedTypeSymbol;
      if ( nts == null ) return null;

      var baseType= nts.BaseType;
      if ( baseType == null ) return null;

      var m = REDiscoverableBase.Match(baseType.ToDisplayString());
      if ( ! m.Success ) return null;

      var usings = new List<string>();
      foreach(var use in cls.SyntaxTree.GetCompilationUnitRoot().Usings)
      {
        usings.Add(use.ToString());
      }

      var ns = GetNamespace(cls);

      return new DiscoverableDefn(GetVisibility(cls.Modifiers, cls.Parent is TypeDeclarationSyntax), ns, classSymbol.Name, m.Groups[1].Value, usings);
    }

    private static string GetVisibility(SyntaxTokenList modifiers, bool a_IsNested)
    {
      if (modifiers.Any(SyntaxKind.PublicKeyword))
          return "public";
      if (modifiers.Any(SyntaxKind.PrivateKeyword))
          return "private";
      if (modifiers.Any(SyntaxKind.ProtectedKeyword))
          return "protected";
      if (modifiers.Any(SyntaxKind.InternalKeyword))
          return "internal";

      // Default for interfaces is internal (if no modifier specified)
      if(a_IsNested)
      {
        // default is private
        return "private";
      }
      else
      {
        // default internal private
        return "internal";
      }
    }

    static string GetNamespace(BaseTypeDeclarationSyntax syntax)
    {
      // If we don't have a namespace at all we'll return an empty string
      // This accounts for the "default namespace" case
      string nameSpace = string.Empty;

      // Get the containing syntax node for the type declaration
      // (could be a nested type, for example)
      SyntaxNode potentialNamespaceParent = syntax.Parent;

      // Keep moving "out" of nested classes etc until we get to a namespace
      // or until we run out of parents
      while(potentialNamespaceParent != null &&
              !(potentialNamespaceParent is NamespaceDeclarationSyntax)
              && !(potentialNamespaceParent is FileScopedNamespaceDeclarationSyntax))
      {
        potentialNamespaceParent = potentialNamespaceParent.Parent;
      }

      // Build up the final namespace by looping until we no longer have a namespace declaration
      if(potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
      {
        // We have a namespace. Use that as the type
        nameSpace = namespaceParent.Name.ToString();

        // Keep moving "out" of the namespace declarations until we 
        // run out of nested namespace declarations
        while(true)
        {
          if(!( namespaceParent.Parent is NamespaceDeclarationSyntax parent ))
          {
            break;
          }

          // Add the outer namespace as a prefix to the final namespace
          nameSpace = $"{namespaceParent.Name}.{nameSpace}";
          namespaceParent = parent;
        }
      }

      // return the final namespace
      return nameSpace;
    }

    private static void Write(DiscoverableDefn a_Defn, SourceProductionContext a_Compiler)
    {
      var code = new Code();
      code.WriteLine($"// Generated by {nameof(GRPCHelperClassGenerator)}");
      code.WriteLine("#pragma warning disable 0105 // we might duplicate namespaces");
      code.WriteLine("#nullable enable");
      foreach(var use in a_Defn.Usings)
      {
        code.WriteLine(use);
      }
      code.WriteLine("using GTMH.Rabbit;");
      code.WriteLine("using GTMH.Rabbit.Impl;");
      code.WriteLine("#pragma warning restore 0105");

      if(!string.IsNullOrEmpty(a_Defn.Namespace))
      {
        code.WriteLine($"namespace {a_Defn.Namespace};");
      }
      code.WriteLine($"{a_Defn.Visibility} partial class {a_Defn.DiscoverableClass}");
      code.WriteLine("{");
      using(code.Indent())
      {
        code.WriteLine($"public override string DiscoverableType=>s_DiscoverableType;");
        code.WriteLine($"public static readonly string s_DiscoverableType=\"{a_Defn.DiscoverableType}\";");
        WriteConstructors(a_Defn, code);
        WriteGetClient(a_Defn, code);
      }
      code.WriteLine("}");
      code.WriteLine("#nullable restore");
      a_Compiler.AddSource($"{a_Defn.DiscoverableClass}.g.cs", code.ToString());
    }
    private static void WriteGetClient(DiscoverableDefn a_Defn, Code code)
    {
      code.WriteLine($"public static async Task<string?> {a_Defn.ClientMethodName}(IRabbitFactory a_Rabbit, CancellationToken a_Cancel, Microsoft.Extensions.Logging.ILogger ? a_Logger = null)");
      code.WriteLine("{");
      using(code.Indent())
      {
        code.WriteLine("var sinkFactory=RabbitStreamSinkFactory.Create(a_Rabbit, TransientQueueTopology.Create<DiscoveryRequest>());");
        code.WriteLine("var sink = await sinkFactory.CreateSink(a_Cancel);");
        code.WriteLine("await using(sink)");
        code.WriteLine("{");
        using(code.Indent())
        {
          code.WriteLine("var sourceFactory=RabbitStreamSourceFactory.Create(a_Rabbit, TransientQueueTopology.Create<DiscoveryResponse>());");
          code.WriteLine("var source = await sourceFactory.CreateSource(a_Cancel);");
          code.WriteLine("await using(source)");
          code.WriteLine("{");
          using(code.Indent())
          {
            code.WriteLine("var l = new Listener();");
            code.WriteLine("await source.AddListenerAsync(s_DiscoverableType, l);");
            code.WriteLine("try {");
            using(code.Indent())
            {
              code.WriteLine("await sink.PublishAsync(s_DiscoverableType, new DiscoveryRequest());");
              code.WriteLine("try {");
              code.WriteLine("return await l.URI.Task.WaitAsync(a_Cancel);");
              code.WriteLine(" } catch (System.OperationCanceledException) { return null; }");
            }
            code.WriteLine("} finally {");
            using(code.Indent())
            {
              code.WriteLine("try { await source.RemoveListenerAsync(s_DiscoverableType, l);} catch { }");
            }
            code.WriteLine("}");
          }
          code.WriteLine("}");
        }
        code.WriteLine("}");
      }
      code.WriteLine("}");
    }

    private static void WriteConstructors(DiscoverableDefn a_Defn, Code code)
    {
      code.WriteLine($"public {a_Defn.DiscoverableClass}(");
      using(code.Indent())
      {
        code.WriteLine("Microsoft.AspNetCore.Hosting.Server.IServer a_Server,");
        code.WriteLine("Microsoft.Extensions.Hosting.IHostApplicationLifetime a_HAL,");
        code.WriteLine("Microsoft.Extensions.Options.IOptions<DiscoveryConfig> a_Config,");
        code.WriteLine($"Microsoft.Extensions.Logging.ILogger <{ a_Defn.DiscoverableClass}> a_Log,");
        code.WriteLine("GTMH.Security.IDecryptor a_Decryptor)");
        code.WriteLine(": base(a_Server, a_HAL, a_Config, a_Log, a_Decryptor) { { } } ");
       }
    }
  }
}
