using System;
using System.Collections.Generic;
using System.Text;

namespace GTMH.Security
{
  public interface ISecretProvider
  {
    string Secret { get; }
  }
}
