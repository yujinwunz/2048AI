using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2048 {
    public class Game {
        public int[,] state;
        int nEmpty;
        int nMoves;
        bool gameOver;

        public Game(bool fast = false) {
            state = new int[4, 4];
            if (fast) return;
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    state[i, j] = 0;
                }
            }
            nEmpty = 4 * 4;
            nMoves = 0;
            gameOver = false;

            seedTwo();
            seedTwo();
        }

        /// <summary>
        /// Damn it abstraction
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="n"></param>
        public void SetState(int x, int y, int n) {
            state[x, y] = n;
        }

        public bool IsGameOver() {
            return gameOver;
        }

        public int MoveCount() {
            return nMoves;
        }

        public int EmptyCount() {
            return nEmpty;
        }

        public int GetSquare(int x, int y) {
            return state[x, y];
        }

        static Random rand = new Random(System.DateTime.Now.Millisecond);

        /// <summary>
        /// Randomly creates two 2's on the board. Each 2 has a 1/16 chance of creating a 4.
        /// </summary>
        private void seedTwo() {
            for (int i = 0; i < 1; i++) {
                if (nEmpty == 0) continue; // nowhere to place next block.

                int toAdd = 1;
                if (rand.Next(0, 16) == 0) toAdd = 2;
                int index = rand.Next(0, nEmpty); // we will add to the index-1'th empty one we find.

                for (int j = 0; j < 4 && index >= 0; j++) {
                    for (int k = 0; k < 4 && index >= 0; k++) {
                        if (state[j, k] == 0) {
                            if (index-- == 0) {
                                state[j, k] = toAdd;
                                nEmpty--;
                            }
                        }
                    }
                }
            }

            // Check if game is still solvable.
            if (nEmpty > 0) return;

            bool stuck = true;
            int[] dx = { 1, 0, -1, 0 };
            int[] dy = { 0, 1, 0, -1 };
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    for (int k = 0; k < 4; k++) {
                        if (i + dx[k] < 0 || i + dx[k] >= 4 || j + dy[k] < 0 || j + dy[k] >= 4) break;
                        if (state[i, j] == state[i + dx[k], j + dy[k]]) stuck = false;
                    }
                }
            }

            if (stuck) gameOver = true;
        }

        static int[,] startx = {
                                {0, 1, 2, 3},
                                {0, 1, 2, 3},
                                {0, 0, 0, 0},
                                {3, 3, 3, 3},
                            };

        static int[,] starty = {
                                {0, 0, 0, 0},
                                {3, 3, 3, 3},
                                {0, 1, 2, 3},
                                {0, 1, 2, 3}

                            };

        static int[] dx = { 0, 0, 1, -1 };
        static int[] dy = { 1, -1, 0, 0 };


        /// <summary>
        /// Performs the move and updates the state. If move has no effect, then returns 1, otherwise 0.
        /// </summary>
        /// <param name="move">Direction of the move.</param>
        public int Move(int move, bool addNew = true) {
            // For the direction, collapse units starting from the back.
            bool changed = false;


            int d = move;

            for (int i = 0; i < 4; i++) {
                int cx = startx[d, i];
                int cy = starty[d, i];
                for (int j = 0; j < 3; j++) {
                    for (int k = 1; k <= 3; k++) {
                        if (cx + dx[d] * k < 0 || cx + dx[d] * k >= 4 || cy + dy[d] * k < 0 || cy + dy[d] * k >= 4) break;
                        if (state[cx, cy] == state[cx + dx[d] * k, cy + dy[d] * k]) {
                            if (state[cx, cy] != 0) state[cx, cy] += 1;
                            state[cx + dx[d] * k, cy + dy[d] * k] = 0;
                            if (state[cx, cy] != 0) {
                                nEmpty++;
                                changed = true;
                            }
                            break;
                        } else if (state[cx + dx[d] * k, cy + dy[d] * k] != 0) {
                            break;
                        }
                    }

                    cx += dx[d];
                    cy += dy[d];
                }
            }

            // Shift everything.
            for (int i = 0; i < 4; i++) {
                int cx = startx[d, i];
                int cy = starty[d, i];

                int gx = startx[d, i];
                int gy = starty[d, i];
                for (int j = 0; j < 4; j++) {
                    if (state[cx, cy] != 0) {
                        state[gx, gy] = state[cx, cy];
                        if (gx != cx || gy != cy) {
                            state[cx, cy] = 0;
                            changed = true;
                        }
                        gx += dx[d];
                        gy += dy[d];
                    }
                    cx += dx[d];
                    cy += dy[d];
                }
            }

            if (!changed) return 1;

            // Add twos.
            if (addNew) seedTwo();

            return 0;
        }

        public Game Copy() {
            Game game = new Game();
            Array.Copy(state, 0, game.state, 0, state.Length);
            game.nEmpty = nEmpty;
            game.nMoves = nMoves;
            game.gameOver = gameOver;

            return game;
        }

        public ulong SerializePosition() {
            ulong result = 0;
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    result = result << 4;
                    result = result | (ulong)state[i, j];
                }
            }
            return result;
        }
    }
}
