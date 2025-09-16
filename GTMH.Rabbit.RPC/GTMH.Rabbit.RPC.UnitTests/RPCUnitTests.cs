using GTMH.Security;

using Microsoft.Extensions.Logging;

using Moq;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GTMH.Rabbit.RPC.UnitTests
{
  public class RPCUnitTests
  {
    public Mock<IRPCFactory> RPCFactory=new();
    public ILogger Logger;
    class LoggerImpl : ILogger
    {
      public IDisposable? BeginScope<TState>(TState state) where TState : notnull
      {
        return null;
      }

      public bool IsEnabled(LogLevel logLevel)
      {
        return true;
      }

      public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
      {
        Debug.WriteLine($"[{logLevel}]: {formatter(state, exception)}");
      }
    }
    public RPCUnitTests()
    {
      RPCFactory.Setup(_=>_.Transport).Returns( new RabbitFactory(new RabbitConfig { Host = "localhost" }, new PlainText() ));
      //Logger = new LoggerImpl();
      Logger = new Mock<ILogger>().Object;
    }
  }
}
