namespace Zinnia.Data.Type.Transformation.Conversion
{
  using UnityEngine;
  using UnityEngine.Events;
  using System;
  using Malimbe.XmlDocumentationAttribute;
  using Malimbe.PropertySerializationAttribute;
  using Zinnia.Data.Attribute;

  public class MyFloatToBoolean : FloatToBoolean
  {
    public FloatRange GetActivationRange() { return PositiveBounds; }
    public void SetActivationRange(FloatRange newRange) { PositiveBounds = newRange; }
  }
}
