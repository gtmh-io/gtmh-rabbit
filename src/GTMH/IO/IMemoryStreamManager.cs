using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.IO
{
  public interface IMemoryStreamManager
  {
    IMemoryStream GetStreamImpl<T>();
  }
  public static class MemoryStreamManager
  {
    public static IMemoryStream GetStream<T>(this IMemoryStreamManager ? a_Manager)
    {
      return a_Manager != null ? a_Manager.GetStreamImpl<T>() : new VanillaMemoryStream();
    }
  }
}
