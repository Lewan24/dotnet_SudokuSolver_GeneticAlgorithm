namespace Core.Entities;

public sealed class Cell
{
    public int Value { get; set; }
    public bool IsDefaultSource { get; set; }
    public bool IsGood { get; set; }
    
    public Cell(){}

    public Cell(int value, bool isDefaultSource, bool isGood)
    {
        Value = value;
        IsDefaultSource = isDefaultSource;
        IsGood = isGood;
    }
}