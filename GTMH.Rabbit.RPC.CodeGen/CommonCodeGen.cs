using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GTMH.Rabbit.RPC.CodeGen
{
  public class CommonCodeGen
  {
    public struct AsyncReturnType
    {
      public bool IsAsync { get; set; }
      public string Type { get; set; }
      public static AsyncReturnType Synchronous => new AsyncReturnType { IsAsync = false };
      public static AsyncReturnType Asynchronous(string a_Type) => new AsyncReturnType { IsAsync = true, Type = a_Type };
    }
    public static AsyncReturnType IsAsyncReturnType(string a_Type)
    {
      // not a compiler !
      if ( ! a_Type.Contains("Task")) return AsyncReturnType.Synchronous;
      var reTask = new Regex(@"^System.Threading.Tasks.Task<(.*)>");
      var reValueTask = new Regex(@"^System.Threading.Tasks.ValueTask<(.*)>");
      var reVoidTask = new Regex("^System.Threading.Tasks.Task$");
      var reVoidValueTask = new Regex("^System.Threading.Tasks.ValueTask$");
      var m = reTask.Match(a_Type);
      if(m.Success)
      {
        return AsyncReturnType.Asynchronous(m.Groups[1].Value);
      }
      m = reValueTask.Match(a_Type);
      if(m.Success)
      {
        return AsyncReturnType.Asynchronous(m.Groups[1].Value);
      }
      if ( reVoidTask.IsMatch(a_Type) ) return AsyncReturnType.Asynchronous(null);
      if ( reVoidValueTask.IsMatch(a_Type) ) return AsyncReturnType.Asynchronous(null);
      return AsyncReturnType.Synchronous;
    }
  }
}
