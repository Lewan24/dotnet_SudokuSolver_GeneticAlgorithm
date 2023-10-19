using Core.Entities;

namespace Application.Data.Static_Classess;

public static class SudokuBoardGeneral
{
    public static Cell[,] ReadSudokuBoardFromFile(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        var sudokuBoard = new Cell[9, 9];

        for (var i = 0; i < 9; i++)
        {
            var line = lines[i];
            for (var j = 0; j < 9; j++)
            {
                var charValue = line[j];
                var value = int.Parse(charValue.ToString());

                var isDefaultSource = value != 0;
                var isGood = value != 0;

                sudokuBoard[i, j] = new Cell(value, isDefaultSource, isGood);
            }
        }

        return sudokuBoard;
    }
}