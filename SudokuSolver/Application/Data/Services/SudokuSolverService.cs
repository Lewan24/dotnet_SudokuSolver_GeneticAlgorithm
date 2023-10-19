using System.ComponentModel.DataAnnotations;
using Application.Data.Interfaces;
using Application.Data.Static_Classess;
using Core.Entities;

namespace Application.Data.Services;

/// <summary>
/// Settings for SudokuSolverService that contains needed data to run Genetic Algorithm
/// </summary>
public class ServiceSettings
{
    [Range(2, 1_000)]
    public int PopulationSize { get; set; } = 10;
    
    [Range(0.1f, 0.5f)]
    public double MutationRate { get; set; } = 0.1f;
    
    [Range(10, Int32.MaxValue)]
    public int MaxGenerations { get; set; } = 100;
}

/// <summary>
/// Sudoku solver service that manages sudoku board, genetic solving etc.
/// </summary>
public class SudokuSolverService : ISudokuSolverService
{
    #region Hyper Parameters
    private readonly ServiceSettings _settings = new();
    #endregion
    
    private Sudoku _sourceSudoku;
    private Sudoku _bestSudokuSolution;
    private static Random _random = new();

    public SudokuSolverService() { }

    public SudokuSolverService(ServiceSettings settings)
    {
        _settings = settings;
    }

    public Task Run(string sudokuFilePathName)
    {
        var validationContext = new ValidationContext(_settings, null, null);
        var validationResults = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(_settings, validationContext, validationResults, true);

        if (!isValid)
            throw new Exception("Settings are not configured well. Fix them first and try again");
        
        Cell[,] sudokuBoard = SudokuBoardGeneral.ReadSudokuBoardFromFile(sudokuFilePathName);
        //GeneralIO.ShowBoard(sudokuBoard);
        
        _sourceSudoku = new(sudokuBoard)
        {
            Fitness = CalculateFitness(sudokuBoard)
        };

        _bestSudokuSolution = new Sudoku(_sourceSudoku.Board);
        
        //var result = sudokuSolver.Solve();
        //General.ShowBoard(result.Board);
        
        return Task.CompletedTask;
    }
}

/// <summary>
/// Class that solves sourceSudoku board.
/// For now it just finds possible solutions.
/// </summary>
sealed class GeneticSudokuSolver
{
    
    
    

    /// <summary>
    /// Solve sourceSudoku using genetic algorithm
    /// </summary>
    /// <returns><see cref="Sudoku"/> with solved board</returns>
    public Sudoku Solve()
    {
        List<Sudoku> sudokuList = new();
        
        CreateFirstPopulation(ref sudokuList);
        ShowPopulationInfo(sudokuList, isFirstPopulation: true);

        FillUpSudoku(ref sudokuList);

        ShowPopulationInfo(sudokuList);
        
        RunEvolutionProcess(sudokuList);

        return _bestSudokuSolution;
    }

    /// <summary>
    /// Duplicate source sudoku to list that's count is population number.
    /// </summary>
    private void CreateFirstPopulation(ref List<Sudoku> sudokuList)
    {
        for (var i = 0; i < PopulationSize; i++)
        {
            var newSudoku = new Sudoku(new int[9, 9]);
            Array.Copy(_sourceSudoku.Board, newSudoku.Board, _sourceSudoku.Board.Length);
            
            newSudoku.Fitness = CalculateFitness(newSudoku.Board);
            
            sudokuList.Add(newSudoku);
        }
    }

