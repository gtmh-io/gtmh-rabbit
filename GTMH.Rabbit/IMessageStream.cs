using System;
using System.Collections.Generic;
using System.Text;

namespace Tofye.IMQ
{
  public interface IMessageStreamListener<M>
  {
    ValueTask OnReceivedAsync(M a_Msg);
  }

  public interface IMessageStreamSource<M> : IAsyncDisposable
  {
    ValueTask AddListenerAsync(string ? a_RoutingKey, IMessageStreamListener<M> a_Listener);
    ValueTask RemoveListenerAsync(string ? a_RoutingKey, IMessageStreamListener<M> a_Listener);
  }

  public interface IMessageStreamSourceFactory<M>
  {
    ValueTask<IMessageStreamSource<M>> CreateSource(CancellationToken a_Cancel = default);
  }

  public interface IMessageStreamSink<M> : IAsyncDisposable
  {
    ValueTask PublishAsync(string a_RoutingKey, M a_Msg, CancellationToken a_Cancel = default);
  }

  public interface IMessageStreamSinkFactory<M>
  {
    ValueTask<IMessageStreamSink<M>> CreateSink(CancellationToken a_Cancel = default);
  }
}
