using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace _2048 {
    public class AI {

        public Direction calculate(Game state) {
            // iterative deepening will do.
            DateTime start = DateTime.Now;
            Tuple<double, Direction> result = new Tuple<double, Direction>(0, Direction.Right);
            for (int i = 1; i < 100; i+=2) {
                depthRemaining = i;
                wcmt = new Dictionary<ulong, double>();
                acmt = new Dictionary<ulong, Tuple<double, Direction>>();
                wctt = new Dictionary<ulong, double>();
                actt = new Dictionary<ulong, double>();
                result = AverageCaseMyTurn(state, true);
                evaluations[i - 1] = result;

                System.Diagnostics.Debug.WriteLine(result.Item2.ToString() + ", " + result.Item1.ToString());
                if ((DateTime.Now - start).TotalMilliseconds > timeout) {
                    System.Diagnostics.Debug.Write("Depth: " + i.ToString() + "\n");

                    finalDepth = i;
                    return result.Item2;
                }
            }
            return result.Item2;
        }

        int depthRemaining;
        int timeout;

        public int finalDepth;
        public Tuple<double, Direction>[] evaluations = new Tuple<double, Direction>[100];

        public AI(int ms) {
            timeout = ms;
        }

        Direction lastMove;
        Dictionary<ulong, double> wcmt, wctt, actt;
        Dictionary<ulong, Tuple<double, Direction> > acmt;

        private double WorstCaseMyTurn(Game state) {
            //if (depthRemaining == 0) return evaluate(state);
            ulong key = state.SerializePosition();
            if (wcmt.ContainsKey(key)) return wcmt[key];

            double answer = double.MinValue;
            if (state.IsGameOver()) return answer;
            foreach (Direction i in Enum.GetValues(typeof(Direction))) {
                // Try this direction.
                Game next = state.Copy();
                int result = next.Move((int)i, false);
                if (result == 1) continue;

                depthRemaining--;
                double score = WorstCaseTheirTurn(next);
                if (score > answer) {
                    answer = score;
                    lastMove = i;
                }
                depthRemaining++;
            }

            wcmt[key] = answer;
            return answer;
        }

        static Direction[] directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

        private Tuple<double, Direction> AverageCaseMyTurn(Game state, bool root = false) {
            Direction lastMove = Direction.Up;
            //if (depthRemaining == 0) return new Tuple<double,Direction>(evaluate(state), Direction.Right);
            ulong key = state.SerializePosition();
            if (acmt.ContainsKey(key)) return acmt[key];


            double answer = double.MinValue;
            if (state.IsGameOver()) return new Tuple<double,Direction>(answer, Direction.Right);
            foreach (Direction i in directions) {
                // Try this direction.
                Game next = state.Copy();
                int result = next.Move((int)i, false);
                if (result == 1) {
                    if (root) System.Diagnostics.Debug.Write(i.ToString() + ": Blocked\n");

                    continue;
                }

                depthRemaining--;
                double score = AverageCaseTheirTurn(next);
                if (score > answer) {
                    answer = score;
                    lastMove = i;
                    if (root) System.Diagnostics.Debug.Write(lastMove.ToString() + " is now the best\n");
                }
                depthRemaining++;

                if (root) System.Diagnostics.Debug.Write(i.ToString() + ": " + answer.ToString() + "\n");
            }

            if(root) System.Diagnostics.Debug.Write(lastMove.ToString() + " is still the best\n");
            if (root) System.Diagnostics.Debug.Write(acmt.Count.ToString() + " is the N of nodes\n");
            acmt[key] = new Tuple<double,Direction>(answer, lastMove);
            return new Tuple<double,Direction>(answer, lastMove);
        }

        private double WorstCaseTheirTurn(Game state) {
            if (depthRemaining == 0) return evaluate(state);
            
            double answer = double.MinValue;

            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    if (state.GetSquare(i, j) == 0) {
                        Game next = state.Copy();
                        next.SetState(i, j, 2);
                        answer = Math.Min(answer, WorstCaseMyTurn(next));
                    }
                }
            }

            return answer;
        }

        private double AverageCaseTheirTurn(Game state) {
            if (depthRemaining == 0) return evaluate(state);

            double answer = 0;

            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    if (state.GetSquare(i, j) == 0) {
                        Game next = state.Copy();
                        next.SetState(i, j, 2);
                        answer += AverageCaseMyTurn(next).Item1 / state.EmptyCount();
                    }
                }
            }

            return answer;
        }

        private double _increaseHeuristic(int d, int l) {
            if (d == 0) return HEURISTIC_EQUAL_MULTIPLIER * l;
            if (d > 0) return HEURISTIC_POSITIVE_MULTIPLIER / d * l;
            int s = l - d;
            return d * HEURISTIC_NEGATIVE_MULTIPLIER * s;
        }

        private static double HEURISTIC_INCREASE_MULTIPLIER = 2.0;
        private static double HEURISTIC_EMPTY_MULTIPLIER = 1.0;
        private static double HEURISTIC_EQUAL_MULTIPLIER = 0.4;
        private static double HEURISTIC_POSITIVE_MULTIPLIER = 0.2;
        private static double HEURISTIC_NEGATIVE_MULTIPLIER = 0.1;

        /// <summary>
        /// A heuristic function that describes the fitness of the game state.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public double evaluate(Game state) {
            double score = state.EmptyCount() * HEURISTIC_EMPTY_MULTIPLIER;
            // 1 points for ones next to each other, 0.5 points for 1 neighbour

            // going left & right
            for (int y = 0; y < 4; y++) {
                double thisRow1 = 0, thisRow2 = 0;
                for (int x = 0; x < 3; x++) {
                    thisRow1 += _increaseHeuristic(state.state[x + 1, y] - state.state[x, y], state.state[x+1, y]);
                }

                for (int x = 3; x > 0; x--) {
                    thisRow2 += _increaseHeuristic(state.state[x - 1, y] - state.state[x, y], state.state[x-1, y]);
                }

                score += Math.Max(thisRow2, thisRow1) * HEURISTIC_INCREASE_MULTIPLIER;
            }

            // going up & down
            for (int x = 0; x < 4; x++) {
                double thisRow1 = 0, thisRow2 = 0;
                for (int y = 0; y < 3; y++) {
                    thisRow1 += _increaseHeuristic(state.state[x, y + 1] - state.state[x, y], state.state[x, y + 1]);
                }

                for (int y = 3; y > 0; y--) {
                    thisRow2 += _increaseHeuristic(state.state[x, y - 1] - state.state[x, y], state.state[x, y - 1]);
                }

                score += Math.Max(thisRow2, thisRow1) * HEURISTIC_INCREASE_MULTIPLIER;
            }
            return score;
        }



        internal void updateSpeed(decimal p) {
            timeout = (int)p;
        }
    }


}
