/*  Application work scheme
 *
 *  1. Create custom board
 *  2. Call Solve
 *  2.1. Duplicate source board to make 10 same boards
 *  2.2. For every duplicated board call TrySolve
 *  2.2.1. Find possible solutions
 *  2.2.2. Enter random solutions in random empty cells
 *  2.2.3. After every enter solution, re-find possible solutions
 *  2.2.4. Repeat 2.2.2 - 2.2.4 until there are not more possible solution to enter
 *  2.3. After generating solution, get 5 best solutions with the smallest amount of empty cells and
 *          reproduce to make 10 children. Use genetic algorithm there. Reproduce with random data from parent1 or parent2.
 *          Also mutate if chance is matched.
 *  2.4. Repeat 2.2 - 2.3 for children until there is any board with 0 empty cells
 *  3. Return solved sudoku board
 *  4. Show the result on console
 */

/*  Genetic algorithm job
 *
 *  0. General
 *      Population: 10 // number of boards
 *      Goal for every board: Fill up as most cells as it's possible
 *      Match to go to next step: Only 5 best board are gonna further. These which are with the smallest number of empty cells
 * 
 *  1. Reproduce
 *      Cross parents, create children, mutate
 * 
 *  2. CrossParents
 *      Take 5 best parents and create 2 children for every relation
 *      Child has random data from parent1 and parent2, for every get data, it needs to check if its even possible and eventually mutate
 * 
 *  3. Mutate
 *      Mutate and change one of data got from parent to another data that is possible in selected cell
 */

using System.ComponentModel.DataAnnotations;

var sudokuBoard = new[,]
{
    {5, 3, 1,   0, 7, 0,   0, 0, 0},
    {6, 0, 0,   1, 9, 5,   0, 0, 0},
    {0, 9, 8,   0, 0, 0,   0, 6, 0},
    
    {8, 0, 0,   0, 6, 0,   0, 0, 3},
    {4, 0, 0,   8, 0, 3,   0, 0, 1},
    {7, 0, 0,   0, 2, 0,   0, 0, 6},
    
    {0, 6, 0,   0, 0, 0,   2, 8, 0},
    {0, 0, 2,   4, 1, 9,   0, 0, 5},
    {0, 0, 0,   0, 8, 0,   0, 7, 9}
};

var sudoku = new Sudoku(sudokuBoard);
var sudokuSolver = new GeneticSudokuSolver(sudoku);

sudoku.ShowFitness();
sudoku.Board[0, 3] = 2;
sudoku.CalculateFitness();
sudoku.ShowFitness();

// var possibilities = sudokuSolver.FindPossibleSolutions();
//
// General.ShowPossibleSolutions(possibilities);
// General.ShowBoard(sudokuBoard);
// General.ShowEmptyCells(possibilities);

/// <summary>
/// Sudoku class that contains Board and fitness value that shows how good is board solved
/// </summary>
sealed class Sudoku
{
    public int[,] SourceBoard { get; }
    public int[,] Board { get; set; }
    public int Fitness => _fitness;
    private int _fitness = 0;

    private const int SolvedSudokuFitness = 81;
    
    public Sudoku(int[,] board)
    {
        Board = board;
        SourceBoard = board;
        
        CalculateFitness(true);
    }

    public int CalculateFitness(bool firstCalculate = false)
    {
        var sudokuSolver = new GeneticSudokuSolver(this);

        var possibilities = firstCalculate
            ? sudokuSolver.FindPossibleSolutions()
            : sudokuSolver.FindPossibleSolutionsForSpecificBoard(Board);

        _fitness = SolvedSudokuFitness - General.ShowEmptyCells(possibilities, true);
        _fitness -= sudokuSolver.GetWrongSolutions(Board);
        
        return Fitness;
    }

    public void ShowFitness()
    {
        Console.WriteLine($"Fitness: {Fitness}");
    }
}

/// <summary>
/// Class that solves sudoku board.
/// For now it just finds possible solutions.
/// </summary>
sealed class GeneticSudokuSolver
{
    #region Public Settings

    [Range(2, 1_000)]
    public int Population { get; set; } = 10;
    [Range(0.1f, 0.5f)]
    public double MutationRate { get; set; } = 0.1f;

    #endregion
    
    private readonly int[,] _sudokuBoard;

