using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit.RPC
{
  internal class NullLogger : ILogger
  {
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
  }
}
