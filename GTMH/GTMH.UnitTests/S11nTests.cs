using ProtoBuf;

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

using GTMH.S11n;

namespace GTMH.UnitTests;

// https://tunit.dev/

public class S11nTests
{
  [DataContract]
  struct Type_t
  {
    [DataMember(Order =10)]
    public string StringValue { get; set; }
    [DataMember(Order =20)]
    public int IntValue { get; set; }
  }
  [DataContract][ProtoContract] // requires [DataContract]
  struct Type_u
  {
    [DataMember][ProtoMember(10)] // requires [DataMember]
    public string StringValue { get; set; }
    [DataMember][ProtoMember(20)] // ditto
    public int IntValue { get; set; }
  }
  [Test]
  public async ValueTask XMLS11n_Type_t()
  {
    var v = new Type_t { StringValue = "xyz", IntValue = 3141 };
    var _v = DCS11n.FromXML<Type_t>(DCS11n.XML(v));
    await Assert.That(v.StringValue).IsEqualTo( _v.StringValue);
    await Assert.That(v.IntValue).IsEqualTo( _v.IntValue);
  }
  [Test]
  public async ValueTask XMLS11n_Type_u()
  {
    var v = new Type_u { StringValue = "xyz", IntValue = 3141 };
    var _v = DCS11n.FromXML<Type_u>(DCS11n.XML(v));
    await Assert.That(v.StringValue).IsEqualTo(_v.StringValue);
    await Assert.That(v.IntValue).IsEqualTo(_v.IntValue);
  }
  [Test]
  public async ValueTask TestPBufferS11n()
  {
    var buf = PBuffer.Create( 69 );
    var value = buf.GetValue<int>();
    await Assert.That(value).IsEqualTo(69);
  }
  [Test]
  public async ValueTask TestImmutArrayS11n()
  {
    var array = ImmutableArray.Create(1, 2, 3);
    var buf = PBuffer.Create(array);
    var value = buf.GetValue<ImmutableArray<int>>();
    //var value = ImmutableArray.Create(1, 2, 3, 4);
    //var value = ImmutableArray.Create(1, 2, 4);
    await Assert.That(value).IsEquivalentTo(array);
  }
}
