﻿using System;

namespace QuickRoute.BusinessEntities.RouteProperties
{
  public class ImageX : RouteMomentaneousProperty
  {
    public ImageX(Session session, RouteLocations locations)
      : base(session, locations)
    {
    }

    public ImageX(Session session, ParameterizedLocation location)
      : base(session, location)
    {
    }

    protected override void Calculate()
    {
      var cachedProperty = GetFromCache();
      if (cachedProperty != null)
      {
        value = cachedProperty.Value;
        return;
      }
      value = Session.AdjustedRoute.GetLocationFromParameterizedLocation(Locations.Location).X;
      AddToCache();
    }

    protected override string ValueToString(object v, string format, IFormatProvider provider)
    {
      return string.Format(provider, format ?? "{0:n0}", v);
    }

    public override string MaxWidthString
    {
      get { return ValueToString(9999); }
    }

    public override bool ContainsValue
    {
      get { return true; }
    }
  }
}