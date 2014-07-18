using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2048 {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private Game game = new Game();

        /// <summary>
        /// Flushes the game state to the screen.
        /// </summary>
        private void drawGame() {
            Color[] scheme = {
                                 Color.FromArgb(0x000000),
                                 Color.FromArgb(0xeee4da),
                                 Color.FromArgb(0xede0c8),
                                 Color.FromArgb(0xf2b179), 
                                 Color.FromArgb(0xf59563),
                                 Color.FromArgb(0xf67c5f),
                                 Color.FromArgb(0xf65e3b),
                                 Color.FromArgb(0xedcf72),
                                 Color.FromArgb(0xedcc61),
                                 Color.FromArgb(0xedc850),
                                 Color.FromArgb(0xedc53f),
                                 Color.FromArgb(0xedc22e),
                                 Color.FromArgb(0x000000),
                                 Color.FromArgb(0x000000),
                                 Color.FromArgb(0x000000),

                             };

            int boxWidth = pictureBox1.Width / 4;

            g.Clear(Color.LightGray);

            int border = 5;

            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    Color col = Color.LightGray;
                    if (game.GetSquare(i, j) != 0) {
                        col = scheme[game.GetSquare(i, j)];
                        // MessageBox.Show(((int)Math.Log(game.GetSquare(i, j), 2)).ToString() + ",   " + Math.Log(game.GetSquare(i, j), 2).ToString());
                    }
                    col = Color.FromArgb(255, col);
                    
                    g.FillRectangle(new SolidBrush(col), 
                        new Rectangle(i*boxWidth + border, j*boxWidth + border, boxWidth - border*2, boxWidth - border*2));


                    if (game.GetSquare(i, j) != 0) {
                        string text2 = ((int)Math.Pow(2, game.GetSquare(i, j))).ToString();

                        using (Font font2 = new Font("Arial", 12, FontStyle.Bold, GraphicsUnit.Point)) {
                            Rectangle rect2 = new Rectangle(i * boxWidth, j * boxWidth, boxWidth, boxWidth);

                            // Create a TextFormatFlags with word wrapping, horizontal center and 
                            // vertical center specified.
                            TextFormatFlags flags = TextFormatFlags.HorizontalCenter |
                                TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;

                            // Draw the text and the surrounding rectangle.
                            TextRenderer.DrawText(g, text2, font2, rect2, Color.Black, flags);
                            
                        }
                    }
                    
                }
            }
            
            g.Flush();
        }

        AI ai = new AI(500);

        Graphics g;
        private void Form1_Load(object sender, EventArgs e) {
            this.KeyDown += Form1_KeyDown;
            MessageBox.Show("Success");
            g = pictureBox1.CreateGraphics();

            drawGame();
        }

        void Form1_KeyDown(object sender, KeyEventArgs e) {

            if (e.KeyCode == Keys.A) {
                game.Move((int)Direction.Left);
            } else if (e.KeyCode == Keys.D) {
                game.Move((int)Direction.Right);
            } else if (e.KeyCode == Keys.W) {
                game.Move((int)Direction.Up);
            } else if (e.KeyCode == Keys.S) {
                game.Move((int)Direction.Down);
            }
            drawGame();

            writeGame();
        }

        void writeGame() {
            System.Diagnostics.Debug.WriteLine(game.EmptyCount());
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    System.Diagnostics.Debug.Write(game.GetSquare(j, i).ToString() + "\t");
                }
                System.Diagnostics.Debug.Write("Evaluation: " + ai.evaluate(game).ToString() + "\n");
            }
            System.Diagnostics.Debug.Write("\n");

            // GUI
            textBox1.Text = "";
            textBox1.Text += "Depth: " + ai.finalDepth.ToString() + "\r\n";
            textBox1.Text += "Current heuristic evaluation: " + ai.evaluate(game).ToString() + "\r\n\r\n";
            for (int i = 0; i < ai.finalDepth; i+=2) {
                textBox1.Text += "At depth " + i.ToString() + ":\t";
                textBox1.Text += ai.evaluations[i].Item1.ToString().Substring(0, Math.Min(5, ai.evaluations[i].Item1.ToString().Length)) + "\t" + ai.evaluations[i].Item2.ToString() + "\r\n";
            }
            
        }

        private void btnReset_Click(object sender, EventArgs e) {
            game = new Game();
            drawGame();
        }

        private void btnStart_Click(object sender, EventArgs e) {
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e) {
            Direction d = ai.calculate(game);
            game.Move((int)d);
            drawGame();
            writeGame();
        }

        private void btnStop_Click(object sender, EventArgs e) {
            timer1.Stop();
        }

        private void btnStep_Click(object sender, EventArgs e) {
            timer1.Stop();
            timer1_Tick(null, null);
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e) {
            ai.updateSpeed(numericUpDown1.Value);
        }
    }

    public enum Direction {
        Up = 0, Down = 1, Left = 2, Right = 3
    }

  
}