public abstract class Part
{
    public virtual int Length { get; } = 2;
    public virtual PartType type { get; protected set; } = PartType.SourceOn;
    public abstract void ProcessLights(bool leftLightIn, bool rightLightIn, ref bool leftLightOut, ref bool rightLightOut);
    public virtual bool hasOnClickBahaviour{ get; } = false; //To disable/enable the button component, so not all Parts get darker when hovered over etc.
    public virtual void OnClickBehaviour() { }
}

public class Source : Part
{
    public bool IsOn { get; private set; } = true;
    public override PartType type { get; protected set; } = PartType.SourceOn;
    public override int Length { get; } = 1;
    public override void ProcessLights(bool leftLightIn, bool rightLightIn, ref bool leftLightOut, ref bool rightLightOut)
    {
        if(IsOn) leftLightOut = true;
    }
    public void SetState(bool on)
    {
        if (on)
        {
            IsOn = true;
            type = PartType.SourceOn;
        }
        else
        {
            IsOn = false;
            type = PartType.SourceOff;
        }
    }
    public override bool hasOnClickBahaviour { get; } = true;
    public override void OnClickBehaviour()
    {
        if (IsOn) SetState(false);
        else SetState(true);
        Grid.Simulate();
    }
}

public class LED : Part
{ 
    public override int Length { get; } = 1;
    public override PartType type { get; protected set; } = PartType.LEDOff;
    public override void ProcessLights(bool leftLightIn, bool rightLightIn, ref bool leftLightOut, ref bool rightLightOut)
    {
        if(leftLightIn) 
        {
            type = PartType.LEDOn;
        }
        else
        {
            type = PartType.LEDOff;
        }
    }
    public bool IsOn()
    {
        if(type == PartType.LEDOn) return true;
        else return false;
    }
}

public class White : Part
{
    public override PartType type { get; protected set; } = PartType.White;
    public override void ProcessLights(bool leftLightIn, bool rightLightIn, ref bool leftLightOut, ref bool rightLightOut)
    {
        //NAND
        if(!(leftLightIn && rightLightIn))
        {
            leftLightOut = true;
            rightLightOut = true;
        }
    }
}

public class RRed : Part
{
    public override PartType type { get; protected set; } = PartType.RRed;
    public override void ProcessLights(bool leftLightIn, bool rightLightIn, ref bool leftLightOut, ref bool rightLightOut)
    {
        if(!rightLightIn)
        {
            leftLightOut = true;
            rightLightOut = true;
        }
    }
}


public class LRed : Part
{
    public override PartType type { get; protected set; } = PartType.LRed;
    public override void ProcessLights(bool leftLightIn, bool rightLightIn, ref bool leftLightOut, ref bool rightLightOut)
    {
        if (!leftLightIn)
        {
            leftLightOut = true;
            rightLightOut = true;
        }
    }
}

public class Blue : Part
{
    public override PartType type { get; protected set; } = PartType.Blue;
    public override void ProcessLights(bool leftLightIn, bool rightLightIn, ref bool leftLightOut, ref bool rightLightOut)
    {
        leftLightOut = leftLightIn;
        rightLightOut = rightLightIn;
    }
}

public enum PartType : byte {SourceOn, SourceOff, LEDOff, LEDOn, White, LRed, RRed, Blue};

public static class PartTypeMethods
{
    public const byte PartTypeAmount = 6; 

    public static Part ConvertToPart(PartType partType)
    {
        Part part = null;
        switch (partType)
        {
            case PartType.SourceOn:
                return new Source();
            case PartType.SourceOff:
                return new Source();
            case PartType.LEDOff:
                return new LED();
            case PartType.LEDOn:
                return new LED();
            case PartType.White:
                return new White();
            case PartType.LRed:
                return new LRed();
            case PartType.RRed:
                return new RRed();
            case PartType.Blue:
                return new Blue();
        }
        return part;
    }

    public static bool ConvertToPartType(char character, out PartType partType)
    {
        partType = PartType.SourceOn;
        switch (character)
        {
            case 'Q':
                return true;
            case 'L':
                partType = PartType.LEDOn;
                return true;
            case 'W':
                partType = PartType.White;
                return true;
            case 'R':
                partType = PartType.LRed;
                return true;
            case 'r':
                partType = PartType.RRed;
                return true;
            case 'B':
                partType = PartType.Blue;
                return true;
        }
        return false;
    }

    public static string ConvertToString(PartType partType) 
    {
        switch (partType)
        {
            case PartType.SourceOn:
                return "Q";
            case PartType.SourceOff:
                return "Q";
            case PartType.LEDOff:
                return "L";
            case PartType.LEDOn:
                return "L";
            case PartType.White:
                return "W  W";
            case PartType.LRed:
                return "R  r";
            case PartType.RRed:
                return "r  R";
            case PartType.Blue:
                return "B  B";
        }
        return "";
    }
}

