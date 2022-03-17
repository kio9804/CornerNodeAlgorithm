using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;

//git test
public class CornerGenerator
{
    //Constants
    private const int OPEN = 0;
    private const int WALL = 1;
    private const int CLOSE = 2;
    private const int NODE = 3;
    private const int CHECK = 4;

    private Cell[,] map;
    private List<PathNode> pNodeList = new List<PathNode>();
    private CheckWall cWall = new CheckWall();
    
    private int[,] direction = {{1, 0}, {0, -1}, {-1, 0}, {0, 1}};
    private int[,] digDir = {{1, 1}, {1, -1}, {-1, 1}, {-1, -1}};
    private int selection = 0;
    private int buf = 0;
    private int ptrX = 0, ptrY = 0;
    private int width, height;

    public void setMap(Cell[,] map) // Set the generated map data
    {
        this.map = map;
        width = map.GetLength(0);
        height = map.GetLength(1);
        cWall.setMapData(this.map);
    }

    public List<PathNode> getPNodeList()
    {
        return pNodeList;
    }


    private void cornerGene() // Main algorithm
    {
        if (findStart())
        {
            return;
        }

        int startX = ptrX;
        int startY = ptrY;
        do
        {
            map[ptrX + direction[selection, 0], ptrY + direction[selection, 1]].Type = CHECK;
            if (ptrX + direction[selection, 0] != width && ptrY + direction[selection, 1] != height)
            {
                ptrX = ptrX + direction[selection, 0];
                ptrY = ptrY + direction[selection, 1];
            }

            if (map[ptrX + direction[selection, 0], ptrY + direction[selection, 1]].Type == WALL)
            {
                continue;
            }
            else if (map[ptrX + direction[selection, 0], ptrY + direction[selection, 1]].Type == CLOSE)
            {
                if (dirSelection())
                {
                    diagonalFind();
                }
            }
            else if (map[ptrX + direction[selection, 0], ptrY + direction[selection, 1]].Type == OPEN)
            {
                if (!dirSelection())
                {
                    nodeCreate();
                }
                else
                {
                    diagonalFind();
                }
                
            }
            else
            {
                //Nothing happen
            }
        } while (ptrX != startX || ptrY != startY);

        createConnect();
        
    }
    public void checkWall(PathNode startNode, PathNode targetNode)
    {
        Vector3 startV = new Vector3(startNode.getX(), startNode.getY());
        Vector3 targetV = new Vector3(targetNode.getX(), targetNode.getY());
        Vector3 checkDir = targetV - startV;
        Vector3 dirNorm = checkDir.normalized;
         for (int i = 1; dirNorm.magnitude * i <= checkDir.magnitude; i++)
         {
             int ptrX = startNode.getX() + (int) (dirNorm * i).x;
             int ptrY = startNode.getY() + (int) (dirNorm * i).y;
        
             if (map[ptrX, ptrY].Type == CLOSE || map[ptrX, ptrY].Type == WALL || map[ptrX, ptrY].Type == CHECK )
             {
                 Debug.Log("return");
                 return;
             }
        }
        startNode.setCnn(targetNode, checkDir);
        targetNode.setCnn(startNode, checkDir);
    }
    private void createConnect()
    {
        for (int i = 0; i < pNodeList.Count; i++)
        {
            for (int j = i + 1; j < pNodeList.Count; j++)
            {
                checkWall(pNodeList[i], pNodeList[j]);
            }
        }
    }

    private bool findStart() // If you haven't set a starting pointer, can find the starting point 
    {
        while (map[ptrX, ptrY].Type != 1)
        {
            ptrX++;
            if (ptrX == width)
            {
                ptrX = 0;
                ptrY++;
            }

            if (ptrX == width - 1 && ptrY == height - 1)
            {
                return true;
            }
        }

        ptrX++;
        return false;
    }

    private bool dirSelection() // Direction select subroutine
    {
        int right = dirCalculator(selection - 1);
        int left = dirCalculator(selection + 1);
        if (map[ptrX + direction[right, 0], ptrY + direction[right, 1]].Type == WALL)
        {
            buf = selection;
            selection = right;
            return false;
        }
        else if (map[ptrX + direction[left, 0], ptrY + direction[left, 1]].Type == WALL)
        {
            buf = selection;
            selection = left;
            return false;
        }
        else
        {
            return true;
        }
    }

    private void nodeCreate() // Select to Node create direction
    {
        int newDirX = direction[dirCalculator(selection + 2), 0];
        int newDirY = direction[dirCalculator(selection + 2), 1];
        int bufDirX = direction[buf, 0];
        int bufDirY = direction[buf, 1];
        int dirX = ptrX + newDirX + bufDirX;
        int dirY = ptrY + newDirY + bufDirY;
        map[dirX, dirY].Type = NODE;
        pNodeList.Add(new PathNode(dirX, dirY));
    }

    private int dirCalculator(int num)
    {
        if (num < 0) num = 4 + num;
        else if (num > 3) num = num - 4;
        return num;
    }

    private void diagonalFind() // Move the pointer diagonally
    {
        for (int i = 0; i < 4; i++)
        {
            int movX = ptrX + digDir[i, 0];
            int movY = ptrY + digDir[i, 1];
            if (map[movX, movY].Type == WALL)
            {
                diagonalNodeCreate();
                diagonalMov(digDir[i, 0], digDir[i, 1]);
                break;
            }
        }
    }

    private void diagonalMov(int x, int y)
    {
        if (map[ptrX + x, ptrY + y].Type == WALL)
        {
            ptrX += x;
            ptrY += y;
            diagonalMov(x, y);
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                if (map[ptrX + direction[i, 0], ptrY + direction[i, 1]].Type == WALL)
                {
                    selection = i;
                    diagonalNodeCreate();
                    break;
                }
            }
        }
    }

    private void diagonalNodeCreate()
    {
        for (int i = 0; i < 4; i++)
        {
            int nodX = ptrX + digDir[i, 0];
            int nodY = ptrY + digDir[i, 1];
            if (map[nodX, nodY].Type == OPEN)
            {
                map[nodX, nodY].Type = NODE;
                pNodeList.Add(new PathNode(nodX, nodY));
            }
        }
    }

    public void geneStart() // Node generation start
    {
        cornerGene();
    }
}