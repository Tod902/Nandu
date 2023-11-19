using System;
using UnityEngine;
using System.Collections.Generic;

public static class Grid
{
    private static Vector2Int _minSize = new Vector2Int(3, 3);
    private static Vector2Int _maxSize = new Vector2Int(1000, 1000);

    private static bool[,] _lights = new bool[_minSize.x, _minSize.y];
    private static Part[,] _parts = new Part[_minSize.x, _minSize.y];

    public static Vector2Int Size { get; private set; } = _minSize;

    /// <summary>
    /// A delegate which will be called when the grid changes in size 
    /// </summary>
    public static Action OnSizeChange;

    /// <summary>
    /// A delegate which will be called when the grid changes its contents
    /// </summary>
    public static Action OnContentChange;

    #region Interaction Methods
    public static void CreateGridFromString(string grid)
    {
        Clear(false);
        string[] instructions = grid.Split(new string[3] { " ", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        try
        {
            SetGridSize(new Vector2Int(int.Parse(instructions[0]), int.Parse(instructions[1])));
            for (int i = 2; i < instructions.Length; i++)
            {
                int x = (i - 2) % Size.x;
                int y = (i - 2) / Size.x;
                char character = instructions[i][0];
                if (PartTypeMethods.ConvertToPartType(character, out PartType partType))
                {
                    Part part = PartTypeMethods.ConvertToPart(partType);
                    PlacePart(new Vector2Int(x, y), part, false);
                    i += part.Length - 1;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to generated Grid from string. Error: " + e.Message);
        }
        Simulate();
    }

    public static string GetGridString()
    {
        string instructions = "";
        instructions += Size.x + " ";
        instructions += Size.y + "\n";

        int sourceCounter = 1;
        int LEDCounter = 1;
        bool previousIsSourceOrLED = false;
        for (int y = 0; y < Size.y; y++)
        {
            for (int x = 0; x < Size.x; x++)
            {
                string prefix = "";
                if (x != 0) prefix = "  ";

                Vector2Int position = new Vector2Int(x, y);
                Part part = _parts[position.x, position.y];
                if (part != null)
                {
                    string partString = PartTypeMethods.ConvertToString(part.type);

                    if (partString == "Q")
                    {
                        if (!previousIsSourceOrLED) partString = prefix + "Q";
                        partString += sourceCounter;
                        partString += " ";
                        sourceCounter++;
                        previousIsSourceOrLED = true;
                    }
                    else if (partString == "L")
                    {
                        if (!previousIsSourceOrLED) partString = prefix + "L";
                        partString += LEDCounter;
                        partString += " ";
                        LEDCounter++;
                        previousIsSourceOrLED = true;
                    }
                    else previousIsSourceOrLED = false;

                    if (previousIsSourceOrLED) instructions += partString;
                    else instructions += prefix + partString;

                    x += part.Length - 1;
                }
                else
                {
                    if (previousIsSourceOrLED) instructions += "X";
                    else instructions += prefix + "X";
                    previousIsSourceOrLED = false;
                }
            }
            if (y != Size.y - 1) instructions += "\n";
        }
        return instructions;
    }

    public static bool SetGridSize(Vector2Int boardSize)
    {
        if (boardSize.x < _minSize.x || boardSize.y < _minSize.y || boardSize.x > _maxSize.x || boardSize.y > _maxSize.x) return false;

        Vector2Int oldSize = Size;
        bool[,] oldLights = _lights;
        Part[,] oldParts = _parts;

        Size = boardSize;
        _lights = new bool[boardSize.x, boardSize.y];
        _parts = new Part[boardSize.x, boardSize.y];

        for (int x = 0; x < oldSize.x; x++)
        {
            for (int y = 0; y < oldSize.y; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                if (IsInGridBounds(position))
                {
                    if (x >= 0 && y >= 0 && x < oldSize.x && y < oldSize.y)
                    {
                        _lights[x, y] = oldLights[x, y];
                        _parts[x, y] = oldParts[x, y];
                    }
                }
            }
        }

        OnSizeChange();
        Simulate();
        //to make lights being placed after SizeChange; check part legality since they might be out of bounds when shrinking
        return true;
    }

    //private placePart without simulate
    private static void PlacePart(Vector2Int position, Part part, bool simulate)
    {
        //Part part = PartTypeMethods.ConvertToPart(partType);
        if (IsInGridBounds(position) && !PartOnPositions(position, part.Length))
        {
            //To prevent placement of Parts with sizes of two or bigger from being placed on the (near) edge of the grid
            //if (position.x > Size.x - part.Length) return; -> redudant since PartOnPositions exists now
            _parts[position.x, position.y] = part;//PartTypeMethods.ConvertToPart(partType);
            if (simulate) Simulate();
        }
    }

    public static void PlacePart(Vector2Int position, PartType partType)
    {
        Part part = PartTypeMethods.ConvertToPart(partType);
        PlacePart(position, part, true);
    }

    public static void DeletePart(Vector2Int position)
    {
        if (IsInGridBounds(position) && PartOnPosition(position, out Vector2Int partPosition))
        {
            _parts[partPosition.x, partPosition.y] = null;
            Simulate();
        }
    }

    private static void Clear(bool triggerOnContentChange)
    {
        _lights = new bool[Size.x, Size.y];
        _parts = new Part[Size.x, Size.y];
        if (triggerOnContentChange) OnContentChange();
    }

    public static void Clear()
    {
        Clear(true);
    }

    public static bool CreateTable(out string table)
    {
        table = "";
        List<Source> sources = new List<Source>();
        List<LED> LEDs = new List<LED>();
        for (int y = 0; y < Size.y; y++)
        {
            for (int x = 0; x < Size.x; x++)
            {
                if (PartOnThisPosition(new Vector2Int(x, y), out Part part))
                {
                    if (part.type == PartType.SourceOn || part.type == PartType.SourceOff) sources.Add((Source)part);
                    else if (part.type == PartType.LEDOn || part.type == PartType.LEDOff) LEDs.Add((LED)part);
                }
            }
        }

        //To simulate the Grid more than 2^10 times is quite expensive and will cause lag.
        if (sources.Count > 10) return false;

        bool[] sourcesOriginalStates = new bool[sources.Count];
        for (int i = 0; i < sources.Count; i++)
        {
            sourcesOriginalStates[i] = sources[i].IsOn;
            table += "Q" + (i + 1) + " ";
            //Changed a little for "An" and "Aus" instead of 1 and 0:
            //table += "Q" + (i + 1) + "  ";
        }
        for (int i = 0; i < LEDs.Count; i++)
        {
            table += "L" + (i + 1) + " ";
            //Changed a little for "An" and "Aus" instead of 1 and 0:
            //table += "L" + (i + 1) + "  ";
        }
        table += "\n";

        int combinations = (int)Mathf.Pow(2f, sources.Count);
        for (int i = 0; i < combinations; i++)
        {
            for (int j = 0; j < sources.Count; j++)
            {
                bool isActive = false;
                if ((i & (1 << j)) != 0) isActive = true;
                sources[sources.Count - 1 - j].SetState(isActive);
            }
            for (int j = 0; j < sources.Count; j++)
            {
                table += sources[j].IsOn ? 1 + "  " : 0 + "  ";
                //"An" and "Aus" instead of 1 and 0:
                //table += sources[j].IsOn ? "An  " : "Aus ";
            }

            Simulate(false);
            for (int j = 0; j < LEDs.Count; j++)
            {
                table += LEDs[j].IsOn() ? 1 + "  " : 0 + "  ";
                //"An" and "Aus" instead of 1 and 0:
                //table += LEDs[j].IsOn() ? "An  " : "Aus ";
            }
            table = table + "\n";
        }

        for (int i = 0; i < sources.Count; i++)
        {
            sources[i].SetState(sourcesOriginalStates[i]);
        }
        Simulate();
        return true;
    }
    #endregion

    private static void Simulate(bool callOnContentChange)
    {
        _lights = new bool[Size.x, Size.y];
        //y++ not y--; cause visual WorldToGridPos
        for (int y = 0; y < Size.y; y++)
        {
            for (int x = 0; x < Size.x; x++)
            {
                Part part = _parts[x, y];
                if (part != null)
                {
                    bool rightLightIn = false;
                    if (x != Size.x - 1) rightLightIn = _lights[x + 1, y];

                    bool leftLightOut = false;
                    bool rightLightOut = false;

                    part.ProcessLights(_lights[x, y], rightLightIn, ref leftLightOut, ref rightLightOut);

                    if (y != Size.y - 1)
                    {
                        _lights[x, y + 1] = leftLightOut;
                        if (x != Size.x - 1) _lights[x + 1, y + 1] = rightLightOut;
                    }
                }
            }
        }
        if (callOnContentChange) OnContentChange();

        /*
        //Old Debug stuff:
        string s = "";
        for (int y = 0; y < Size.y; y++)
        {
            for(int x = 0; x < Size.x; x++)
            {
                if(_lights[x,y]) s += "y";
                else s += "n";
            }
            s += "\n";
        }
        */
        //Debug.LogWarning(s);
    }

    public static void Simulate()
    {
        Simulate(true);
    }

    #region Check Methods
    //All PartOnPositions do not make a default IsInGridBounds call since that would often lead to redundance, though it would be safer

    /// <summary>
    /// Also checks the position before the given position
    /// </summary>
    public static bool PartOnPosition(Vector2Int position, out Vector2Int partPosition)
    {
        partPosition = new Vector2Int(-1, -1);
        if (_parts[position.x, position.y] != null)
        {
            partPosition = new Vector2Int(position.x, position.y);
            return true;
        }

        if (position.x != 0)
        {
            if (_parts[position.x - 1, position.y] != null && _parts[position.x - 1, position.y].Length > 1)
            {
                partPosition = new Vector2Int(position.x - 1, position.y);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Also checks the position before the given position
    /// </summary>
    public static bool PartOnPosition(Vector2Int position)
    {
        return PartOnPosition(position, out Vector2Int ignore);
    }

    /// <summary>
    /// Checks the position before the given position and after it according to the given length
    /// <br/> Used for Placement checks only
    /// <br/> Also returns false if the part of the given length would be outside the grid's bounds
    /// </summary>
    public static bool PartOnPositions(Vector2Int position, int length)
    {
        if (position.x != 0)
        {
            if (_parts[position.x - 1, position.y] != null && _parts[position.x - 1, position.y].Length > 1)
            {
                return true;
            }
        }

        for (int i = 0; i < length; i++)
        {
            Vector2Int posToCheck = new Vector2Int(position.x + i, position.y);
            if (!Grid.IsInGridBounds(posToCheck) || _parts[posToCheck.x, posToCheck.y] != null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Doesn't check the previous x-position, like the others PartOnPosition
    /// <br/> That's because this method is made for going through each Part of _parts once
    /// <br/> The others check the previous x-position, since some Parts take up two spaces!
    /// </summary>
    public static bool PartOnThisPosition(Vector2Int position, out Part part)
    {
        part = null;
        if (_parts[position.x, position.y] != null)
        {
            part = _parts[position.x, position.y];
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns wheter a light is on the given Position
    /// Returns false if a Part is on the given Position, regardless of the light
    /// </summary>
    public static bool LightOnThisPosition(Vector2Int position)
    {
        if (!PartOnPosition(position)) return _lights[position.x, position.y];
        return false;
    }

    public static bool IsInGridBounds(Vector2Int position)
    {
        if (position.x < 0 || position.x >= Size.x || position.y < 0 || position.y >= Size.y) return false;
        return true;
    }
    #endregion
}
