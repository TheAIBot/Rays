using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Rays.GeometryLoaders.Materials;

public sealed class Material
{
    public Vector3? AmbientColor { get; internal set; } // Ka
    public Vector3? DiffuseColor { get; internal set; } // Kd
    public Vector3? SpecularColor { get; internal set; } // Ks
    public float? SpecularExponent { get; internal set; } // Ns
    public float? Transparency { get; internal set; } // Tr
    public Vector3? TransmissinFilterColor { get; internal set; } // Tf
    public float? OpticalDensity { get; internal set; } // Ni
    public string? AmbientTextureMapFileName { get; internal set; } // map_Ka
    public string? DiffuseTextureMapFileName { get; internal set; } // map_kd
    public string? SpecularTextureMapFileName { get; internal set; } // map_Ks
    public string? SpecularHighlightTextureMapFileName { get; internal set; } // map_Ns
    public string? AlphaTextureMapFileName { get; internal set; } // map_d
    public string? BumpMapTextureFileName { get; internal set; } // map_bump or bump
    public string? DisplacementTextureMapFileName { get; internal set; } // disp
    public string? StencilDecalTextureMapFileName { get; internal set; } // decal
}
