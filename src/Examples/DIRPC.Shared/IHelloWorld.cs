using GTMH.IDL;

namespace DIRPC.Shared;

[RPCInterface]
public interface IHelloWorld
{
  [RPCMethod]
  ValueTask<string> IntroducingAsync(string a_Identity);
}

