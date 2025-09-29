using GTMH.S11n.FieldTypes;

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

    private static List<IFieldType> ParseGTFields(INamedTypeSymbol classSymbol)
    {
      var attrs = new List<IFieldType>();
      foreach (var member in classSymbol.GetMembers())
      {
        if (member is IPropertySymbol property)
        {
          // Check for GTS11nAttribute on this property
          var gtfAttr = property.GetAttributes().FirstOrDefault(attr => attr.AttributeClass?.ToDisplayString() == "GTMH.S11n.GTS11nAttribute");
          if(gtfAttr != null)
          {
            attrs.Add(ParseAttribute(property, classSymbol, gtfAttr));
          }
        }
        else if( member is IFieldSymbol symbol)
        {
          var gtfAttr = symbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass?.ToDisplayString() == "GTMH.S11n.GTS11nAttribute");
          if(gtfAttr != null)
          {
            attrs.Add(ParseAttribute(symbol, classSymbol, gtfAttr));
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
            if(attr.AttributeClass.ToDisplayString() == "GTMH.S11n.GTS11nInitAttribute")
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

    private static GTFieldAttrs RealiseAttribute(AttributeData gtfAttr)
    {
      GTFieldAttrs rval = new GTFieldAttrs();
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
          case "Parse":
          {
            if(attr.Value.Value is string strValue)
            {
              if(!string.IsNullOrWhiteSpace(strValue))
              {
                rval.Parse = strValue;
              }
            }
            break;
          }
          case "DeParse":
          {
            if(attr.Value.Value is string strValue)
            {
              if(!string.IsNullOrWhiteSpace(strValue))
              {
                rval.DeParse = strValue;
              }
            }
            break;
          }
          case "Instance":
          {
            if(attr.Value.Value is bool boolValue)
            {
              rval.Instance = boolValue;
            }
            break;
          }
          case "Required":
          {
            if(attr.Value.Value is bool boolValue)
            {
              rval.Required = boolValue;
            }
            break;
          }
          default:
          {
            // TODO - better
            //System.Diagnostics.Debugger.Launch();
            throw new NotImplementedException();
          }
        }
      }
      return rval;
    }

    private static IFieldType ParseAttribute(IPropertySymbol property, INamedTypeSymbol a_Container, AttributeData a_AttrData)
    {
      return ParseAttribute(property.Name, property.Type, a_Container, a_AttrData);
    }

    private static IFieldType ParseAttribute(IFieldSymbol field, INamedTypeSymbol a_Container, AttributeData a_AttrData)
    {
      return ParseAttribute(field.Name, field.Type, a_Container, a_AttrData);
    }

    static ITypeSymbol GetInstanceType(ISymbol a_Symbol)
    {
      if(a_Symbol is IPropertySymbol property)
      {
        return property.Type;
      }
      else if(a_Symbol is IFieldSymbol field)
      {
        return field.Type;
      }
      else
      {
        throw new ArgumentException("Invalid type for instance");

      }
    }

    static readonly Regex REArray= new Regex("System.Collections.Immutable.ImmutableArray<.*>");
    private static IFieldType ParseAttribute(string a_Name, ITypeSymbol a_Type, INamedTypeSymbol a_Container, AttributeData a_AttrData)
    {
      var attr = RealiseAttribute(a_AttrData);
      if(attr.Instance)
      {
        if ( a_Type is INamedTypeSymbol nts )
        {
          if(REArray.IsMatch(nts.ToDisplayString()))
          {
            if(nts.TypeArguments.Length > 0)
            {
              if(nts.TypeArguments[0] is INamedTypeSymbol t_nts)
              {
                return new InstanceArrayField(a_Name, t_nts.ToDisplayString(), attr);
              }
            }
          }
        }
        return new InstanceField(a_Name, a_Type.ToDisplayString(), attr);

      }
      else if(attr.Parse != null || attr.DeParse != null)
      {
        return new CustomField(a_Name, attr);
      }
      switch(a_Type.TypeKind)
      {
        case TypeKind.Enum:
        {
          return new EnumField(a_Name, a_Type.ToDisplayString(), attr);
        }
        default:
        {
          return new TryParseField(a_Name, a_Type.Name, attr);
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
      code.WriteLine("using System.Reflection;");
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
        code.WriteLine("return S11nGather(new GTParseArgs()).Value;");
      }
      code.WriteLine("}");
      code.WriteLine($"public {modifier} IGTParseArgs S11nGather(IGTParseArgs a_Args)");
      code.WriteLine("{");
      if(a_Defn.HasGTParent)
      {
        code.WriteLine("base.S11nGather(a_Args);");
      }
      using(code.Indent())
      {
        foreach(var field in a_Defn.Fields)
        {
          field.WriteGather(code);

        }
        code.WriteLine("return a_Args;");
      }
      code.WriteLine("}");
    }

    private static void WriteConstructors(S11nClassDefn a_Defn, Code code)
    {
      if(a_Defn.CustomConstructor)
      {
        code.WriteLine("private void SetS11n(IGTInitArgs a_Args)");
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
        code.WriteLine($"public {a_Defn.ClassName}(IGTInitArgs a_Args){parentInit}");
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
