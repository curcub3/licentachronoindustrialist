using MessagePack;
using Core.Simulation.Data;

namespace Core.Simulation.Serialization
{
    [MessagePackObject]
    public struct GameCommand
    {
        [Key(0)] public int Tick;           // The exact integer tick to execute
        [Key(1)] public int PlayerID;       // The actor issuing the command
        [Key(2)] public byte ActionType;    // Enum: Build, Demolish, Interact
        [Key(3)] public IntVector2 Payload; // Grid coordinates or directional data
    }
}
