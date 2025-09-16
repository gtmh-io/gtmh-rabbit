using System;
using System.Collections.Generic;
using System.Text;

namespace Tofye.IMQ.UnitTests
{
  class Listener<T> : IMessageStreamListener<T>
  {
    public List<T> Recvd =new List<T>();
    public ManualResetEvent Throw = new ManualResetEvent(false);
    public ValueTask OnReceivedAsync(T a_Msg)
    {
      if ( Throw.WaitOne(0)) throw new Exception("Test exception");
      lock(Recvd)
      {
        Recvd.Add(a_Msg);
      }
      return ValueTask.CompletedTask;
    }
  }
}
