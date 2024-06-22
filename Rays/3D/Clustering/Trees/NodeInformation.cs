using System.Runtime.CompilerServices;

namespace Rays._3D;

public readonly struct NodeInformation
{
    private const uint _isLeafNodeMask = 0b10000000_00000000_00000000_00000000;
    private const int _isLeafNodeShift = 31;
    private const uint _childCountMask = 0b01111111_00000000_00000000_00000000;
    private const int _childCountShift = 24;
    private const uint _childStartMask = 0b00000000_11111111_11111111_11111111;
    private const int _childStartShift = 0;
    private const uint _trianglesMask_ = 0b01111111_11111111_11111111_11111111;
    private const int _trianglesShift_ = 0;
    private readonly uint _value;

    public bool IsLeafNode => ((_value & _isLeafNodeMask) >> _isLeafNodeShift) == 1;

    public int ChildStartIndex => (int)((_value & _childStartMask) >> _childStartShift);

    public int ChildCount => (int)((_value & _childCountMask) >> _childCountShift);

    public int TexturedTriangleIndex => (int)((_value & _trianglesMask_) >> _trianglesShift_);

    public static NodeInformation CreateLeafNode(int texturedTrianglesIndex)
    {
        return new NodeInformation(true, 0, 0, texturedTrianglesIndex);
    }

    public static NodeInformation CreateParentNode(int childStartIndex, int childCount)
    {
        return new NodeInformation(false, childStartIndex, childCount, 0);
    }

    private NodeInformation(bool isLeafNode, int childStartIndex, int childCount, int texturedTrianglesIndex)
    {
        WithinBounds(childStartIndex, _childStartMask, _childStartShift);
        WithinBounds(childCount, _childCountMask, _childCountShift);
        WithinBounds(texturedTrianglesIndex, _trianglesMask_, _trianglesShift_);

        _value = (isLeafNode ? 1u : 0u) << _isLeafNodeShift;
        _value |= ((uint)childCount << _childCountShift) & _childCountMask;
        _value |= ((uint)childStartIndex << _childStartShift) & _childStartMask;
        _value |= ((uint)texturedTrianglesIndex << _trianglesShift_) & _trianglesMask_;
    }

    private static void WithinBounds(int value, uint mask, int shift, [CallerArgumentExpression(nameof(value))] string? name = null)
    {
        int maxValue = (int)(mask >> shift);
        if (value > maxValue || value < 0)
        {
            throw new ArgumentOutOfRangeException(name, $"Value must be within 0 to {maxValue} but had value {value}.");
        }
    }
}