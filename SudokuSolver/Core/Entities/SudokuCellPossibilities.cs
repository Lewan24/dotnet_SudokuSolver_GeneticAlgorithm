namespace Core.Entities;

/// <summary>
/// Simple class to store information about which cell has what possibilities
/// </summary>
public sealed class SudokuCellPossibilities
{
    public int Value { get; set; } = 0;
    public int Row { get; }
    public int Column { get; }
    public List<int>? PossibleSolutions { get; }
    
    public SudokuCellPossibilities(int row, int col, List<int>? possibleSolutions)
    {
        Row = row;
        Column = col;
        PossibleSolutions = possibleSolutions;
    }
};