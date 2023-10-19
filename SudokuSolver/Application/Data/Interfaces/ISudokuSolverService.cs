namespace Application.Data.Interfaces;

public interface ISudokuSolverService
{
    Task Run(string sudokuFilePathName);
}