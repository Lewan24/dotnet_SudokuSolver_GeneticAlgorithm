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

List<SudokuCellPossibilities> possibilities = new();
FindPossibleSolutions(sudokuBoard, ref possibilities);

// Show possible solutions to empty cells
Console.WriteLine("Possible solutions to empty cells:");
foreach (var possibility in possibilities.Where(c => c.Value == 0))
    Console.WriteLine($"Row: {possibility.Row}, Col: {possibility.Column}, Possible: {string.Join(",", possibility.PossibleSolutions!)}");

// Show sudoku board
Console.WriteLine("\nBoard:");

var tempRow = 1;
for (var row = 0; row < 9; row++)
{
    var tempColumn = 1;
    for (var col = 0; col < 9; col++)
    {
        Console.Write(sudokuBoard[row, col]);
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

// Show number of empty cells
var emptyCells = possibilities.Count(c => c.Value == 0);
Console.WriteLine($"Empty cells: {emptyCells}");

// Methods used to find possible sudoku cells solutions
static void FindPossibleSolutions(int[,] board, ref List<SudokuCellPossibilities> possibilities)
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

static bool IsSafe(int[,] board, int row, int col, int num)
{
    // Check Row and Column
    for (var i = 0; i < 9; i++)
        if (board[row, i] == num || board[i, col] == num)
            return false;

    // Check 3x3
    var startRow = row - row % 3;
    var startCol = col - col % 3;
    for (var i = 0; i < 3; i++)
        for (var j = 0; j < 3; j++)
            if (board[i + startRow, j + startCol] == num)
                return false;

    return true;
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