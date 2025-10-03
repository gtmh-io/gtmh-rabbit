using System.Runtime.Serialization;

namespace GTMH.S11n
{
  public class DCS11n
  {
    public static string XML<T>(T a_Value)
    {
      // TODO there are some xml writer settings we'll need to stop shit blowing up
      using(var str = new MemoryStream())
      {
        using(var writer = System.Xml.XmlWriter.Create(str, new System.Xml.XmlWriterSettings { Indent = true }))
        {
          var ser = new DataContractSerializer(typeof(T));
          ser.WriteObject(writer, a_Value);
          writer.Flush();
          return System.Text.Encoding.UTF8.GetString(str.ToArray());
        }
      }
    }
    [System.Diagnostics.DebuggerStepThrough]
    public static T FromXML<T>(string a_XML)
    {
      using(var mem = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(a_XML)))
      {
        using(var reader = System.Xml.XmlReader.Create(mem, new System.Xml.XmlReaderSettings { }))
        {
          var ser = new DataContractSerializer(typeof(T));
#pragma warning disable 8600,8603
          // you asked for it - fail hard
          return (T)ser.ReadObject(reader);
#pragma warning restore
        }
      }
    }
  }
}
