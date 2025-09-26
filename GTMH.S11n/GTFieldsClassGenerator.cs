using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace GTMH.S11n
{
  [Generator]
  internal class GTFieldsClassGenerator : IIncrementalGenerator
  {
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
      //System.Diagnostics.Debugger.Launch();
      IncrementalValuesProvider<S11nClassDefn> defns = context.SyntaxProvider.CreateSyntaxProvider(
        predicate: (node, cancelToken)=> FastFilterTarget(node),
        transform: (ctx, cancelToken)=> DeepSeekTarget(ctx)
      ).Where( _=> ! ( _ is null )  );
      context.RegisterSourceOutput(defns, (spc, source) => Write((S11nClassDefn)source, spc)); // filtered for null above
    }

    private static bool FastFilterTarget(SyntaxNode node)
    {
      var cls = node as ClassDeclarationSyntax;
      if ( cls == null ) return false;
      /*foreach(var pds in cls.Members.OfType<PropertyDeclarationSyntax>().Where(property => property.AttributeLists.Any()))
      {
        if ( pds.AttributeLists.Any(al=>
        {
          return al.Attributes.Any(attr=>
          {
            var ins = attr.Name as IdentifierNameSyntax;
            if ( ins ==null ) return false;
            return ins.Identifier.ValueText=="GTField";
          });
        })) return true;
      }
      foreach(var fds in cls.Members.OfType<FieldDeclarationSyntax>().Where(field => field.AttributeLists.Any()))
      {
        if ( fds.AttributeLists.Any(al=>
        {
          return al.Attributes.Any(attr=>
          {
            var ins = attr.Name as IdentifierNameSyntax;
            if ( ins ==null ) return false;
            return ins.Identifier.ValueText=="GTField";
          });
        })) return true;
      }
      return false;*/
      return true;
    }

    private static List<S11nClassDefn.IFieldData> ParseGTFields(INamedTypeSymbol classSymbol)
    {
      var attrs = new List<S11nClassDefn.IFieldData>();
      foreach (var member in classSymbol.GetMembers())
      {
        if (member is IPropertySymbol property)
        {
          // Check for GTFieldAttribute on this property
          var gtfAttr = property.GetAttributes().FirstOrDefault(attr => attr.AttributeClass?.Name == "GTFieldAttribute" || attr.AttributeClass?.ToDisplayString() == "GTMH.S11n.GTFieldAttribute");
          if(gtfAttr != null)
          {
            attrs.Add(ParseAttribute(property, gtfAttr));
          }
        }
        else if( member is IFieldSymbol symbol)
        {
          var gtfAttr = symbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass?.Name == "GTFieldAttribute" || attr.AttributeClass?.ToDisplayString() == "GTMH.S11n.GTFieldAttribute");
          if(gtfAttr != null)
          {
            attrs.Add(ParseAttribute(symbol, gtfAttr));
          }
        }
      }
      return attrs;
    }

    private static S11nClassDefn DeepSeekTarget(GeneratorSyntaxContext ctx)
    {
      var cls = (ClassDeclarationSyntax)ctx.Node;
      var classSymbol = ctx.SemanticModel.GetDeclaredSymbol(cls);
      if ( classSymbol == null ) return null;

      var attrs=ParseGTFields(classSymbol);

      // need to check if any parent classes have GTFields
      var isGTDerived = SeekGTFieldParents(classSymbol.BaseType);

      if(!attrs.Any()&&!isGTDerived)
      {
        return null;
      }
      // check for custom constructors
      var constructors = ParseCustomConstructors(classSymbol);

      var ns = GetNamespace(cls);
      var usings = new List<string>();
      foreach(var use in cls.SyntaxTree.GetCompilationUnitRoot().Usings)
      {
        usings.Add(use.ToString());
      }
      return new S11nClassDefn(usings, ns, GetVisibility(cls.Modifiers, cls.Parent is TypeDeclarationSyntax), classSymbol.Name, attrs, isGTDerived, constructors.Any());
    }

    private static List<IMethodSymbol> ParseCustomConstructors(INamedTypeSymbol classSymbol)
    {
      var rval = new List<IMethodSymbol>();
      foreach(var constructor in classSymbol.Constructors)
      {
        var attrs = constructor.GetAttributes();
        if(attrs != null&&attrs.Length >0)
        {
          foreach(var attr in attrs)
          {
            if(attr.AttributeClass.ToDisplayString() == "GTMH.S11n.GTFieldCustomConstructorAttribute")
            {
              rval.Add(constructor);
            }
          }
        }
      }
      return rval;
    }

    private static bool SeekGTFieldParents(INamedTypeSymbol baseType)
    {
      if ( baseType == null ) return false;
      else if ( baseType.SpecialType == SpecialType.System_Object ) return false;
      var attrs = ParseGTFields(baseType);
      if ( attrs.Any() ) return true;
      else return SeekGTFieldParents(baseType.BaseType);
    }

    private static S11nClassDefn.GTFieldAttrs RealiseAttribute(AttributeData gtfAttr)
    {
      S11nClassDefn.GTFieldAttrs rval = new S11nClassDefn.GTFieldAttrs();
      foreach(var attr in gtfAttr.NamedArguments)
      {
        switch(attr.Key)
        {
          case "AKA":
          {
            if(attr.Value.Value is string strValue)
            {
              if(!string.IsNullOrWhiteSpace(strValue))
              {
                rval.AKA = strValue;
              }
            }
            break;
          }
          default:
          {
            System.Diagnostics.Debugger.Launch();
            throw new NotImplementedException();
          }
        }
      }
      return rval;
    }

    private static S11nClassDefn.IFieldData ParseAttribute(IPropertySymbol property, AttributeData gtfAttr)
    {
      switch(property.Type.TypeKind)
      {
        case TypeKind.Enum:
        {
          return new S11nClassDefn.EnumField(property.Name, property.Type.ToDisplayString(), RealiseAttribute(gtfAttr));
        }
        default:
        {
          return new S11nClassDefn.TryParseField(property.Name, property.Type.Name, RealiseAttribute(gtfAttr));
        }
      }
    }

    private static S11nClassDefn.IFieldData ParseAttribute(IFieldSymbol field, AttributeData gtfAttr)
    {
      switch(field.Type.TypeKind)
      {
        case TypeKind.Enum:
        {
          return new S11nClassDefn.EnumField(field.Name, field.Type.ToDisplayString(), RealiseAttribute(gtfAttr));
        }
        default:
        {
          return new S11nClassDefn.TryParseField(field.Name, field.Type.Name, RealiseAttribute(gtfAttr));
        }
      }
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

    private static void Write(S11nClassDefn a_Defn, SourceProductionContext a_Compiler)
    {
      var code = new Code();
      code.WriteLine($"// Generated by {nameof(GTFieldsClassGenerator)}");
      code.WriteLine("#pragma warning disable 0105 // we might duplicate namespaces");
      code.WriteLine("#nullable enable");
      foreach(var use in a_Defn.Usings)
      {
        code.WriteLine(use);
      }
      code.WriteLine("using GTMH.S11n;");
      code.WriteLine("#pragma warning restore 0105");

      if(!string.IsNullOrEmpty(a_Defn.Namespace))
      {
        code.WriteLine($"namespace {a_Defn.Namespace};");
      }
      code.WriteLine($"{a_Defn.Visibility} partial class {a_Defn.ClassName}");
      code.WriteLine("{");
      using(code.Indent())
      {
        WriteConstructors(a_Defn, code);
        WriteS11n(a_Defn, code);
      }
      code.WriteLine("}");
      code.WriteLine("#nullable restore");
      a_Compiler.AddSource($"{a_Defn.ClassName}.g.cs", code.ToString());
    }

    private static void WriteS11n(S11nClassDefn a_Defn, Code code)
    {
      var modifier = a_Defn.HasGTParent ? "override" : "virtual";
      code.WriteLine($"public {modifier} Dictionary<string,string> ParseS11n()");
      code.WriteLine("{");
      using(code.Indent())
      {
        code.WriteLine("return S11nGather(new Dictionary<string, string>());");
      }
      code.WriteLine("}");
      code.WriteLine($"public {modifier} Dictionary<string,string> S11nGather(Dictionary<string,string> a_Args)");
      code.WriteLine("{");
      if(a_Defn.HasGTParent)
      {
        code.WriteLine("base.S11nGather(a_Args);");
      }
      using(code.Indent())
      {
        foreach(var field in a_Defn.Fields)
        {
          field.WriteGather(code, "a_Args");

        }
        code.WriteLine("return a_Args;");
      }
      code.WriteLine("}");
    }

    private static void WriteConstructors(S11nClassDefn a_Defn, Code code)
    {
      if(a_Defn.CustomConstructor)
      {
        code.WriteLine("private void SetS11n(IGTArgs a_Args)");
        code.WriteLine("{");
        using(code.Indent())
        {
          foreach(var field in a_Defn.Fields)
          {
            field.WriteInitialisation(code);
          }
        }
        code.WriteLine("}");
      }
      else
      {
        var parentInit = a_Defn.HasGTParent ? " : base(a_Args)" : "";
        code.WriteLine($"public {a_Defn.ClassName}(IGTArgs a_Args){parentInit}");
        code.WriteLine("{");
        using(code.Indent())
        {
          foreach(var attr in a_Defn.Fields)
          {
            attr.WriteInitialisation(code);
          }
        }
        code.WriteLine("}");
      }
    }
  }
}
