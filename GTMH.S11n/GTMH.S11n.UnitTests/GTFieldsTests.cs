using GTMH.S11n.UnitTests.Impl;

using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.UnitTests
{
  public class GTFieldsTests
  {
    [Test]
    public async ValueTask TestHasGTFieldsAsProperties()
    {
      var obj = new HasGTFieldsAsProperties("roger", 1974, HasGTFieldsAsProperties.Value_t.ValueB);
      var s11n = obj.ParseS11n();
      var _obj = new HasGTFieldsAsProperties(new GTArgs(s11n));
      await Assert.That(_obj.StringValue).IsEqualTo("roger");
      await Assert.That(_obj.IntValue).IsEqualTo(1974);
      await Assert.That(_obj.EnumValue).IsEqualTo(HasGTFieldsAsProperties.Value_t.ValueB);
    }
    [Test]
    public async ValueTask TestHasGTFieldsAsROFields()
    {
      var obj = new HasGTFieldsAsROFields("roger", 1974, HasGTFieldsAsROFields.Value_t.ValueB);
      var s11n = obj.ParseS11n();
      var _obj = new HasGTFieldsAsROFields(new GTArgs(s11n));
      await Assert.That(_obj.StringValue).IsEqualTo("roger");
      await Assert.That(_obj.IntValue).IsEqualTo(1974);
      await Assert.That(_obj.EnumValue).IsEqualTo(HasGTFieldsAsROFields.Value_t.ValueB);
    }
    [Test]
    public async ValueTask TestHasGTFields000_Default()
    {
      var obj = new HasGTFieldsAsProperties(new GTArgs() );
      await Assert.That(obj.StringValue).IsEqualTo("StringValueDefault");
      await Assert.That(obj.IntValue).IsEqualTo(69);
      await Assert.That(obj.EnumValue).IsEqualTo(HasGTFieldsAsProperties.Value_t.ValueA);
      var s11n = obj.ParseS11n();
      var _obj = new HasGTFieldsAsProperties(new GTArgs(s11n));
      await Assert.That(_obj.StringValue).IsEqualTo("StringValueDefault");
      await Assert.That(_obj.IntValue).IsEqualTo(69);
      await Assert.That(_obj.EnumValue).IsEqualTo(HasGTFieldsAsProperties.Value_t.ValueA);
    }
    [Test]
    public async ValueTask TestBaseDerivedSimple()
    {
      var obj = new HasGTFieldsDerived("roger_derived", "roger_base");
      await Assert.That(obj.BaseStringValue).IsEqualTo("roger_base");
      await Assert.That(obj.DerivedStringValue).IsEqualTo("roger_derived");
      var s11n = obj.ParseS11n();
      var _obj = new HasGTFieldsDerived(new GTArgs(s11n));
      await Assert.That(_obj.BaseStringValue).IsEqualTo("roger_base");
      await Assert.That(_obj.DerivedStringValue).IsEqualTo("roger_derived");

    }
    [Test]
    public async ValueTask TestBaseDerivedNotGTFields()
    {
      var obj = new HasNotGTFieldsDerived("roger");
      var s11n = obj.ParseS11n();
      var _obj = new HasNotGTFieldsDerived(new GTArgs(s11n));
      await Assert.That(_obj.BaseStringValue).IsEqualTo("roger");
    }
  }
}
