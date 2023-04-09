﻿using Rays.GeometryLoaders.Materials;

namespace Rays.GeometryLoaders.Geometry;

public sealed record GeometryModel(string Name, Face[] Faces, Material Material);
