using Application.Data.Services;
using Core.Entities;

namespace Application.Data.Static_Classess;

/// <summary>
/// General actions for <see cref="SudokuSolverService"/>
/// </summary>
public static class GeneralIO
{
    /// <summary>
    /// Show on console selected board
    /// </summary>
    /// <param name="board">Board as int[,] that contains board cells</param>
    public static void ShowBoard((int Value, bool IsDefaultSource, bool IsGood)[,] board)
    {
        Console.WriteLine("\nBoard:");

        var tempRow = 1;
        for (var row = 0; row < 9; row++)
        {
            var tempColumn = 1;
            for (var col = 0; col < 9; col++)
            {
                Console.Write(board[row, col].Value);
                if (tempColumn == 3)
                {
                    Console.Write("  ");
                    tempColumn = 1;
                    continue;
                }
        
                Console.Write(" ");
                tempColumn++;
            }

            if (tempRow == 3)
            {
                Console.WriteLine("\n");
                tempRow = 1;
                continue;
            }

            Console.WriteLine();
            tempRow++;
        }
    }

    /// <summary>
    /// Show on console number of empty cells in selected board
    /// </summary>
    /// <param name="possibilities">List of <see cref="SudokuCellPossibilities"/> that contains empty cells information bout possible solutions</param>
    /// <param name="onlyReturnEmptyCellsNumber">Bool that declares if method should only return number of empty cells or should also show them in console</param>
    /// <returns>Number of empty cells in selected board</returns>
    public static int ShowEmptyCells(List<SudokuCellPossibilities> possibilities, bool onlyReturnEmptyCellsNumber = false)
    {
        var emptyCells = possibilities.Count(c => c.Value == 0);
        
        if (onlyReturnEmptyCellsNumber) 
            return emptyCells;
        
        Console.WriteLine($"Empty cells: {emptyCells}");
        return emptyCells;
    }
    
    /// <summary>
    /// Shows on console possible solutions for every empty cell
    /// </summary>
    /// <param name="possibilities">List of <see cref="SudokuCellPossibilities"/> that contains empty cells information bout possible solutions</param>
    public static void ShowPossibleSolutions(List<SudokuCellPossibilities> possibilities)
    {
        Console.WriteLine("Possible solutions to empty cells:");
        foreach (var possibility in possibilities.Where(c => c.Value == 0))
            Console.WriteLine($"Row: {possibility.Row}, Col: {possibility.Column}, Possible: {string.Join(",", possibility.PossibleSolutions!)}");
    
    }
}