    public GeneticSudokuSolver(Sudoku sudoku)
    {
        _sudokuBoard = sudoku.SourceBoard;
    }

    /// <summary>
    /// Solve sudoku using genetic algorithm
    /// </summary>
    /// <returns>Solved sudoku board // int[,]</returns>
    public int[,] Solve()
    {
        return _sudokuBoard;
    }

    /// <summary>
    /// Finds possible solutions to empty cells
    /// </summary>
    /// <returns>List of <see cref="SudokuCellPossibilities"/> that contains information about cell position, value and possible solutions</returns>
    public List<SudokuCellPossibilities> FindPossibleSolutions()
    {
        var possibilities = new List<SudokuCellPossibilities>();
        FindPossibleSolutions(_sudokuBoard, ref possibilities);

        return possibilities;
    }
    
    /// <summary>
    /// Finds possible solutions to empty cells for specific board
    /// </summary>
    /// <returns>List of <see cref="SudokuCellPossibilities"/> that contains information about cell position, value and possible solutions</returns>
    public List<SudokuCellPossibilities> FindPossibleSolutionsForSpecificBoard(int[,] board)
    {
        var possibilities = new List<SudokuCellPossibilities>();
        FindPossibleSolutions(board, ref possibilities);

        return possibilities;
    }

    // Methods used to find possible sudoku cells solutions
    private void FindPossibleSolutions(int[,] board, ref List<SudokuCellPossibilities> possibilities)
    {
        for (var row = 0; row < 9; row++)
            for (var col = 0; col < 9; col++)
            {
                if (board[row, col] != 0)
                {
                    var cell = new SudokuCellPossibilities(row, col, null);
                    cell.Value = board[row, col];
                    possibilities.Add(cell);

                    continue;
                }

                var possible = new List<int>();
                for (var num = 1; num <= 9; num++)
                    if (IsSafe(board, row, col, num))
                        possible.Add(num);

                possibilities.Add(new SudokuCellPossibilities(row, col, possible));
            }
    }

    public int GetWrongSolutions(int[,] board)
    {
        var wrongSolutions = 0;
        
        for (var row = 0; row < 9; row++)
        for (var col = 0; col < 9; col++)
        {
            if (board[row, col] == 0)
                continue;

            if (!IsUnSafe(board, row, col)) continue;
            
            wrongSolutions++;
            break;
        }

        return wrongSolutions;
    }
    
    public bool IsSafe(int[,] workingBoard, int row, int col, int num)
    {
        // Check Row and Column
        for (var i = 0; i < 9; i++)
            if (workingBoard[row, i] == num || workingBoard[i, col] == num)
                return false;

        // Check 3x3
        var startRow = row - row % 3;
        var startCol = col - col % 3;
        for (var i = 0; i < 3; i++)
        for (var j = 0; j < 3; j++)
            if (workingBoard[i + startRow, j + startCol] == num)
                return false;

        return true;
    }
    
    public bool IsUnSafe(int[,] workingBoard, int row, int col)
    {
        var counter = 0;
        var num = workingBoard[row, col];
        
        // Check Row and Column
        for (var i = 0; i < 9; i++)
        {
            if (workingBoard[row, i] == num || workingBoard[i, col] == num)
            {
                counter++;

                if (counter > 2)
                    return true;
            }
        }

        counter = 0;
        
        // Check 3x3
        var startRow = row - row % 3;
        var startCol = col - col % 3;
        for (var i = 0; i < 3; i++)
        for (var j = 0; j < 3; j++)
            if (workingBoard[i + startRow, j + startCol] == num)
            {
                counter++;

                if (counter >= 2)
                    return true;
            }

        return false;
    }
}

/// <summary>
/// General actions for <see cref="GeneticSudokuSolver"/>
/// </summary>
static class General
{
    /// <summary>
    /// Show on console selected board
    /// </summary>
    /// <param name="board">Board as int[,] that contains board cells</param>
    public static void ShowBoard(int[,] board)
    {
        Console.WriteLine("\nBoard:");

        var tempRow = 1;
        for (var row = 0; row < 9; row++)
        {
            var tempColumn = 1;
            for (var col = 0; col < 9; col++)
            {
                Console.Write(board[row, col]);
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

/// <summary>
/// Simple class to store information about which cell has what possibilities
/// </summary>
sealed class SudokuCellPossibilities
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