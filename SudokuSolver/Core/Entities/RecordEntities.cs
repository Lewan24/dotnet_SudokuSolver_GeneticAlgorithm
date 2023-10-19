namespace Core.Entities;

public sealed record Cell(int Value, bool IsDefaultSource = false, bool IsGood = false);