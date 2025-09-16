using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.IO
{
  public interface IMemoryStream : IDisposable
  {
    ReadOnlyMemory<byte> Content { get; }
    System.IO.Stream Impl();
  }
}
