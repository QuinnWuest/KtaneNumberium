using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Rnd = UnityEngine.Random;
using KModkit;

public class NumberiumScript : MonoBehaviour
{
    public KMBombModule Module;
    public KMBombInfo BombInfo;
    public KMAudio Audio;

    private int _moduleId;
    private static int _moduleIdCounter = 1;
    private bool _moduleSolved;

    struct NumberiumOp
    {
        public string operation;
        public int value;
        public NumberiumOp(string op, int val)
        {
            operation = op;
            value = val;
        }
    }
    private static readonly string[] _operationNames = new string[3] { "add", "subtract", "multiply" };

    private NumberiumOp[] _operationBoard = new NumberiumOp[16];
    private int _initPosition;
    private int _initValue;
    private int _goalPosition;

    private int _currentPosition;
    private int _currentValue;

    private void Start()
    {
        _moduleId = _moduleIdCounter++;
        GeneratePuzzle();
    }

    private void GeneratePuzzle()
    {
        tryAgain:
        _initValue = 1;
        int[] positions = Enumerable.Range(0, 16).ToArray().Shuffle().Take(5).ToArray();
        List<int> visited = new List<int>();
        List<string> actions = new List<string>();
        _operationBoard[positions[1]] = new NumberiumOp(_operationNames[Rnd.Range(0, 3)], Rnd.Range(2, 8));
        _operationBoard[positions[2]] = new NumberiumOp(_operationNames[Rnd.Range(0, 3)], Rnd.Range(2, 8));
        _operationBoard[positions[3]] = new NumberiumOp(_operationNames[Rnd.Range(0, 3)], Rnd.Range(2, 8));
        bool[] operatorsVisited = new bool[3];
        _initPosition = positions[0];
        _goalPosition = positions[4];
        var moveCount = 0;
        visited.Add(_initPosition);
        while (_initPosition != _goalPosition)
        {
            var dirs = GetMovements(_initPosition, visited);
            if (dirs.Count == 0)
                goto tryAgain;
            var moves = new int[4] { -4, 1, 4, -1 };
            var rndDir = Rnd.Range(0, dirs.Count);
            _initPosition = _initPosition += moves[dirs[rndDir]];
            if (_initPosition == positions[1])
            {
                _initValue = ReturnNumberiumOperation(_initValue, _operationBoard[positions[1]]);
                operatorsVisited[0] = true;
            }
            else if (_initPosition == positions[2])
            {
                _initValue = ReturnNumberiumOperation(_initValue, _operationBoard[positions[2]]);
                operatorsVisited[1] = true;
            }
            else if (_initPosition == positions[3])
            {
                _initValue = ReturnNumberiumOperation(_initValue, _operationBoard[positions[3]]);
                operatorsVisited[2] = true;
            }
            else
                _initValue++;
            actions.Add("Move " + new[] { "up", "right", "down", "left" }[dirs[rndDir]] + " to " + GetCoord(_initPosition) + " to get " + _initValue);
            visited.Add(_initPosition);
            moveCount++;
        }
        if (operatorsVisited.Contains(false) || moveCount < 6)
            goto tryAgain;
        string str = "";
        for (int i = 0; i < 16; i++)
        {
            if (i == positions[0])
                str += "[st]";
            else if (i == positions[1])
                str +=
                    _operationBoard[positions[1]].operation == "add" ? ("[+" + _operationBoard[positions[1]].value + "]") :
                    _operationBoard[positions[1]].operation == "subtract" ? ("[-" + _operationBoard[positions[1]].value + "]") :
                    ("[*" + _operationBoard[positions[1]].value + "]");
            else if (i == positions[2])
                str +=
                    _operationBoard[positions[2]].operation == "add" ? ("[+" + _operationBoard[positions[2]].value + "]") :
                    _operationBoard[positions[2]].operation == "subtract" ? ("[-" + _operationBoard[positions[2]].value + "]") :
                    ("[*" + _operationBoard[positions[2]].value + "]");
            else if (i == positions[3])
                str +=
                    _operationBoard[positions[3]].operation == "add" ? ("[+" + _operationBoard[positions[3]].value + "]") :
                    _operationBoard[positions[3]].operation == "subtract" ? ("[-" + _operationBoard[positions[3]].value + "]") :
                    ("[*" + _operationBoard[positions[3]].value + "]");
            else if (i == positions[4])
                str += "[en]";
            else
                str += "[--]";
            if (i % 4 == 3)
                str += "\n";
        }
        Debug.LogFormat("[Numberium #{0}] Board:\n{1}", _moduleId, str);
        Debug.LogFormat("[Numberium #{0}] Start at {1} with value 1.", _moduleId, GetCoord(positions[0]));
        for (int i = 0; i < actions.Count; i++)
            Debug.LogFormat("[Numberium #{0}] {1}.", _moduleId, actions[i]);
        Debug.LogFormat("[Numberium #{0}] End value: {1}.", _moduleId, _initValue);
    }

    private string GetCoord(int pos)
    {
        return "ABCD"[pos % 4].ToString() + "1234"[pos / 4].ToString();
    }

    private List<int> GetMovements(int pos, List<int> visited)
    {
        var list = new List<int>();
        if (pos % 4 > 0 && !visited.Contains(pos - 1))
            list.Add(3);
        if (pos % 4 < 3 && !visited.Contains(pos + 1))
            list.Add(1);
        if (pos / 4 > 0 && !visited.Contains(pos - 4))
            list.Add(0);
        if (pos / 4 < 3 && !visited.Contains(pos + 4))
            list.Add(2);
        return list;
    }

    private int ReturnNumberiumOperation(int initValue, NumberiumOp op)
    {
        if (op.operation == "add")
            return initValue + op.value;
        if (op.operation == "subtract")
            return initValue - op.value;
        if (op.operation == "multiply")
            return initValue * op.value;
        throw new NotImplementedException("An invalid operation was used: " + op.operation + op.value);
    }
}
