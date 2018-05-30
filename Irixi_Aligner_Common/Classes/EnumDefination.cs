﻿namespace Irixi_Aligner_Common
{
    public enum MoveMode
    {
        ABS,
        REL
    }

    public enum SystemState
    {
        IDLE,
        BUSY,
        PAUSE
    }
    public enum ScriptState
    {
        IDLE,
        BUSY,
        PAUSE
    }
    public enum MotionControllerType
    {
        LUMINOS_P6A,
        THORLABS_TDC001,
        IRIXI_EE0017
    }
}
