using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class Constants
{
    public const int WIDTH = 40;
    public const int HEIGHT = 40;
}
public abstract class AbstractMapData2D {
    public abstract Node[,] getMap();
}
