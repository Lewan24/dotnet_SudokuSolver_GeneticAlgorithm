/*  Application work scheme
 *
 *  1. Create custom board
 *  2. Call Solve
 *  2.1. Duplicate source board to make 10 same boards
 *  2.2. For every duplicated board call TrySolve
 *  2.2.1. Enter random solutions in random empty cells
 *  2.2.2. After filling up board. Calculate Fitness for every board
 *  2.3. After generating solution, get 5 best solutions with the biggest fitness rate
 *          reproduce to make 10 children. Use genetic algorithm there. Reproduce with random data from parent1 or parent2. // or like half of parent1 nd half of parent2
 *          Also mutate if chance is matched.
 *  2.4. Repeat 2.2 - 2.3 for children until there is a board with fitness 81 // 81 (cells number in sourceSudoku) means there is not any wrong data inserted and not a one empty cell
 *  3. Return solved sourceSudoku board
 *  4. Show the result on console
 */

/*  Genetic algorithm job
 *
 *  0. General
 *      PopulationSize: 10 // number of boards
 *      Goal for every board: Get the best Fitness value
 *      Match to go to next step: Only 5 best board are gonna further. These which are with the biggest rate of Fitness
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
 *      Mutation could be also something like just swapping values in 3x3 sections
 */

using Application.Data.Interfaces;
using Application.Data.Services;

var filePathWithSudokuBoard = "sudoku.txt";

var serviceSettings = new ServiceSettings()
{
  PopulationSize = 10,
  MaxGenerations = 100,
  MutationRate = 0.1f
};

ISudokuSolverService sudokuSolver = new SudokuSolverService(serviceSettings);

try
{
    await sudokuSolver.Run(filePathWithSudokuBoard);
}
catch (Exception e)
{
    var consoleForeground = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"There was a problem while solving a sudoku. Error: {e.Message}");
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Base Exception: {e}");
    Console.ForegroundColor = consoleForeground;
}