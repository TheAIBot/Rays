using Rays.Polygons;
using System.Numerics;
using Rectangle = Rays.Polygons.Rectangle;

namespace Rays.Scenes;

public sealed class SpinningRectangleFactory : I2DSceneFactory
{
    public I2DScene Create(IPolygonDrawer polygonDrawer)
    {
        var rectangle = new Rectangle(new Vector2(10, 10), new Vector2(10, 0), new Vector2(0, 10));
        var bottomWall = new Wall(new Line(new Vector2(0, 0), new Vector2(polygonDrawer.Size.X - 1, 0)), new Vector2(0, 1));
        var topWall = new Wall(new Line(new Vector2(0, polygonDrawer.Size.Y - 1), new Vector2(polygonDrawer.Size.X - 1, polygonDrawer.Size.Y - 1)), new Vector2(0, -1));
        var leftWall = new Wall(new Line(new Vector2(0, 0), new Vector2(0, polygonDrawer.Size.Y - 1)), new Vector2(1, 0));
        var rightWall = new Wall(new Line(new Vector2(polygonDrawer.Size.X - 1, 0), new Vector2(polygonDrawer.Size.X - 1, polygonDrawer.Size.Y - 1)), new Vector2(-1, 0));

        var walls = new List<Wall>
        {
            bottomWall,
            topWall,
            leftWall,
            rightWall
        };
        return new SpinningRectangle(polygonDrawer, walls, rectangle);
    }
}