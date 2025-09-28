using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.S11n.FieldTypes
{
  public interface IFieldType
  {
    void WriteGather(Code code);
    void WriteInitialisation(Code code);
  }
}
