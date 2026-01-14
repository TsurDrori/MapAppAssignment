namespace MapServer.Domain.ValueObjects;

/// <summary>
/// Represents a geographic coordinate (latitude/longitude pair).
/// Immutable value object - coordinates are identity-less.
/// </summary>
public record GeoCoordinate(double Latitude, double Longitude);
