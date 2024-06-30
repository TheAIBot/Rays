using System.Numerics;

namespace Rays._3D;

public sealed class SurfaceAreaHeuristicNodeClusterBuilder : INodeClusterBuilder
{
    public Node Create(ISubDividableTriangleSet[] texturedTriangleSets)
    {
        static float Heuristic(AxisAlignedBox box, int triangleCount) => (box.Size.X * box.Size.Y +
                                                                          box.Size.X * box.Size.Z +
                                                                          box.Size.Y * box.Size.Z) * triangleCount;

        static IEnumerable<Triangle> CountTriangles(IEnumerable<Triangle> triangles, Counter counter)
        {
            foreach (var triangle in triangles)
            {
                counter.Count++;
                yield return triangle;
            }
        }

        SubBoxConfiguration[] defaultSizeBubBoxConfigurations = GetDefaultSizeSubBoxConfigurations().ToArray();

        AxisAlignedBox rootBox = AxisAlignedBox.GetBoundingBoxForTriangles(texturedTriangleSets.SelectMany(x => x.GetTriangles()));
        var root = new Node(texturedTriangleSets, new List<Node>());
        var nodesToGoThrough = new Stack<(Node Node, float Cost, AxisAlignedBox Box)>();
        nodesToGoThrough.Push((root, float.MaxValue, rootBox));

        _ = Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync())
            {
                Console.WriteLine(nodesToGoThrough.Count);
            }
        });

        while (nodesToGoThrough.Count > 0)
        {
            (var node, float cost, var boundingBox) = nodesToGoThrough.Pop();

            int trianglesInParent = node.TexturedTriangleSets.Sum(x => x.Triangles.Length);
            var splitOptions = new List<(SubBoxConfiguration Configuration, float Cost, List<(AxisAlignedBox SubBox, int TriangleCount, AxisAlignedBox FullBox)> SubNodesAndBoxes)>();
            foreach (SubBoxConfiguration defaultSizeBoxConfiguration in defaultSizeBubBoxConfigurations)
            {
                SubBoxConfiguration boxConfiguration = defaultSizeBoxConfiguration.GetRelativeToBox(boundingBox);
                var wantToSplitUp = new List<(AxisAlignedBox SubBox, int TriangleCount, AxisAlignedBox FullBox)>();
                foreach (AxisAlignedBox subBox in boxConfiguration.Boxes)
                {
                    Counter triangleCounter = new Counter();
                    AxisAlignedBox fullSizedBox = AxisAlignedBox.GetBoundingBoxForTriangles(CountTriangles(node.TexturedTriangleSets
                                                                                                .SelectMany(x => x.Triangles)
                                                                                                .Where(x => subBox.CollidesWith(x.Center)), triangleCounter));
                    if (triangleCounter.Count == 0)
                    {
                        continue;
                    }

                    wantToSplitUp.Add((subBox, triangleCounter.Count, fullSizedBox));
                }

                if (wantToSplitUp.Count == 0)
                {
                    continue;
                }

                splitOptions.Add((boxConfiguration, wantToSplitUp.Sum(x => Heuristic(x.FullBox, x.TriangleCount)), wantToSplitUp));
            }

            if (splitOptions.Count == 0)
            {
                continue;
            }

            var bestConfiguration = splitOptions.MinBy(x => x.Cost);
            if (bestConfiguration.Cost >= cost)
            {
                continue;
            }

            Node[] subNodes = bestConfiguration.SubNodesAndBoxes.Select(SubBoxes => new Node(node.TexturedTriangleSets
                                                                                                 .Select(x => x.SubCopy(y => SubBoxes.SubBox.CollidesWith(y.Center)))
                                                                                                 .Where(x => x.Triangles.Length > 0)
                                                                                                 .ToArray(),
                                                                                             new List<Node>())).ToArray();
            node.Children.AddRange(subNodes);
            for (int i = 0; i < subNodes.Length; i++)
            {
                Node nodeToSplitUp = subNodes[i];
                AxisAlignedBox subBox = bestConfiguration.SubNodesAndBoxes[i].FullBox;

                const float minTriangleReductionToAllowSplitNode = 0.7f;
                const int maxTrianglesBeforeConsiderSplit = 5;
                int trianglesInSubBox = nodeToSplitUp.TexturedTriangleSets.Sum(x => x.Triangles.Length);
                if (trianglesInSubBox <= maxTrianglesBeforeConsiderSplit &&
                    trianglesInSubBox > trianglesInParent * minTriangleReductionToAllowSplitNode)
                {
                    continue;
                }
                nodesToGoThrough.Push((nodeToSplitUp, Heuristic(subBox, nodeToSplitUp.TexturedTriangleSets.Sum(x => x.Triangles.Length)), subBox));
            }
        }

        return root;
    }

    private static IEnumerable<SubBoxConfiguration> GetDefaultSizeSubBoxConfigurations()
    {
        float[] axisSplits = [0.25f, 0.50f, 0.75f];
        foreach (var subBoxConfiguration in Get2SubBoxConfigurations(axisSplits))
        {
            yield return subBoxConfiguration;
        }

        foreach (var subBoxConfiguration in Get4SubBoxConfigurations(axisSplits))
        {
            yield return subBoxConfiguration;
        }

        foreach (var subBoxConfiguration in Get8SubBoxConfigurations(axisSplits))
        {
            yield return subBoxConfiguration;
        }
    }

    private static IEnumerable<SubBoxConfiguration> Get2SubBoxConfigurations(float[] axisSplits)
    {
        const int axisCount = 3;
        for (int axisIndex = 0; axisIndex < axisCount; axisIndex++)
        {
            foreach (var axisSPlit in axisSplits)
            {
                var box1Start = new Vector4(0, 0, 0, 0);
                var box1End = new Vector4(1, 1, 1, 0);
                box1End[axisIndex] = axisSPlit;
                var box1 = new AxisAlignedBox(box1Start, box1End);

                var box2Start = new Vector4(0, 0, 0, 0);
                var box2End = new Vector4(1, 1, 1, 0);
                box2Start[axisIndex] = axisSPlit;
                var box2 = new AxisAlignedBox(box2Start, box2End);

                AxisAlignedBox[] subBoxes = [
                    MakeSlightlyLarger(box1),
                    MakeSlightlyLarger(box2)
                ];

                yield return new SubBoxConfiguration(subBoxes);
            }
        }
    }

    private static IEnumerable<SubBoxConfiguration> Get4SubBoxConfigurations(float[] axisSplits)
    {
        const int axisCount = 3;
        for (int firstAxisIndex = 0; firstAxisIndex < axisCount - 1; firstAxisIndex++)
        {
            for (int secondAxisIndex = firstAxisIndex + 1; secondAxisIndex < axisCount; secondAxisIndex++)
            {
                foreach (var firstAxisSplit in axisSplits)
                {
                    foreach (var secondAxisSplit in axisSplits)
                    {
                        var box1Start = new Vector4(0, 0, 0, 0);
                        var box1End = new Vector4(1, 1, 1, 0);
                        box1End[firstAxisIndex] = firstAxisSplit;
                        box1End[secondAxisIndex] = secondAxisSplit;
                        var box1 = new AxisAlignedBox(box1Start, box1End);

                        var box2Start = new Vector4(0, 0, 0, 0);
                        var box2End = new Vector4(1, 1, 1, 0);
                        box2Start[firstAxisIndex] = firstAxisSplit;
                        box2End[secondAxisIndex] = secondAxisSplit;
                        var box2 = new AxisAlignedBox(box2Start, box2End);

                        var box3Start = new Vector4(0, 0, 0, 0);
                        var box3End = new Vector4(1, 1, 1, 0);
                        box3End[firstAxisIndex] = firstAxisSplit;
                        box3Start[secondAxisIndex] = secondAxisSplit;
                        var box3 = new AxisAlignedBox(box3Start, box3End);

                        var box4Start = new Vector4(0, 0, 0, 0);
                        var box4End = new Vector4(1, 1, 1, 0);
                        box4Start[firstAxisIndex] = firstAxisSplit;
                        box4Start[secondAxisIndex] = secondAxisSplit;
                        var box4 = new AxisAlignedBox(box4Start, box4End);

                        AxisAlignedBox[] subBoxes = [
                            MakeSlightlyLarger(box1),
                            MakeSlightlyLarger(box2),
                            MakeSlightlyLarger(box3),
                            MakeSlightlyLarger(box4)
                        ];

                        yield return new SubBoxConfiguration(subBoxes);
                    }
                }
            }
        }
    }

    private static IEnumerable<SubBoxConfiguration> Get8SubBoxConfigurations(float[] axisSplits)
    {
        static AxisAlignedBox CreateBox(float xAxisSplit, float yAxisSplit, float zAxisSplit, float xOffset, float yOffset, float zOffset)
        {

            var startOffset = new Vector4(xOffset, yOffset, zOffset, 0);
            var boxSize = new Vector4(xAxisSplit, yAxisSplit, zAxisSplit, 0);
            var boxEnd = startOffset + boxSize;
            return MakeSlightlyLarger(new AxisAlignedBox(startOffset, boxEnd));
        }

        foreach (var xSplit in axisSplits)
        {
            foreach (var ySplit in axisSplits)
            {
                foreach (var zSplit in axisSplits)
                {
                    AxisAlignedBox[] subBoxes = [
                        CreateBox(xSplit, ySplit, zSplit, 0, 0, 0),
                        CreateBox(1.0f - xSplit, ySplit, zSplit, xSplit, 0, 0),
                        CreateBox(xSplit, 1.0f - ySplit, zSplit, 0, ySplit, 0),
                        CreateBox(1.0f - xSplit, 1.0f - ySplit, zSplit, xSplit, ySplit, 0),
                        CreateBox(xSplit, ySplit, 1.0f - zSplit, 0, 0, zSplit),
                        CreateBox(1.0f - xSplit, ySplit, 1.0f - zSplit, xSplit, 0, zSplit),
                        CreateBox(xSplit, 1.0f - ySplit, 1.0f - zSplit, 0, ySplit, zSplit),
                        CreateBox(1.0f - xSplit, 1.0f - ySplit, 1.0f - zSplit, xSplit, ySplit, zSplit)
                    ];

                    yield return new SubBoxConfiguration(subBoxes);
                }
            }
        }
    }

    private static AxisAlignedBox MakeSlightlyLarger(AxisAlignedBox box)
    {
        // There is rare cases of sub boxes not covering the entire area that the
        // large box did because of floating point error which is why the boxes will
        // overlap slightly.
        var smallOverlap = new Vector4(0.00001f);
        return new AxisAlignedBox(box.MinPosition - smallOverlap, box.MaxPosition + smallOverlap);
    }

    private sealed class Counter
    {
        public int Count { get; set; }
    }

    private readonly record struct Vector3Int(int X, int Y, int Z);

    private readonly record struct SubBoxConfiguration(AxisAlignedBox[] Boxes)
    {
        public SubBoxConfiguration GetRelativeToBox(AxisAlignedBox box)
        {
            return new SubBoxConfiguration(Boxes.Select(x => new AxisAlignedBox(box.MinPosition + x.MinPosition * box.Size,
                                                                                box.MinPosition + x.MaxPosition * box.Size))
                                                .ToArray());
        }
    }
}
