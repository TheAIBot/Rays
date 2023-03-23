using System.Diagnostics.CodeAnalysis;
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
        //return GetIntersection(ray.Start, ray.Direction, Line.Start, Line.End);
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
            Console.WriteLine("null");
            return null;
        }

        float t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denominator;
        float u = ((x1 - x3) * (y1 - y2) - (y1 - y3) * (x1 - x2)) / denominator;

        // see https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection#Given_two_points_on_each_line_segment
        const float minAcceptableWallIntersectionFactor = -0.01f;
        const float maxAcceptableWallIntersectionFactor = 1.01f;
        if (t < 0 || u < minAcceptableWallIntersectionFactor || u > maxAcceptableWallIntersectionFactor)
        {
            Console.WriteLine("null");
            return null;
        }

        Console.WriteLine(Line.Start + u * (Line.End - Line.Start));
        return Line.Start + u * (Line.End - Line.Start);
    }

    public static Vector2? GetIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float x1 = p1.X;
        float y1 = p1.Y;
        float x2 = p1.X + p2.X;
        float y2 = p1.Y + p2.Y;
        float x3 = p3.X;
        float y3 = p3.Y;
        float x4 = p4.X;
        float y4 = p4.Y;

        float denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

        if (MathF.Abs(denominator) < float.Epsilon)
        {
            // Lines are parallel or coincident
            return null;
        }

        //// see https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection#Given_two_points_on_each_line_segment
        float u = ((x1 - x3) * (y1 - y2) - (y1 - y3) * (x1 - x2)) / denominator;
        float t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denominator;
        const float minAcceptableWallIntersectionFactor = -0.01f;
        const float maxAcceptableWallIntersectionFactor = 1.01f;
        if (t < minAcceptableWallIntersectionFactor || u < minAcceptableWallIntersectionFactor || u > maxAcceptableWallIntersectionFactor)
        {
            Console.WriteLine("null");
            return null;
        }

        return p3 + u * (p4 - p3);
    }

    public readonly Vector2? TryGetIntersectionPointVectorized(Ray ray)
    {
        Vector2 rayStart = ray.Start;
        Vector2 rayDirection = ray.Direction;
        Vector2 lineStart = Line.Start;
        Vector2 lineEnd = Line.End;

        Vector2 deltaLine = lineEnd - lineStart;
        Vector2 deltaRay = rayDirection;

        float denominator = deltaRay.X * deltaLine.Y - deltaRay.Y * deltaLine.X;
        if (MathF.Abs(denominator) < 0.001f)
        {
            // The lines are parallel
            Console.WriteLine("null");
            return null;
        }

        Vector2 deltaStart = rayStart - lineStart;

        float t = Vector2.Dot(deltaStart, new Vector2(deltaLine.Y, -deltaLine.X)) / denominator;
        float u = Vector2.Dot(deltaStart, new Vector2(-deltaRay.Y, deltaRay.X)) / denominator;

        // see https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection#Given_two_points_on_each_line_segment
        const float minAcceptableWallIntersectionFactor = -0.01f;
        const float maxAcceptableWallIntersectionFactor = 1.01f;
        if (t < 0 || u < minAcceptableWallIntersectionFactor || u > maxAcceptableWallIntersectionFactor)
        {
            Console.WriteLine("null");
            return null;
        }

        Console.WriteLine(rayStart + t * deltaRay);
        return rayStart + t * deltaRay;
    }

    //public readonly Vector2? TryGetIntersectionPointVectorized(Ray ray)
    //{
    //    Vector2 rayDirection = ray.Direction;
    //    Vector2 wallDirection = Line.Direction;
    //    Vector2 rayStartToWallStart = ray.Start - Line.Start;
    //    Vector2 swappedRayStartToWallStart = new Vector2(rayStartToWallStart.Y, rayStartToWallStart.X);

    //    float denominator = rayDirection.X * wallDirection.Y - rayDirection.Y * wallDirection.X;
    //    if (Math.Abs(denominator) < float.Epsilon)
    //    {
    //        return null;
    //    }

    //    Vector2 wallStartToEnd = Line.Start - Line.End;
    //    Vector2 rayTComponents = swappedRayStartToWallStart * wallStartToEnd;
    //    Vector2 wallUComponents = swappedRayStartToWallStart * rayDirection;
    //    float rayIntersectionFactor = (rayTComponents.Y - rayTComponents.X) / denominator;
    //    float wallIntersectionFactor = (wallUComponents.Y - wallUComponents.X) / denominator;
    //    if (wallIntersectionFactor >= 0 && wallIntersectionFactor <= 1)
    //    {
    //        return ray.Start + rayIntersectionFactor * ray.Direction;
    //    }

    //    return null;
    //}

    public readonly Vector2 GetReflectedVector(Ray ray)
    {
        return Vector2.Reflect(ray.Direction, Normal);
    }
}
