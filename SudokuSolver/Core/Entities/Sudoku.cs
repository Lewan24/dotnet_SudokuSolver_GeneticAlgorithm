namespace Core.Entities;

/// <summary>
/// Sudoku class that contains Board and fitness value that shows how good is board solved
/// </summary>
public sealed class Sudoku
{
    public (int Value, bool IsDefaultSource, bool IsGood)[,] Board { get; set; }
    public int Fitness { get; set; }
    
    public Sudoku((int Value, bool IsDefaultSource, bool IsGood)[,] board)
    {
        Board = board;
    }

    /// <summary>
    /// Shows actual board fitness on console
    /// </summary>
    public string? ShowFitness(bool returnString = false)
    {
        var showString = $"Fitness: {Fitness}";

        if (returnString)
            return showString;
        
        Console.WriteLine(showString);
        return null;
    }
}