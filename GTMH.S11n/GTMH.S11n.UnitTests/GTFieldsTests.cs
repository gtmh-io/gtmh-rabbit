using GTMH.S11n.UnitTests.Impl;

using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.UnitTests
{
  public class GTFieldsTestsG
  {
    [Test]
    public async ValueTask TestHasGTFieldsAsProperties()
    {
      var obj = new HasGTFieldsAsProperties("roger", 1974, HasGTFieldsAsProperties.Value_t.ValueB);
      var s11n = obj.ParseS11n();
      var _obj = new HasGTFieldsAsProperties(new DictionaryConfig(s11n).ForInit());
      await Assert.That(_obj.StringValue).IsEqualTo("roger");
      await Assert.That(_obj.IntValue).IsEqualTo(1974);
      await Assert.That(_obj.EnumValue).IsEqualTo(HasGTFieldsAsProperties.Value_t.ValueB);
    }
    [Test]
    public async ValueTask TestHasGTFieldsAsROFields()
    {
      var obj = new HasGTFieldsAsROFields("roger", 1974, HasGTFieldsAsROFields.Value_t.ValueB);
      var s11n = obj.ParseS11n();
      var _obj = new HasGTFieldsAsROFields(new DictionaryConfig(s11n).ForInit());
      await Assert.That(_obj.StringValue).IsEqualTo("roger");
      await Assert.That(_obj.IntValue).IsEqualTo(1974);
      await Assert.That(_obj.EnumValue).IsEqualTo(HasGTFieldsAsROFields.Value_t.ValueB);
    }
    [Test]
    public async ValueTask TestHasGTFields000_Default()
    {
      var obj = new HasGTFieldsAsProperties(new DictionaryConfig().ForInit());
      await Assert.That(obj.StringValue).IsEqualTo("StringValueDefault");
      await Assert.That(obj.IntValue).IsEqualTo(69);
      await Assert.That(obj.EnumValue).IsEqualTo(HasGTFieldsAsProperties.Value_t.ValueA);
      var s11n = obj.ParseS11n();
      var _obj = new HasGTFieldsAsProperties(new DictionaryConfig(s11n).ForInit());
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
      var _obj = new HasGTFieldsDerived(new DictionaryConfig(s11n).ForInit());
      await Assert.That(_obj.BaseStringValue).IsEqualTo("roger_base");
      await Assert.That(_obj.DerivedStringValue).IsEqualTo("roger_derived");

    }
    [Test]
    public async ValueTask TestBaseDerivedNotGTFields()
    {
      var obj = new HasNotGTFieldsDerived("roger");
      var s11n = obj.ParseS11n();
      var _obj = new HasNotGTFieldsDerived(new DictionaryConfig(s11n).ForInit());
      await Assert.That(_obj.BaseStringValue).IsEqualTo("roger");
    }
    [Test]
    public async ValueTask TestAKA()
    {
      var obj = new HasGTFieldsAKA(new DictionaryConfig(new Dictionary<string,string>{
        { "OldStringProperty", "roger" },
        { "OldIntProperty", "1974" },
        { "OldEnumProperty", "ValueB" },
      }).ForInit());
      await Assert.That(obj.NewStringProperty).IsEqualTo("roger");
      await Assert.That(obj.NewIntProperty).IsEqualTo(1974);
      await Assert.That(obj.NewEnumProperty).IsEqualTo(HasGTFieldsAKA.Enum_t.ValueB);
    }
    [Test]
    public async ValueTask TestAKAExtant()
    {
      // should prefer the 'new' values
      var obj = new HasGTFieldsAKA(new DictionaryConfig(new Dictionary<string,string>{
        { "OldStringProperty", "roger" },
        { "OldIntProperty", "1974" },
        { "OldEnumProperty", "ValueB" },
        { "NewStringProperty", "rabbit" },
        { "NewIntProperty", "3141" },
        { "NewEnumProperty", "ValueC" },
      }).ForInit());
      await Assert.That(obj.NewStringProperty).IsEqualTo("rabbit");
      await Assert.That(obj.NewIntProperty).IsEqualTo(3141);
      await Assert.That(obj.NewEnumProperty).IsEqualTo(HasGTFieldsAKA.Enum_t.ValueC);
    }
    [Test]
    public async ValueTask TestCustomParsing()
    {
      var obj = new HasGTFieldsCustomParse("roger", "rabbit");
      var s11n = obj.ParseS11n();
      var _obj = new HasGTFieldsCustomParse(new DictionaryConfig(s11n).ForInit());
      await Assert.That(_obj.Id.Code).IsEqualTo("roger");
      await Assert.That(_obj.Id2.Code).IsEqualTo("rabbit");
    }
    [Test]
    public async ValueTask TestCustomParsingAKA()
    {
      var obj = new HasGTFieldsCustomParse(new DictionaryConfig(new Dictionary<string, string>{
        { "Id", "roger" },
        { "OldId2", "rabbit" }
      }).ForInit());
      await Assert.That(obj.Id.Code).IsEqualTo("roger");
      await Assert.That(obj.Id2.Code).IsEqualTo("rabbit");
    }
    [Test]
    public async ValueTask TestCustomParsingAKAPrecedence()
    {
      var obj = new HasGTFieldsCustomParse(new DictionaryConfig(new Dictionary<string, string>{
        { "Id", "roger" },
        { "OldId2", "rabbit" },
        { "Id2", "beetroot" } // this should take precedence
      }).ForInit());
      await Assert.That(obj.Id.Code).IsEqualTo("roger");
      await Assert.That(obj.Id2.Code).IsEqualTo("beetroot");
    }
    
    [Test]
    public async ValueTask TestGTInstanceBasic()
    {
      var obj = new HasGTFieldsTInstance("roger", "rabbit");
      var s11n = obj.ParseS11n();
      var _obj = new HasGTFieldsTInstance(new DictionaryConfig(s11n).ForInit());
      await Assert.That(_obj.NewInstance.NewStringProperty).IsEqualTo("roger");
      await Assert.That(_obj.OtherInstance.NewStringProperty).IsEqualTo("rabbit");
    }
    [Test]
    public async ValueTask TestGTInstanceAKABaseline()
    {
      // baseline
      var s11n = new Dictionary<string, string>{
        { "NewInstance", "GTMH.S11n.UnitTests.Impl.InstanceType" },
        { "NewInstance.NewStringProperty", "roger" },
        { "OtherInstance", "GTMH.S11n.UnitTests.Impl.InstanceType" },
        { "OtherInstance.NewStringProperty", "rabbit" },
      };
      var _obj = new HasGTFieldsTInstance(new DictionaryConfig(s11n).ForInit());
      await Assert.That(_obj.NewInstance.NewStringProperty).IsEqualTo("roger");
      await Assert.That(_obj.OtherInstance.NewStringProperty).IsEqualTo("rabbit");
    }
    [Test]
    public async ValueTask TestGTInstanceAKAChildProperty()
    {
      // child class property name changes
      var s11n = new Dictionary<string, string>{
        { "NewInstance", "GTMH.S11n.UnitTests.Impl.InstanceType" },
        { "NewInstance.OldStringProperty", "roger" },
        { "OtherInstance", "GTMH.S11n.UnitTests.Impl.InstanceType" },
        { "OtherInstance.NewStringProperty", "rabbit" },
      };
      var _obj = new HasGTFieldsTInstance(new DictionaryConfig(s11n).ForInit());
      await Assert.That(_obj.NewInstance.NewStringProperty).IsEqualTo("roger");
      await Assert.That(_obj.OtherInstance.NewStringProperty).IsEqualTo("rabbit");
    }
    [Test]
    public async ValueTask TestGTInstanceAKAInstanceProperty()
    {
      var s11n = new Dictionary<string, string>{
        { "OldInstance", "GTMH.S11n.UnitTests.Impl.InstanceType" },
        { "OldInstance.NewStringProperty", "roger" },
        { "OtherInstance", "GTMH.S11n.UnitTests.Impl.InstanceType" },
        { "OtherInstance.NewStringProperty", "rabbit" },
      };
      var _obj = new HasGTFieldsTInstance(new DictionaryConfig(s11n).ForInit());
      await Assert.That(_obj.NewInstance.NewStringProperty).IsEqualTo("roger");
      await Assert.That(_obj.OtherInstance.NewStringProperty).IsEqualTo("rabbit");
    }
    [Test]
    public async ValueTask TestHasGTFieldsTInstanceRequired()
    {
      var obj = new HasGTFieldsTInstanceRequired(new InterfaceImplA("A") );
      await Assert.That(obj.Required).IsNotNull();
      await Assert.That(obj.Optional).IsNull();
      await Assert.That(obj.Required.Value).IsEqualTo("A_A");

      var s11n = obj.ParseS11n();
      var _obj = new HasGTFieldsTInstanceRequired(new DictionaryConfig(s11n).ForInit());
      await Assert.That(_obj.Required).IsNotNull();
      await Assert.That(_obj.Optional).IsNull();
      await Assert.That(_obj.Required.Value).IsEqualTo("A_A");
    }
    [Test]
    public async ValueTask TestGTInstanceCustomParse()
    {
      var obj = new HasGTFieldsTInstanceNonS8bleResource("roger");
      var s11n = obj.ParseS11n();
      var _obj = new HasGTFieldsTInstanceNonS8bleResource(new DictionaryConfig(s11n).ForInit());
      await Assert.That(_obj.Interface.S8ble).IsEqualTo("roger");
    }
    [Test]
    public async ValueTask TestGTInstanceCustomParseAKA()
    {
      var s11n = new Dictionary<string, string>{
        {"OldInterface", "GTMH.S11n.UnitTests.Impl.HaveNonS8bleResource" },
        {"OldInterface.S8ble", "roger" }
      };
      var _obj = new HasGTFieldsTInstanceNonS8bleResource(new DictionaryConfig(s11n).ForInit());
      await Assert.That(_obj.Interface.S8ble).IsEqualTo("roger");
    }
    [Test]
    public async ValueTask TestGTInstanceArray()
    {
      var obj = new HasGTFieldsTInstanceArray(new InterfaceImplA("roger"), new InterfaceImplB("rabbit"));
      await Assert.That(obj.Instances.Length).IsEqualTo(2);
      await Assert.That(obj.Instances[0].Value).IsEqualTo("roger_A");
      await Assert.That(obj.Instances[1].Value).IsEqualTo("rabbit_B");
      var s11n = obj.ParseS11n();
      var _obj = new HasGTFieldsTInstanceArray(new DictionaryConfig(s11n).ForInit());
      await Assert.That(_obj.Instances.Length).IsEqualTo(2);
      await Assert.That(_obj.Instances[0].Value).IsEqualTo("roger_A");
      await Assert.That(_obj.Instances[1].Value).IsEqualTo("rabbit_B");
    }
    [Test]
    public async ValueTask TestGTInstanceArrayEmpty()
    {
      var obj = new HasGTFieldsTInstanceArray();
      var s11n = obj.ParseS11n();
      var _obj = new HasGTFieldsTInstanceArray(new DictionaryConfig(s11n).ForInit());
      await Assert.That(_obj.Instances.Length).IsEqualTo(0);
    }
    [Test]
    public async ValueTask TestGTInstanceArrayNulls()
    {
      var obj = new HasGTFieldsTInstanceArray(new InterfaceImplA("roger"), null, new InterfaceImplB("rabbit"));
      var s11n = obj.ParseS11n();
      var _obj = new HasGTFieldsTInstanceArray(new DictionaryConfig(s11n).ForInit());
      await Assert.That(_obj.Instances.Length).IsEqualTo(3);
      await Assert.That(_obj.Instances[0].Value).IsEqualTo("roger_A");
      await Assert.That(_obj.Instances[1]).IsNull();
      await Assert.That(_obj.Instances[2].Value).IsEqualTo("rabbit_B");
    }
    [Test]
    public async ValueTask TestGTInstanceArrayAKA()
    {
      var s11n = new Dictionary<string, string>
      {
        { "OldInstances.Array-Length", "2"},
        { "OldInstances.0", "GTMH.S11n.UnitTests.Impl.InterfaceImplA" },
        { "OldInstances.0.Value", "roger_A" },
        { "OldInstances.1", "GTMH.S11n.UnitTests.Impl.InterfaceImplB" },
        { "OldInstances.1.Value", "rabbit_B"}
      };
      var _obj = new HasGTFieldsTInstanceArrayAKA(new DictionaryConfig(s11n).ForInit());
      await Assert.That(_obj.Instances.Length).IsEqualTo(2);
      await Assert.That(_obj.Instances[0].Value).IsEqualTo("roger_A");
      await Assert.That(_obj.Instances[1].Value).IsEqualTo("rabbit_B");
    }
    [Test]
    public async ValueTask TestGTInstanceArrayCustomPDP()
    {
      var obj = new HasGTFieldsTInstanceArrayCustomS11n(new InterfaceImplA("roger"), null, new InterfaceImplB("rabbit"));
      var s11n = obj.ParseS11n();
      var _obj = new HasGTFieldsTInstanceArrayCustomS11n(new DictionaryConfig(s11n).ForInit());
      await Assert.That(_obj.Instances.Length).IsEqualTo(3);
      await Assert.That(_obj.Instances[0].Value).IsEqualTo("roger_A");
      await Assert.That(_obj.Instances[1]).IsNull();
      await Assert.That(_obj.Instances[2].Value).IsEqualTo("rabbit_B");
    }
    [Test]
    public async ValueTask TestGTInstanceArrayCustomPDPAKA()
    {
      var s11n = new Dictionary<string, string>
      {
        { "OldInstances.Array-Length", "2"},
        { "OldInstances.0", "GTMH.S11n.UnitTests.Impl.InterfaceImplA" },
        { "OldInstances.0.Value", "roger_A" },
        { "OldInstances.1", "GTMH.S11n.UnitTests.Impl.InterfaceImplB" },
        { "OldInstances.1.Value", "rabbit_B"}
      };
      var _obj = new HasGTFieldsTInstanceArrayCustomS11nAKA(new DictionaryConfig(s11n).ForInit());
      await Assert.That(_obj.Instances.Length).IsEqualTo(2);
      await Assert.That(_obj.Instances[0].Value).IsEqualTo("roger_A");
      await Assert.That(_obj.Instances[1].Value).IsEqualTo("rabbit_B");
    }
    [Test]
    public async ValueTask TestGTInstanceArrayCustomPDPAKA3()
    {
      var s11n = new Dictionary<string, string>
      {
        { "OldInstances.Array-Length", "3"},
        { "OldInstances.0", "GTMH.S11n.UnitTests.Impl.InterfaceImplA" },
        { "OldInstances.0.Value", "roger_A" },
        { "OldInstances.2", "GTMH.S11n.UnitTests.Impl.InterfaceImplB" },
        { "OldInstances.2.Value", "rabbit_B"}
      };
      var _obj = new HasGTFieldsTInstanceArrayCustomS11nAKA(new DictionaryConfig(s11n).ForInit());
      await Assert.That(_obj.Instances.Length).IsEqualTo(3);
      await Assert.That(_obj.Instances[0].Value).IsEqualTo("roger_A");
      await Assert.That(_obj.Instances[1]).IsNull();
      await Assert.That(_obj.Instances[2].Value).IsEqualTo("rabbit_B");
    }
    [Test]
    public async ValueTask TestGTInstanceArrayRequired()
    {
      // that this compiles is the real test
      var obj = new HasGTFieldsTInstanceArrayRequired(new InterfaceImplA("roger"), new InterfaceImplB("rabbit"));
      var s11n = obj.ParseS11n();
      var _obj = new HasGTFieldsTInstanceArrayRequired(new DictionaryConfig(s11n).ForInit());
      await Assert.That(_obj.Instances.Length).IsEqualTo(2);
      await Assert.That(_obj.Instances[0].Value).IsEqualTo("roger_A");
      await Assert.That(_obj.Instances[1].Value).IsEqualTo("rabbit_B");
    }
  }
}
