using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using app.Services;

[assembly: InternalsVisibleTo("tests")]

namespace app.Models
{
    // TODO: or just the simplest array possible?
    public class BoardState
    {
        private string _fen;
        private bool _whiteTurn;
        private bool _modified;
        private StringBuilder[] _rows;

        public BoardState(string fenStr)
        {
            // TODO: validate FEN
            _fen = string.Empty;
            _whiteTurn = fenStr.Split(' ')[1][0].ToColorFromTurn() == ColorEnum.White;
            _modified = false;
            _rows = null;
            ConstructState(fenStr);
        }
        
        private void ConstructState(string fenStr)
        {
            var fields = fenStr.Split(' ');
            var splits = fields[0].Split('/');

            _rows = new StringBuilder[8];
            for (var row = 0; row < 8; row++)
            {
                _rows[row] = new StringBuilder(8);
                var line = Expand(splits[row]);
                _rows[row].Append(line);
            }

            _whiteTurn = fields[1] == "w";

            ConstructFen();
        }

        private void ConstructFen()
        {
            var fen = new StringBuilder();
            for (var row = 0; row < 8; row++)
            {
                fen.Append(Serialize(_rows[row].ToString()));
                fen.Append('/');
            }

            fen.Remove(fen.Length-1, 1);
            fen.Append(' ');
            fen.Append(GetTurn());
            _fen = fen.ToString();

            _modified = false;
        }
        
        public string Fen
        {
            get
            {
                if (_modified) ConstructFen();
                return _fen;
            }
        }

        
        public IEnumerable<char> Captures => GetCaptures();
        private IEnumerable<char> GetCaptures()
        {
            // TODO: fix unittests for this
            var res = new List<char>();
            var fullBoard = "rnbqkbnrppppppppPPPPPPPPRNBQKBNR";

            // pieces that are left in fullBoard, move to captures
            foreach (var chr in Fen)
            {
                if (chr == ' ') break; // we're done
                if (chr == '/' || chr.IsAlpha()) continue;

                var i = fullBoard.IndexOf(chr);
                fullBoard = fullBoard.Remove(i, 1);
            }

            res.AddRange(fullBoard.Select(x => x));

            return res;
        }

        public ColorEnum WhosTurn => GetTurn().ToColorFromTurn();

        public char? this[Position pos]
        {
            get
            {
                if (pos.Col <= 0 || pos.Col > 8 || pos.Row <= 0 || pos.Row > 8) return null;
                
                var p = _rows[pos.Row - 1][pos.Col - 1];
                return p == '_' ? null : p;
            }
        }
        
        private char GetTurn()
        {
            return _whiteTurn ? 'w' : 'b';
        }

        
        public ICollection<PieceInfo> GetPieces(ColorEnum who)
        {
            var res = new List<PieceInfo>();
            
            for (var rowNo = 0; rowNo < _rows.Length; rowNo++)
            {
                var row = _rows[rowNo];
                for (var col = 0; col < row.Length; col++)
                {
                    var piece = row[col];
                    if (piece == '_' || who != piece.ToColorFromPiece()) continue;
                    res.Add(new PieceInfo(piece, new(col+1, rowNo+1)));
                }
            }

            return res;
        }
        
        public Position GetPosition(char piece)
        {
            var rowNo = 0;
            foreach (var row in _rows)
            {
                rowNo++;
                var index = row.IndexOf(piece);
                if (index == -1) continue;

                return new(index + 1, rowNo);
            }

            throw new Exception("No such piece found");
        }

        internal string Expand(string line)
        {
            var res = string.Empty;
            foreach (var chr in line)
            {
                // if number, jump steps
                if (chr.IsAlpha())
                {
                    var spaces = int.Parse(chr.ToString());
                    for (var s = 0; s < spaces; s++)
                    {
                        res += "_";
                    }
                    continue;
                }

                if (chr == ' ') break;

                res += chr.ToString();
            }

            if (res.Length != 8) res = res.PadRight(8, '_');

            return res;
        }

        private void InternalMove(Position from, Position to, bool changeTurn=true)
        {
            _rows[to.Row-1][to.Col-1] = _rows[from.Row-1][from.Col-1];
            _rows[from.Row-1][from.Col-1] = '_';
            _whiteTurn = changeTurn ? !_whiteTurn : _whiteTurn;
            
            _modified = true;
        }

        internal string Serialize(string str)
        {
            if (!str.Any(x => char.IsUpper(x) || char.IsLower(x))) return "8";
            var res = new StringBuilder(20);
            var space = 0;
            
            foreach (var chr in str)
            {
                if (chr == '_')
                {
                    space++;
                    continue;
                }

                if (chr == '/')
                {
                    res.Append(chr);
                    break;
                }

                if (space > 0)
                {
                    res.Append(space);
                    space = 0;
                }

                res.Append(chr);
            }

            return res.ToString();
        }

        public BoardState Move(Position from, Position to)
        {
            // create a new state and do the move
            var res = new BoardState(Fen);
            res.InternalMove(from, to);
            Debug.Assert(res.Fen.Contains('k') && res.Fen.Contains('K'), 
                "You can't create a state with missing kings!");
            return res;
        }

        public BoardState Promote(Position pos, char piece)
        {
            var res = new BoardState(Fen);
            res.PlacePiece(pos, piece);
            // res[pos] = piece;
            return res;
        }

        private void PlacePiece(Position pos, char piece)
        {
            _rows[pos.Row - 1][pos.Col - 1] = piece;
            _modified = true;
        }

        public void Capture(Position pos)
        {
            _rows[pos.Row - 1][pos.Col - 1] = '_';
            _modified = true;
        }

        public void MoveWithState(Position from, Position to)
        {
            InternalMove(from, to, changeTurn:false);
        }
    }
}