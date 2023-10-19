namespace Application.Data.Interfaces;

/// <summary>
/// Interface that contains all needed elements and methods to run and manage service
/// </summary>
public interface ISudokuSolverService
{
    Task Run(string sudokuFilePathName);
}