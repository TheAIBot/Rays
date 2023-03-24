using System.Numerics;

namespace Rays.Polygons;

public readonly record struct Ray(Vector2 Start, Vector2 Direction)
{
    public static List<Line> GetRayPath(Ray ray, int bounceCount, List<Wall> walls)
    {
        List<Vector2> points = new List<Vector2>
        {
            ray.Start
        };
        List<Wall>? previousWalls = null;
        for (int i = 0; i < bounceCount; i++)
        {
            List<Wall> wallsHit = GetClosestCollidingWalls(ray, walls.Where(x => previousWalls == null || !previousWalls.Contains(x)).ToArray());
            if (wallsHit.Count == 0)
            {
                break;
            }
            previousWalls = wallsHit;

            Ray? reflectedRay = wallsHit[0].TryReflectRay(ray);
            if (!reflectedRay.HasValue)
            {
                throw new Exception("");
            }
            foreach (var wall in wallsHit.Skip(1))
            {
                reflectedRay = new Ray(reflectedRay.Value.Start, wall.GetReflectedVector(reflectedRay.Value));
            }
            ray = reflectedRay.Value;
            points.Add(ray.Start);
        }

        List<Line> lines = new List<Line>();
        for (int i = 0; i < points.Count - 1; i++)
        {
            lines.Add(new Line(points[i], points[i + 1]));
        }

        return lines;
    }

    private static List<Wall> GetClosestCollidingWalls(Ray ray, Wall[] walls)
    {
        var wallsCollidingWith = new List<Wall>();
        float bestDistance = float.MaxValue;
        for (int i = 0; i < walls.Length; i++)
        {
            if (!walls[i].IsColliding(ray, out Vector2? collisionPoint))
            {
                continue;
            }

            float distance = Vector2.Distance(ray.Start, collisionPoint.Value);
            float differenceInDistance = distance - bestDistance;
            const float acceptableError = 0.02f;
            if (differenceInDistance < -acceptableError)
            {
                wallsCollidingWith.Clear();

            }
            else if (differenceInDistance > acceptableError)
            {
                continue;
            }

            wallsCollidingWith.Add(walls[i]);
            bestDistance = distance;
        }

        return wallsCollidingWith;
    }
}
