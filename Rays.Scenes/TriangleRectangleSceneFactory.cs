using Rays.Polygons;
using System.Numerics;
using Rectangle = Rays.Polygons.Rectangle;

namespace Rays.Scenes;

public sealed class TriangleRectangleSceneFactory : ISceneFactory
{
    public IScene Create(IPolygonDrawer polygonDrawer)
    {
        var triangle = new Triangle(new Vector2(20, 20), new Vector2(40, 40), new Vector2(60, 20));
        var rectangle1 = new Rectangle(new Vector2(80, 10), new Vector2(20, 0), new Vector2(0, 20));
        var rectangle2 = new Rectangle(new Vector2(30, 60), new Vector2(20, 0), new Vector2(0, 10));

        var bottomWall = new Wall(new Line(new Vector2(0, 0), new Vector2(polygonDrawer.Size.X - 1, 0)), new Vector2(0, 1));
        var topWall = new Wall(new Line(new Vector2(0, polygonDrawer.Size.Y - 1), new Vector2(polygonDrawer.Size.X - 1, polygonDrawer.Size.Y - 1)), new Vector2(0, -1));
        var leftWall = new Wall(new Line(new Vector2(0, 0), new Vector2(0, polygonDrawer.Size.Y - 1)), new Vector2(1, 0));
        var rightWall = new Wall(new Line(new Vector2(polygonDrawer.Size.X - 1, 0), new Vector2(polygonDrawer.Size.X - 1, polygonDrawer.Size.Y - 1)), new Vector2(-1, 0));

        var walls = new List<Wall>
        {
            bottomWall,
            topWall,
            leftWall,
            rightWall,
        };
        walls.AddRange(triangle.GetAsWalls());
        walls.AddRange(rectangle1.GetAsWalls());
        walls.AddRange(rectangle2.GetAsWalls());

        return new OnlyStaticContentScene(polygonDrawer, walls);
    }
}