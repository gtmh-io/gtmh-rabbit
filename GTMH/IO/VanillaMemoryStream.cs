using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.IO
{
  public class VanillaMemoryStream : IMemoryStream
  {
    private System.IO.MemoryStream m_Stream;
    public ReadOnlyMemory<byte> Content => m_Stream.ToArray();
    public System.IO.Stream Impl()=>m_Stream;
    public VanillaMemoryStream(int a_Size = 0) => m_Stream = new MemoryStream(a_Size);
    public void Dispose()=> m_Stream.Dispose();
  }
}