    /// <summary>
    /// Shows on console actual population information. If its first population then show global fitness of source board. If not, then shows fitness for every board.
    /// </summary>
    /// <param name="sudokuList"></param>
    /// <param name="showBoard">Shown board to console for every sudoku in population</param>
    /// <param name="isFirstPopulation">Simple check if its first population or not</param>
    private void ShowPopulationInfo(List<Sudoku> sudokuList, bool showBoard = false, bool isFirstPopulation = false)
    {
        if (isFirstPopulation)
        {
            Console.WriteLine($"First population sudoku Fitness: {_sourceSudoku.ShowFitness(returnString: true)}");
            return;
        }
        
        for (var index = 0; index < sudokuList.Count; index++)
        {
            var sudoku = sudokuList[index];
            
            Console.WriteLine($"Sudoku nr {index + 1}. Fitness: {sudoku.ShowFitness(returnString: true)}");
            // Todo: prepare new way of showing board using Cell[,]
            // if (showBoard)
            //     General.ShowBoard(sudoku.Board);
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Fills up the empty cells in sudoku board with random numbers
    /// </summary>
    private void FillUpSudoku(ref List<Sudoku> sudokuList)
    {
        foreach (var sudoku in sudokuList)
        {
            for (var row = 0; row < 9; row++)
            for (var col = 0; col < 9; col++)
            {
                if (sudoku.Board[row, col] != 0) continue;
                
                fillup:
                var num = _random.Next(1, 10);
                
                if (!IsSafe(sudoku.Board, row, col, num, false, true))
                    goto fillup;
                    
                sudoku.Board[row, col] = num;
            }
            
            sudoku.Fitness = CalculateFitness(sudoku.Board);
        }
    }
    
    private void RunEvolutionProcess(List<Sudoku> sudokuList)
    {
        var population = sudokuList;
        
        for (var generation = 0; generation < MaxGenerations; generation++)
        {
            foreach (var sudoku in population)
                sudoku.Fitness = CalculateFitness(sudoku.Board);

            if (population.Any(s => s.Fitness == 81))
            {
                _bestSudokuSolution = population.First(s => s.Fitness == 81);
                break;
            }

            _bestSudokuSolution = population.OrderByDescending(c => c.Fitness).First();
            
            var parents = SelectParents(population);

            var newPopulation = new List<Sudoku>();
            for (var i = 0; i < population.Count; i += 2)
            {
                // Cross over parents
                var children = Crossover(parents[i], parents[i + 1]);

                newPopulation.AddRange(children);
            }

            population = newPopulation;
        }
    }

    /// <summary>
    /// Calculate for board
    /// </summary>
    /// <param name="board"></param>
    /// <param name="firstCalculate">Check if it's the first calculation (during creating constructor)</param>
    /// <returns>Fitness is an int number of how good the solution is</returns>
    private int CalculateFitness(int[,] board, bool firstCalculate = false)
    {
        var possibilities = firstCalculate
            ? FindPossibleSolutions()
            : FindPossibleSolutionsForSpecificBoard(board);
        
        var newFitness = 81 - GeneralIO.ShowEmptyCells(possibilities, true);
        
        newFitness -= GetWrongSolutions(board);

        return newFitness;
    }
    
    /// <summary>
    /// Selection of the best half of population parents
    /// </summary>
    /// <returns>List of <see cref="Sudoku"/> that contains half of population number best parents to reproduce // the highest Fitness rate</returns>
    private List<Sudoku> SelectParents(List<Sudoku> sudokuList) => 
        sudokuList.OrderByDescending(sudoku => sudoku.Fitness).Take((PopulationSize / 2) % 2 == 0 ? PopulationSize / 2 : (PopulationSize / 2) + 1).ToList();

    private List<Sudoku> Crossover(Sudoku parent1, Sudoku parent2)
    {
        var children = new List<Sudoku>();

        var isFirstChild = true;
        for (var i = 0; i < 2; i++)
        {
            var child = CrossParentsIntoChild(new (parent1, parent2), reverseCross: isFirstChild);
            Mutate(ref child);

            children.Add(child);
            
            isFirstChild = false;
        }

        return children;
    }

    public Sudoku CrossParentsIntoChild((Sudoku parent1, Sudoku Parent2) parents, bool reverseCross)
    {
        var sudokuBoard1 = parents.parent1.Board;
        var sudokuBoard2 = parents.Parent2.Board;

        var childBoard = new int[9, 9];

        // Copy top row from parent 1
        for (var row = 0; row < 3; row++)
            for (var col = 0; col < 9; col++)
                childBoard[row, col] = sudokuBoard1[row, col];

        // Copy 1 middle left column from parent 1
        for (var row = 3; row < 6; row++)
            for (var col = 0; col < 3; col++)
                childBoard[row, col] = sudokuBoard1[row - 3, col];

        var whichParent = _random.NextDouble() <= MutationRate;
        
        // Copy middle row middle column from parent 1 or parent 2, depends on mutation chance rate
        for (var row = 3; row < 6; row++)
            for (var col = 3; col < 6; col++)
                childBoard[row, col] = whichParent ? sudokuBoard1[row - 3, col] : sudokuBoard2[row - 3, col];
        
        // Copy 1 middle right column from parent 2
        for (var row = 3; row < 6; row++)
            for (var col = 6; col < 9; col++)
                childBoard[row, col] = sudokuBoard2[row - 3, col];
        
        // Copy bottom row from parent 2
        for (var row = 6; row < 9; row++)
            for (var col = 0; col < 9; col++)
                childBoard[row, col] = sudokuBoard2[row - 3, col];

        var child = new Sudoku(childBoard);
        return child;
    }
    
    // Todo: implement mutation
    private void Mutate(ref Sudoku child)
    {
        for (var i = 0; i < 9; i++)
        {
            var mutationChance = _random.NextDouble();

            if (mutationChance <= MutationRate)
                child.Board[0, 0] = 1;
        }
    }
    
    /// <summary>
    /// Finds possible solutions to empty cells
    /// </summary>
    /// <returns>List of <see cref="SudokuCellPossibilities"/> that contains information about cell position, value and possible solutions</returns>
    private List<SudokuCellPossibilities> FindPossibleSolutions()
    {
        var possibilities = new List<SudokuCellPossibilities>();
        FindPossibleSolutions(_sourceSudoku.Board, ref possibilities);
    
        return possibilities;
    }
    
    /// <summary>
    /// Finds possible solutions to empty cells for specific board
    /// </summary>
    /// <returns>List of <see cref="SudokuCellPossibilities"/> that contains information about cell position, value and possible solutions</returns>
    private List<SudokuCellPossibilities> FindPossibleSolutionsForSpecificBoard(int[,] board)
    {
        var possibilities = new List<SudokuCellPossibilities>();
        FindPossibleSolutions(board, ref possibilities);
    
        return possibilities;
    }
    
    // Methods used to find possible sourceSudoku cells solutions
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

    private int GetWrongSolutions(int[,] board)
    {
        var wrongSolutions = 0;
        
        for (var row = 0; row < 9; row++)
        for (var col = 0; col < 9; col++)
        {
            if (board[row, col] == 0)
                continue;

            if (!IsUnSafe(board, row, col)) continue;
            
            wrongSolutions++;
        }

        return wrongSolutions;
    }
    private bool IsSafe(int[,] workingBoard, int row, int col, int num, bool checkRowCol = true, bool check3x3 = true)
    {
        // Check Row and Column
        if (checkRowCol)
            for (var i = 0; i < 9; i++)
                if (workingBoard[row, i] == num || workingBoard[i, col] == num)
                    return false;
    
        // Check 3x3
        if (check3x3)
        {
            var startRow = row - row % 3;
            var startCol = col - col % 3;
            for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++)
                if (workingBoard[i + startRow, j + startCol] == num)
                    return false;
        }
    
        return true;
    }
    private bool IsUnSafe(int[,] workingBoard, int row, int col)
    {
        var counter = 0;
        var num = workingBoard[row, col];
        
        // Check Row and Column
        for (var i = 0; i < 9; i++)
        {
            if (workingBoard[row, i] == num || workingBoard[i, col] == num)
            {
                counter++;

                if (counter >= 2)
                    return true;
            }
        }

        // counter = 0;
        //
        // // Check 3x3
        // var startRow = row - row % 3;
        // var startCol = col - col % 3;
        // for (var i = 0; i < 3; i++)
        // for (var j = 0; j < 3; j++)
        //     if (workingBoard[i + startRow, j + startCol] == num)
        //     {
        //         counter++;
        //
        //         if (counter >= 2)
        //             return true;
        //     }

        return false;
    }
}