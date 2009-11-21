﻿using System;
using QuickRoute.BusinessEntities.Numeric;

namespace QuickRoute.BusinessEntities.RouteProperties
{
  public class CircleTimeAtEndOfSpan : RouteSpanProperty
  {
    public CircleTimeAtEndOfSpan(Session session, ParameterizedLocation start, ParameterizedLocation end)
      : base(session, start, end)
    {
      DistanceThreshold = 35; // TODO: set dynamically
    }

    public CircleTimeAtEndOfSpan(Session session, RouteLocations locations)
      : base(session, locations)
    {
      DistanceThreshold = 35; // TODO: set dynamically
    }

    protected double DistanceThreshold { get; set; }

    protected override void Calculate()
    {
      var cachedProperty = GetFromCache();
      if (cachedProperty != null)
      {
        value = cachedProperty.Value;
        return;
      }

      var currentLongLat = Session.Route.GetLocationFromParameterizedLocation(End);
      var lastPL = new ParameterizedLocation(End);
      ParameterizedLocation thisPL;
      ParameterizedLocation thresholdPL;
      double lastNodeDistance = 0;
      double thisNodeDistance = 0;
      while (true)
      {
        thisPL = Session.Route.GetNextPLNode(lastPL, ParameterizedLocation.Direction.Backward);
        if (thisPL == null || Session.Route.IsFirstPLInSegment(thisPL) || thisPL < Start) break;
        var longLat = Session.Route.GetLocationFromParameterizedLocation(thisPL);
        thisNodeDistance = LinearAlgebraUtil.DistancePointToPointLongLat(longLat, currentLongLat);
        if (thisNodeDistance > DistanceThreshold) break;
        lastPL = thisPL;
        lastNodeDistance = thisNodeDistance;
      }
      if (thisPL == null) thisPL = new ParameterizedLocation(Start.SegmentIndex, 0);
      if (Session.Route.IsFirstPLInSegment(thisPL))
      {
        thresholdPL = thisPL;
      }
      else if (thisPL < Start)
      {
        thresholdPL = new ParameterizedLocation(Start);
      }
      else
      {
        var t = thisNodeDistance - lastNodeDistance == 0 ? 1 : (DistanceThreshold - lastNodeDistance) / (thisNodeDistance - lastNodeDistance);
        thresholdPL = new ParameterizedLocation(thisPL.SegmentIndex, lastPL.Value - t * (lastPL.Value - thisPL.Value));
      }
      value = Session.Route.GetTimeFromParameterizedLocation(End) - Session.Route.GetTimeFromParameterizedLocation(thresholdPL);
      AddToCache();
    }

    public override Type GetRouteFromStartPropertyType()
    {
      return typeof(CircleTimeAtEndOfSpanFromStart);
    }

    public override int CompareTo(object obj)
    {
      return ((TimeSpan)Value).CompareTo((TimeSpan)(((RouteProperty)obj).Value));
    }

    protected override string ValueToString(object v, string format, IFormatProvider provider)
    {
      if (provider == null)
      {
        var tc = new TimeConverter(TimeConverter.TimeConverterType.ElapsedTime);
        return tc.ToString((TimeSpan)v);
      }
      return string.Format(provider, format ?? "{0}", (TimeSpan)v);
    }

    public override string MaxWidthString
    {
      get { return ValueToString(new TimeSpan(29, 59, 59)); }
    }

    public override bool ContainsValue
    {
      get { return true; }
    }
  }

  public class CircleTimeAtEndOfSpanFromStart : RouteFromStartProperty
  {
    public CircleTimeAtEndOfSpanFromStart(Session session, ParameterizedLocation location)
      : base(session, location)
    {
      DistanceThreshold = 35; // TODO: set dynamically
    }

    public CircleTimeAtEndOfSpanFromStart(Session session, RouteLocations locations)
      : base(session, locations)
    {
      DistanceThreshold = 35; // TODO: set dynamically
    }

    protected double DistanceThreshold { get; set; }

    protected override void Calculate()
    {
      var cachedProperty = GetFromCache();
      if (cachedProperty != null)
      {
        value = cachedProperty.Value;
        return;
      }

      var time = Session.Route.GetTimeFromParameterizedLocation(Location);
      var sum = new TimeSpan();
      for (var i = 0; i < Session.Laps.Count; i++)
      {
        if (Session.Laps[i].LapType != LapType.Start)
        {
          if (time >= Session.Laps[i].Time)
          {
            sum += (TimeSpan)new CircleTimeAtEndOfSpan(
                               Session,
                               Session.Route.GetParameterizedLocationFromTime(Session.Laps[i - 1].Time),
                               Session.Route.GetParameterizedLocationFromTime(Session.Laps[i].Time)
                               ).Value;
          }
        }
      }
      value = sum;

      AddToCache();
    }

    public override int CompareTo(object obj)
    {
      return ((TimeSpan)Value).CompareTo((TimeSpan)(((RouteProperty)obj).Value));
    }

    protected override string ValueToString(object v, string format, IFormatProvider provider)
    {
      if (provider == null)
      {
        var tc = new TimeConverter(TimeConverter.TimeConverterType.ElapsedTime);
        return tc.ToString((TimeSpan)v);
      }
      return string.Format(provider, format ?? "{0}", (TimeSpan)v);
    }

    public override string MaxWidthString
    {
      get { return ValueToString(new TimeSpan(29, 59, 59)); }
    }

    public override bool ContainsValue
    {
      get { return true; }
    }
  }
}