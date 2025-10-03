using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Rabbit.RPC.UnitTests
{
  public class TestMarshalling
  {
    [Test]
    public async ValueTask TestResultVoid()
    {
      var msg = RPCResult.Void();
      var _msg = PBuffer.Clone(msg);
      await Assert.That( ()=>true ).IsTrue();
    }
    [Test]
    public async ValueTask TestResultReturnValue()
    {
      var result = 42;
      var msg = RPCResult.ReturnValue(result);
      var _msg = PBuffer.Clone(msg);
      await Assert.That( _msg.Unpack<int>()).IsEqualTo(result);
    }
    [Test]
    public async ValueTask TestResultException()
    {
      var msg = RPCResult.Failure(new Exception("Test Exception"));
      var _msg = PBuffer.Clone(msg);
      await Assert.That( _msg.ExceptionData ).IsNotNullOrEmpty();
    }
    [Test]
    public async ValueTask TestPack0()
    {
      var msg = RPCCall.Pack("HelloWorld");
      var _msg = PBuffer.Clone(msg);
      await Assert.That(_msg.MethodName).IsEqualTo(msg.MethodName);
    }
    [Test]
    public async ValueTask TestPack1()
    {
      var msg = RPCCall.Pack("HelloWorld", "0");
      var _msg = PBuffer.Clone(msg);
      await Assert.That(_msg.MethodName).IsEqualTo(msg.MethodName);
      await Assert.That(_msg.Unpack<string>(0)).IsEqualTo("0");
    }
    [Test]
    public async ValueTask TestPack2()
    {
      var msg = RPCCall.Pack("HelloWorld", "0", "1");
      var _msg = PBuffer.Clone(msg);
      await Assert.That(_msg.MethodName).IsEqualTo(msg.MethodName);
      await Assert.That(_msg.Unpack<string>(0)).IsEqualTo("0");
      await Assert.That(_msg.Unpack<string>(1)).IsEqualTo("1");
    }
    [Test]
    public async ValueTask TestPack3()
    {
      var msg = RPCCall.Pack("HelloWorld", "0", "1", "2");
      var _msg = PBuffer.Clone(msg);
      await Assert.That(_msg.MethodName).IsEqualTo(msg.MethodName);
      await Assert.That(_msg.Unpack<string>(0)).IsEqualTo("0");
      await Assert.That(_msg.Unpack<string>(1)).IsEqualTo("1");
      await Assert.That(_msg.Unpack<string>(2)).IsEqualTo("2");
    }
    [Test]
    public async ValueTask TestPack4()
    {
      var msg = RPCCall.Pack("HelloWorld", "0", "1", "2", "3");
      var _msg = PBuffer.Clone(msg);
      await Assert.That(_msg.MethodName).IsEqualTo(msg.MethodName);
      await Assert.That(_msg.Unpack<string>(0)).IsEqualTo("0");
      await Assert.That(_msg.Unpack<string>(1)).IsEqualTo("1");
      await Assert.That(_msg.Unpack<string>(2)).IsEqualTo("2");
      await Assert.That(_msg.Unpack<string>(3)).IsEqualTo("3");
    }
    [Test]
    public async ValueTask TestPack5()
    {
      var msg = RPCCall.Pack("HelloWorld", "0", "1", "2", "3", "4");
      var _msg = PBuffer.Clone(msg);
      await Assert.That(_msg.MethodName).IsEqualTo(msg.MethodName);
      await Assert.That(_msg.Unpack<string>(0)).IsEqualTo("0");
      await Assert.That(_msg.Unpack<string>(1)).IsEqualTo("1");
      await Assert.That(_msg.Unpack<string>(2)).IsEqualTo("2");
      await Assert.That(_msg.Unpack<string>(3)).IsEqualTo("3");
      await Assert.That(_msg.Unpack<string>(4)).IsEqualTo("4");
    }
  }
}
