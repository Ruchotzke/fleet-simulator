

// L/R: left/right (0,1) (x)
// U/D: up/down (1,0) (y)
// B/F: back/front (0,1) (z)
using UnityEngine;

public enum Octant
{
    LDB,
    LDF,
    LUB,
    LUF,
    RDB,
    RDF,
    RUB,
    RUF
}

public static class OctantExtensions
{
    public static readonly Octant[] Octants = new Octant[] { Octant.LDB, Octant.LDF, Octant.LUB, Octant.LUF, Octant.RDB, Octant.RDF, Octant.RUB, Octant.RUF };

    public static byte GetBitCode(this Octant oct)
    {
        switch (oct)
        {
            case Octant.LDB:
                return 0b000;
            case Octant.LDF:
                return 0b001;
            case Octant.LUB:
                return 0b010;
            case Octant.LUF:
                return 0b011;
            case Octant.RDB:
                return 0b100;
            case Octant.RDF:
                return 0b101;
            case Octant.RUB:
                return 0b110;
            case Octant.RUF:
                return 0b111;
            default:
                return 0b000;
        }
    }

    public static Vector3 GetDirection(this Octant oct)
    {
        switch (oct)
        {
            case Octant.LDB:
                return new Vector3(-1f, -1f, -1f);
            case Octant.LDF:
                return new Vector3(-1f, -1f, 1f);
            case Octant.LUB:
                return new Vector3(-1f, 1f, -1f);
            case Octant.LUF:
                return new Vector3(-1f, 1f, 1f);
            case Octant.RDB:
                return new Vector3(1f, -1f, -1f);
            case Octant.RDF:
                return new Vector3(1f, -1f, 1f);
            case Octant.RUB:
                return new Vector3(1f, 1f, -1f);
            case Octant.RUF:
                return new Vector3(1f, 1f, 1f);
            default:
                return new Vector3(-1f, -1f, -1f);
        }
    }
}