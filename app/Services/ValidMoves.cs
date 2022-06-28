using System;
using System.Collections.Generic;
using app.Models;

namespace app.Services
{
    public class ValidMoves
    {
        
        public static void CheckKnight(List<Position> captureOffset, List<Position> moveList)
            {
                captureOffset.AddRange(new List<Position>()
                {
                    new(-2, -1), new(-1, -2), new(1, -2), new(2, -1),
                    new(2, 1), new(1, 2), new(-1, 2), new(-2, 1)
                });
            }

        public static void CheckPawn(char pieceAtStartingPlace, List<Position> captureOffset, List<Position> moveList)
            {
                var moveOffset = pieceAtStartingPlace.IsUpper() ? -1 : 1;
                captureOffset.AddRange(new List<Position>()
                    {new(-1, moveOffset), new(1, moveOffset)});
                moveList.AddRange(new List<Position>() {new(0, moveOffset)});
                
            }

        public static void CheckKing(List<Position> captureOffset, List<Position> moveList)
            {
                /* - - - - -
                 * - C C C -
                 * - C X C - - - 
                 * - C C C -
                 * - - - - -
                 */
                captureOffset.AddRange(new List<Position>
                {
                    new(-1, -1), new(0, -1), new(1, -1),
                    new(-1, 0), new(1, 0),
                    new(-1, 1), new(0, 1), new(1, 1)
                });
            }

        public static void CheckRook(List<Position> captureOffset, List<Position> moveList)
            {
                /* - - C - -
                 * - - C - -
                 * C C X C C - - 
                 * - - C - -
                 * - - C - -
                 */
                captureOffset.AddRange(new List<Position>
                {
                    new(-1, 0), new(0, -1), new(1, 0), new(0, 1)
                });
                // checker.Invoke(from, state,validMoves, true, directions, true, new List<Position>(), true);
            }

        public static void CheckBishop(List<Position> captureOffset, List<Position> moveList)
            {
                /* C - - - C
                 * - C - C -
                 * - - X - - - - 
                 * - C - C -
                 * C - - - C
                 */
                
                // checker.Invoke(from, state,validMoves, true, directions, true, new List<Position>(), true);
            }

        public static void CheckQueen(List<Position> captureOffset, List<Position> moveList)
            {
                /* C - C - C
                 * - C C C -
                 * C C X C C - - 
                 * - C C C -
                 * C - C - C
                 */
                captureOffset.AddRange(new List<Position>
                {
                    new(-1, -1), new(1, -1), new(1, 1), new(-1, 1),
                    new(0, -1), new(1, 0), new(0, 1), new(-1, 0)
                });
                // checker.Invoke(from, state, validMoves, true, directions, true, new List<Position>(), true);
            }
    }
}