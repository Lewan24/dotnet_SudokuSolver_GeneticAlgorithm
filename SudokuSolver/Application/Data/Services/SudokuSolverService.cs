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

    public SudokuSolverService(){}

    public SudokuSolverService(ServiceSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Run service // Initialize, prepare and run all needed functions and algorithms to solve sudoku using genetic algorithm
    /// </summary>
    /// <param name="sudokuFilePathName">File path to .txt file where is the sudoku board</param>
    /// <returns></returns>
    /// <exception cref="Exception">Throws an exception if settings are not configured well. There is a simple data annotations validator</exception>
    public Task Run(string sudokuFilePathName)
    {
        var validationResult = ValidateSettings(sudokuFilePathName);
        if (!validationResult)
            throw new Exception("Settings are not configured well. Fix them first and try again");

        (int Value, bool IsDefaultSource, bool IsGood)[,] sudokuBoard = SudokuBoardGeneral.ReadSudokuBoardFromFile(sudokuFilePathName);
        
        _sourceSudoku = new(sudokuBoard)
        {
            Fitness = CalculateFitness(sudokuBoard)
        };

        _bestSudokuSolution = new(new (int Value, bool IsDefaultSource, bool IsGood)[9,9]);
        Array.Copy(_sourceSudoku.Board, _bestSudokuSolution.Board, _sourceSudoku.Board.Length);
        
        var result = Solve();
        Console.WriteLine($"Best sudoku with '{result.ShowFitness(true)}'");
        GeneralIO.ShowBoard(result.Board);
        
        return Task.CompletedTask;
    }
    
    private bool ValidateSettings(string sudokuFilePathName)
    {
        var validationContext = new ValidationContext(_settings, null, null);
        var validationResults = new List<ValidationResult>();

        return Validator.TryValidateObject(_settings, validationContext, validationResults, true);
    }
    
    /// <summary>
    /// Solve sourceSudoku using genetic algorithm
    /// </summary>
    /// <returns><see cref="Sudoku"/> with solved board</returns>
    private Sudoku Solve()
    {
        List<Sudoku> sudokuList = new();
        
        CreateFirstPopulation(ref sudokuList);
        ShowPopulationInfo(sudokuList, true, isFirstPopulation: true);

        FillUpSudoku(ref sudokuList);

        RunEvolutionProcess(sudokuList);

        _bestSudokuSolution.Fitness = CalculateFitness(_bestSudokuSolution.Board);
        return _bestSudokuSolution;
    }
    
    /// <summary>
    /// Duplicate source sudoku to list that's count is population number.
    /// </summary>
    private void CreateFirstPopulation(ref List<Sudoku> sudokuList)
    {
        for (var i = 0; i < _settings.PopulationSize; i++)
        {
            var newSudoku = new Sudoku(new (int Value, bool IsDefaultSource, bool IsGood)[9,9]);
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
            if (showBoard)
                GeneralIO.ShowBoard(_sourceSudoku.Board);
            return;
        }
        
        for (var index = 0; index < sudokuList.Count; index++)
        {
            var sudoku = sudokuList[index];
            
            Console.WriteLine($"Sudoku nr {index + 1}. Fitness: {sudoku.ShowFitness(returnString: true)}");
            if (showBoard)
                GeneralIO.ShowBoard(sudoku.Board);
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
                if (sudoku.Board[row, col].Value != 0) continue;
                
                var possibilities = FindPossibleSolutionsForSpecificBoard(sudoku.Board);
                var possibleSolutionsForThisRowCol = possibilities.First(c => c.Row == row && c.Column == col).PossibleSolutions;

                if (possibleSolutionsForThisRowCol!.Count == 0)
                    continue;
                
                var randomSolutionValue = possibleSolutionsForThisRowCol![_random.Next(0, possibleSolutionsForThisRowCol.Count)];

                sudoku.Board[row, col].Value = randomSolutionValue;
            }
            
            sudoku.Fitness = CalculateFitness(sudoku.Board);
        }
    }

    private void RunEvolutionProcess(List<Sudoku> sudokuList)
    {
        var population = sudokuList;
        
        for (var generation = 0; generation < _settings.MaxGenerations; generation++)
        {
            foreach (var sudoku in population)
                sudoku.Fitness = CalculateFitness(sudoku.Board);
            
            ShowGenerationPopulationInfo(population, generation);

            if (population.Any(s => s.Fitness == 81))
            {
                _bestSudokuSolution = population.First(s => s.Fitness == 81);
                break;
            }

            _bestSudokuSolution = population.OrderByDescending(c => c.Fitness).First();
            
            var parents = SelectParents(population);

            var newPopulation = new List<Sudoku>();
            for (var i = 0; i < parents.Count; i += 2)
            {
                // Cross over parents
                var children = Crossover(parents[i], parents[i + 1]);
                newPopulation.AddRange(children);
            }

            population = newPopulation;
        }
    }

    /// <summary>
    /// Shows on console actual population information. If its first population then show global fitness of source board. If not, then shows fitness for every board.
    /// </summary>
    /// <param name="sudokuList"></param>
    /// <param name="generationNumber">Number that show for which generation its population info</param>
    /// <param name="showBoard">Shown board to console for every sudoku in population</param>
    private void ShowGenerationPopulationInfo(List<Sudoku> sudokuList, int generationNumber, bool showBoard = false)
    {
        Console.WriteLine($"\nGeneration nr {generationNumber + 1}.");
        for (var index = 0; index < sudokuList.Count; index++)
        {
            var sudoku = sudokuList[index];
            
            Console.WriteLine($"Sudoku nr {index + 1}. Fitness: {sudoku.ShowFitness(returnString: true)}");
            if (showBoard)
                GeneralIO.ShowBoard(sudoku.Board);
        }
    }
    
    /// <summary>
    /// Calculate for board
    /// </summary>
    /// <param name="board"></param>
    /// <param name="firstCalculate">Check if it's the first calculation (during creating constructor)</param>
    /// <returns>Fitness is an int number of how good the solution is</returns>
    private int CalculateFitness((int Value, bool IsDefaultSource, bool IsGood)[,] board, bool firstCalculate = false)
    {
        for (var row = 0; row < 9; row++)
        for (var col = 0; col < 9; col++)
        {
            if (board[row, col].Value == 0)
            {
                board[row, col].IsGood = false;
                continue;
            }
            
            if (!IsUnSafe(board, row, col))
            {
                board[row, col].IsGood = true;
                continue;
            }
                
            board[row, col].IsGood = false;
        }
        
        var newFitness = 81 - GetWrongSolutions(board);

        return newFitness;
    }
    
    /// <summary>
    /// Selection of the best half of population parents
    /// </summary>
    /// <returns>List of <see cref="Sudoku"/> that contains half of population number best parents to reproduce // the highest Fitness rate</returns>
    private List<Sudoku> SelectParents(List<Sudoku> sudokuList) => 
        sudokuList.OrderByDescending(sudoku => sudoku.Fitness).Take((_settings.PopulationSize / 2) % 2 == 0 ? _settings.PopulationSize / 2 : (_settings.PopulationSize / 2) + 1).ToList();

    private List<Sudoku> Crossover(Sudoku parent1, Sudoku parent2)
    {
        var children = new List<Sudoku>();
        for (var i = 0; i < 2; i++)
        {
            var child = CrossParentsIntoChildAndMutate(new (parent1, parent2));
            children.Add(child);
        }

        return children;
    }

    private Sudoku CrossParentsIntoChildAndMutate((Sudoku parent1, Sudoku Parent2) parents)
    {
        var fatherBoard = parents.parent1.Board;
        var motherBoard = parents.Parent2.Board;

        var childBoard = new (int Value, bool IsDefaultSource, bool IsGood)[9,9];
        Array.Copy(_sourceSudoku.Board, childBoard, _sourceSudoku.Board.Length);

        for (var row = 0; row < 9; row++)
        for (var col = 0; col < 9; col++)
        {
            if (childBoard[row, col].IsDefaultSource)
                continue;

            childBoard[row, col] = _random.NextDouble() <= 0.5 ? fatherBoard[row, col] : motherBoard[row, col];
            
            if (_random.NextDouble() <= _settings.MutationRate)
            {
                // Todo: wykonuje sie w nieskonczonosc i nie moze program sie wykonac
                getRandomNumber:
                var num = _random.Next(1, 9);
                if (!IsSafe(childBoard, num, row, col, false, true))
                    goto getRandomNumber;
                
                childBoard[row, col].Value = num;
            }
        }

        var child = new Sudoku(childBoard);
        return child;
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
    private List<SudokuCellPossibilities> FindPossibleSolutionsForSpecificBoard((int Value, bool IsDefaultSource, bool IsGood)[,] board)
    {
        var possibilities = new List<SudokuCellPossibilities>();
        FindPossibleSolutions(board, ref possibilities);
    
        return possibilities;
    }
    
    // Methods used to find possible sourceSudoku cells solutions
    private void FindPossibleSolutions((int Value, bool IsDefaultSource, bool IsGood)[,] board, ref List<SudokuCellPossibilities> possibilities)
    {
        for (var row = 0; row < 9; row++)
            for (var col = 0; col < 9; col++)
            {
                if (board[row, col].Value != 0)
                {
                    var cell = new SudokuCellPossibilities(row, col, null);
                    cell.Value = board[row, col].Value;
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

    private int GetWrongSolutions((int Value, bool IsDefaultSource, bool IsGood)[,] board)
    {
        var wrongSolutions = 0;
        
        for (var row = 0; row < 9; row++)
        for (var col = 0; col < 9; col++)
        {
            if (board[row, col].Value == 0)
                continue;

            //if (!IsUnSafe(board, row, col)) continue;
            
            if (!board[row, col].IsGood)
                wrongSolutions++;
        }

        return wrongSolutions;
    }
    private bool IsSafe((int Value, bool IsDefaultSource, bool IsGood)[,] workingBoard, int row, int col, int num, bool checkRowCol = true, bool check3x3 = true)
    {
        // Check Row and Column
        if (checkRowCol)
            for (var i = 0; i < 9; i++)
                if (workingBoard[row, i].Value == num || workingBoard[i, col].Value == num)
                    return false;
    
        // Check 3x3
        if (check3x3)
        {
            var startRow = row - row % 3;
            var startCol = col - col % 3;
            for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++)
                if (workingBoard[i + startRow, j + startCol].Value == num)
                    return false;
        }
    
        return true;
    }
    
    private bool IsUnSafe((int Value, bool IsDefaultSource, bool IsGood)[,] workingBoard, int row, int col)
    {
        var counter = 0;
        var num = workingBoard[row, col].Value;
        
        // Check Row and Column
        for (var i = 0; i < 9; i++)
        {
            if (workingBoard[row, i].Value == num || workingBoard[i, col].Value == num)
            {
                counter++;

                if ((row == 0 && col == 0) || (row == 9 && col == 9) || (row == 0 && col == 9) ||
                    (row == 9 && col == 0))
                {
                    if (counter > 2)
                        return true;
                }
                else
                    if (counter >= 2)
                        return true;
            }
        }
    
        return false;
    }
}