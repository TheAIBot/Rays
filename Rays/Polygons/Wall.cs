﻿using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Rays.Polygons;

public readonly record struct Wall(Line Line, Vector2 Normal)
{
    public readonly Ray? TryReflectRay(Ray ray)
    {
        Vector2? reflectedRayStart = TryGetIntersectionPointScalar(ray);
        if (!reflectedRayStart.HasValue)
        {
            return null;
        }

        Vector2 reflectedRayDirection = GetReflectedVector(ray);

        return new Ray(reflectedRayStart.Value, reflectedRayDirection);
    }

    public readonly bool IsColliding(Ray ray, [NotNullWhen(true)] out Vector2? collisionPosition)
    {
        if (Vector2.Dot(ray.Direction, Normal) > 0)
        {
            collisionPosition = null;
            return false;
        }
        collisionPosition = TryGetIntersectionPointScalar(ray);
        return collisionPosition.HasValue;
    }

    public readonly Vector2? TryGetIntersectionPointScalar(Ray ray)
    {
        float x1 = ray.Start.X;
        float y1 = ray.Start.Y;
        float x2 = ray.Start.X + ray.Direction.X;
        float y2 = ray.Start.Y + ray.Direction.Y;
        float x3 = Line.Start.X;
        float y3 = Line.Start.Y;
        float x4 = Line.End.X;
        float y4 = Line.End.Y;

        float denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        if (Math.Abs(denominator) < float.Epsilon)
        {
            // The lines are parallel
            return null;
        }

        float t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denominator;
        float u = ((x1 - x3) * (y1 - y2) - (y1 - y3) * (x1 - x2)) / denominator;

        // see https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection#Given_two_points_on_each_line_segment
        const float minAcceptableWallIntersectionFactor = -0.01f;
        const float maxAcceptableWallIntersectionFactor = 1.01f;
        if (t < 0 || u < minAcceptableWallIntersectionFactor || u > maxAcceptableWallIntersectionFactor)
        {
            return null;
        }

        return Line.Start + u * (Line.End - Line.Start);
    }

    public readonly Vector2 GetReflectedVector(Ray ray)
    {
        return Vector2.Reflect(ray.Direction, Normal);
    }
}
