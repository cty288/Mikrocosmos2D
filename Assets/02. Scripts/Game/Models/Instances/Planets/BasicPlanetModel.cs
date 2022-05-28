using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mikrocosmos
{
    public class BasicPlanetModel : AbstractBasePlanetModel
    {
        //Write a program to solve a Sudoku puzzle by filling the empty cells
        //Input
        //A puzzle string
        //Output
        //The solution string
        //If no solution exists, return the original puzzle string.
        //Example
        //Input:
        //[
        //  ["5","3",".",".","7",".",".",".","."],
        //  ["6",".",".","1","9","5",".",".","."],
        //  [".","9","8",".",".",".",".","6","."],
        //  ["8",".",".",".","6",".",".",".","3"],
        //  ["4",".",".","8",".","3",".",".","1"],
        //  ["7",".",".",".","2",".",".",".","6"],
        //  [".","6",".",".",".",".","2","8","."],
        //  [".",".",".","4","1","9",".",".","5"],
        //  [".",".",".",".","8",".",".","7","9"]
        //]
        //Output:
        //[
        //  ["5","3","4","6","7","8","9","1","2"],
        //  ["6","7","2","1","9","5","3","4","8"],
        //  ["1","9","8","3","4","2","5","6","7"],
        //  ["8","5","9","7","6","1","4","2","3"],
        //  ["4","2","6","8","5","3","7","9","1"],
        //  ["7","1","3","9","2","4","8","5","6"],
        //  ["9","6","1","5","3","7","2","8","4"],
        //  ["2","8","7","4","1","9","6","3","5"],
        //  ["3","4","5","2","8","6","1","7","9"]
        //]
        //Explanation:
        //Note:
        //A Sudoku puzzle...
        //The Sudoku board could be partially filled, where empty cells are filled with the character '.'.
        //A partially filled sudoku which is valid.
        //The Sudoku board could be filled without any information on some cells, where empty cells are filled with the character '0'.
        //A partially filled sudoku which is not valid.
        //If there is no solution possible, return the original puzzle.
        //The given board contain only digits 1-9 and the character '.'.
        //The given board size is always 9x9.
        public string[][] solveSudoku(string[][] board)
        {
            if (board == null || board.Length == 0) return board;
            int n = board.Length;
            int m = board[0].Length;
            int[,] row = new int[n, m];
            int[,] col = new int[n, m];
            int[,] block = new int[n, m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (board[i][j] != ".")
                    {
                        int num = int.Parse(board[i][j]);
                        row[i, num - 1] = 1;
                        col[j, num - 1] = 1;
                        block[i / 3 * 3 + j / 3, num - 1] = 1;
                    }
                }
            }

            dfs(board, row, col, block, 0, 0);
            return board;
        }

        private void dfs(string[][] board, int[,] row, int[,] col, int[,] block, int i, int i1)
        {
            int n = board.Length;
            int m = board[0].Length;
            if (i == n)
            {
                return;
            }
            if (i1 == m)
            {
                dfs(board, row, col, block, i + 1, 0);
                return;
            }
            if (board[i][i1] != ".")
            {
                dfs(board, row, col, block, i, i1 + 1);
                return;
            }
            for (int j = 0; j < 9; j++)
            {
                if (row[i, j] == 0 && col[i1, j] == 0 && block[i / 3 * 3 + i1 / 3, j] == 0)
                {
                    board[i][i1] = (j + 1).ToString();
                    row[i, j] = 1;
                    col[i1, j] = 1;
                    block[i / 3 * 3 + i1 / 3, j] = 1;
                    dfs(board, row, col, block, i, i1 + 1);
                    board[i][i1] = ".";
                    row[i, j] = 0;
                    col[i1, j] = 0;
                    block[i / 3 * 3 + i1 / 3, j] = 0;
                }
            }   
        }
    }
}