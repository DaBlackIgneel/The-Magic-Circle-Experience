using System.Collections;
using System.Collections.Generic;

public enum MagicCircleType
{
    None = 0,
    START = 0,
    Element = 1,
    Form = 2,
    Movement = 3,
    END = 3,
    Input = 4,
    Logic = 5,
    Math = 6,
}

public enum ElementType
{
    Water = 0,
    START = 0,
    Earth = 1,
    Fire = 2,
    Wind = 3,
    END = 3
}

public enum ElementPhase
{
    Liquid = 0,
    Solid = 1
}

public enum FormType
{
    Triangle = 0,
    START = 0,
    Ball = 1,
    Rectangle = 2,
    Custom = 3,
    END = 3
}

public enum MovementType
{
    START = 0,
    Push = 0,
    Path = 1,
    Control = 2,
    Pour = 3,
    Stop = 4,
    END = 4
}

public enum InputType
{
    START = 0,
    User = 0,
    Target = 1,
    Scan = 2,
    END = 2
}

public enum LogicType
{
    START = 0,
    Equals = 0,
    NotEquals = 1,
    GreaterThan = 2,
    LessThan = 3,
    GreaterThanOrEqual = 4,
    LessThanOrEqual = 5,
    And = 6,
    Or = 7,
    END = 7
}

public enum MathType
{
    START = 0,
    END = 1
}

public enum LinkTypes
{
    Transition,
    Data
}

public delegate void ActivationFunction();
