﻿

// L/R: left/right (0,1) (x)
// U/D: up/down (1,0) (y)
// B/F: back/front (0,1) (z)
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

public static class OctreeExtensions
{
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
}