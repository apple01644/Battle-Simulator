using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Battle_Simulator
{

    public partial class Form1 : Form
    {
        private readonly Block[,] block = new Block[16, 16];
        private readonly float[,] ai = new float[16, 16];
        private readonly int[,] geo = new int[16, 16];
        private readonly StringFormat center_fmt = new StringFormat();

        private readonly BufferedGraphicsContext currentContext;
        private readonly BufferedGraphics myBuffer;

        delegate void ForPos(Point pos);
        delegate void ForBlock(Block o, int x, int y);
        delegate void ForConnectness(int x1, int y1, int x2, int y2);
        ForConnectness forConnectness;
        SolidBrush brs = new SolidBrush(Color.Black);

        private readonly Font font0 = new Font("Consolas", 11, FontStyle.Bold);

        //private readonly string[] TeamName = { "Nazis", "Soviet", "Italy", "France" };

        private const int MaxTeam = 9;
        private readonly int[] TeamDeath;
        private readonly int[] TeamRun;
        private readonly int[] TeamInto;
        private int[] TeamStartArmy;
        private bool[] TeamSurrender;
        private Color[] TeamColor = { Color.FromArgb(209, 48, 1), Color.FromArgb(27, 27, 126), Color.FromArgb(58, 113, 40), Color.FromArgb(209, 199, 65), Color.FromArgb(126, 129, 129), Color.FromArgb(124, 75, 165), Color.FromArgb(214, 92, 27), Color.FromArgb(46, 194, 187), Color.FromArgb(204, 36, 108) };
        private Point[] TeamLoc = { new Point(0, 0), new Point(15, 0), new Point(0, 15), new Point(15, 15), new Point(8, 0), new Point(0, 8), new Point(8, 15), new Point(15, 8), new Point(8, 8) };

        //private const int StartArmy = 150_000;
        private const int MaxArmyInField = 500_000;
        private const int MaxArmyInBattle = 860_000;

        private void Button1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.T:
                    block[0, 10] = new Block()
                    {
                        team = 0
                    };
                    block[0, 6] = new Block()
                    {
                        team = 0
                    };
                    block[0, 9] = new Block()
                    {
                        team = 0
                    };
                    block[0, 7] = new Block()
                    {
                        team = 0
                    };
                    block[0, 8] = new Block()
                    {
                        team = 0
                    };
                    TeamInto[0] += 500_000;
                    break;
                case Keys.R:
                    block[0, 6] = new Block()
                    {
                        team = 0
                    };
                    block[0, 9] = new Block()
                    {
                        team = 0
                    };
                    block[0, 7] = new Block()
                    {
                        team = 0
                    };
                    block[0, 8] = new Block()
                    {
                        team = 0
                    };
                    TeamInto[0] += 400_000;
                    break;
                case Keys.E:
                    block[0, 9] = new Block()
                    {
                        team = 0
                    };
                    block[0, 7] = new Block()
                    {
                        team = 0
                    };
                    block[0, 8] = new Block()
                    {
                        team = 0
                    };
                    TeamInto[0] += 300_000;
                    break;
                case Keys.W:
                    block[0, 7] = new Block()
                    {
                        team = 0
                    };
                    block[0, 8] = new Block()
                    {
                        team = 0
                    };
                    TeamInto[0] += 200_000;
                    break;
                case Keys.Q:
                    block[0, 8] = new Block()
                    {
                        team = 0
                    };
                    TeamInto[0] += 100_000;
                    break;


                case Keys.G:
                    block[15, 10] = new Block()
                    {
                        team = 1
                    };
                    block[15, 6] = new Block()
                    {
                        team = 1
                    };
                    block[15, 9] = new Block()
                    {
                        team = 1
                    };
                    block[15, 7] = new Block()
                    {
                        team = 1
                    };
                    block[15, 8] = new Block()
                    {
                        team = 1
                    };
                    TeamInto[1] += 500_000;
                    break;
                case Keys.F:
                    block[15, 6] = new Block()
                    {
                        team = 1
                    };
                    block[15, 9] = new Block()
                    {
                        team = 1
                    };
                    block[15, 7] = new Block()
                    {
                        team = 1
                    };
                    block[15, 8] = new Block()
                    {
                        team = 1
                    };
                    TeamInto[1] += 400_000;
                    break;
                case Keys.D:
                    block[15, 9] = new Block()
                    {
                        team = 1
                    };
                    block[15, 7] = new Block()
                    {
                        team = 1
                    };
                    block[15, 8] = new Block()
                    {
                        team = 1
                    };
                    TeamInto[1] += 300_000;
                    break;
                case Keys.S:
                    block[15, 7] = new Block()
                    {
                        team = 1
                    };
                    block[15, 8] = new Block()
                    {
                        team = 1
                    };
                    TeamInto[1] += 200_000;
                    break;
                case Keys.A:
                    block[15, 8] = new Block()
                    {
                        team = 1
                    };
                    TeamInto[1] += 100_000;
                    break;
                default:
                    break;
            }
        }
        public Form1()
        {
            InitializeComponent();

            var r = new Random();

            TeamDeath = new int[MaxTeam];
            TeamRun = new int[MaxTeam];
            TeamInto = new int[MaxTeam];
            TeamStartArmy = new int[MaxTeam];
            TeamSurrender = new bool[MaxTeam];

            for (int i = 0; i < MaxTeam; ++i)
            {
                TeamDeath[i] = 0;
                TeamRun[i] = 0;
                TeamInto[i] = 0;
                TeamStartArmy[i] = r.Next(12_000, 100_000);
                TeamSurrender[i] = false;
            }

            currentContext = BufferedGraphicsManager.Current;
            myBuffer = currentContext.Allocate(panel1.CreateGraphics(), panel1.DisplayRectangle);

            center_fmt.LineAlignment = StringAlignment.Center;
            center_fmt.Alignment = StringAlignment.Center;

            //const int left_wdith = 1;
            //const int left_depth = 2;
            //const int right_wdith = 1;
            //const int right_depth = 2;

            const float attrition = 0.05f;
            forConnectness = (x1, y1, x2, y2) =>
            {
                if ((block[x1, y1] == null || block[x2, y2] == null) && geo[x1, y2] != 1 && geo[x2, y2] != 1)
                {
                    if (ai[x1, y1] < ai[x2, y2] - attrition) ai[x1, y1] = ai[x2, y2] - attrition;
                    else if (ai[x2, y2] < ai[x1, y1] - attrition) ai[x2, y2] = ai[x1, y1] - attrition;
                }
            };

            //ForBlock forLeft = (o, x, y) =>
            //{
            //    o.att *= 1 + r.Next(0, 100) / 200.0f;
            //    TeamInto[o.team] += o.size;
            //};
            //ForBlock forRight = (o, x, y) =>
            //{
            //    o.att *= 1 + r.Next(0, 100) / 200.0f;
            //    TeamInto[o.team] += o.size;
            //};

            for (int x = 0; x < 16; ++x)
            {
                for (int y = 0; y < 16; ++y)
                {
                    if (r.Next(0, 17) == 0)
                    {
                        geo[x, y] = 1;
                    }
                }
            }

            int[,] pw = { { 1, 0 }, { 1, -1 }, { 0, -1 }, { -1, -1 }, { -1, 0 }, { -1, 1 }, { 0, 1 }, { 1, 1 } };

            //Build River

            int life = 16 * 16 / 11;
            ForPos func_river = (pos) => { };
            func_river = (pos) =>
            {
                int w = r.Next(0, 8); 
                while (pos.X >= 0 && pos.X < 16 && pos.Y >= 0 && pos.Y < 16 && life > 0)
                {
                    if (geo[pos.X, pos.Y] != 2) --life;
                    geo[pos.X, pos.Y] = 2; 
                    w = r.Next(0, 5) == 0 ? -1 : 0 + r.Next(0, 5) == 0 ? 1 : 0;
                    if (w < 0) w += 8; if (w >= 8) w -= 8;
                    pos.X += pw[w, 0];
                    pos.Y += pw[w, 1];
                    if (r.Next(0, 16) == 0) func_river(new Point(pos.X, pos.Y));
                }
            };

            while (life > 0)
            {
                func_river(new Point(r.Next(0, 16), r.Next(0, 16)));
            }

            
            //for (int i = 0; i < left_wdith; ++i)
            //{
            //    for (int j = 0; j < left_depth; ++j)
            //    {
            //        block[7 - i, j] = new Block()
            //        {
            //            team = 0
            //        };
            //        forLeft(block[7 - i, j], 7 - i, j);
            //        block[8 + i, j] = new Block()
            //        {
            //            team = 0
            //        };
            //        forLeft(block[8 + i, j], 8 + i, j);
            //    }
            //}
            //for (int i = 0; i < right_wdith; ++i)
            //{
            //    for (int j = 0; j < right_depth; ++j)
            //    {
            //        block[7 - i, 15 - j] = new Block()
            //        {
            //            team = 1
            //        };
            //        forRight(block[7 - i, 15 - j], 7 - i, 15 - j);
            //        block[8 + i , 15 - j] = new Block()
            //        {
            //            team = 1
            //        };
            //        forRight(block[8 + i, 15 - j], 8 + i, 15 - j);
            //    }
            //}
        }

        private void Panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            // Timer1_Tick(null, null);
            timer1.Enabled = !timer1.Enabled;
            button1.Text = timer1.Enabled ? "멈추기" : "진행하기";
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            Point pos = new Point();
            float max_value = 0;
            for (int x = 0; x < 16; ++x)
            {
                for (int y = 0; y < 16; ++y)
                {
                    if (block[x, y] != null)
                    {
                        var o = block[x, y];
                        o.prio += o.mor / 100 + 10;
                        if (o.mor <= 0)
                        {
                            o.mor = 0;
                            o.surrender = true;
                        }
                        //if (!o.surrender && TeamSurrender[o.team])
                        //{
                        //    o.surrender = true;
                        //}
                        if (max_value < o.prio)
                        {
                            pos = new Point(x, y);
                            max_value = o.prio;
                        }
                    }
                }
            }

            if (max_value > 0)
            {
                var o = block[pos.X, pos.Y];

                //Marking
                for (int x = 0; x < 16; ++x)
                {
                    for (int y = 0; y < 16; ++y)
                    {
                        if (geo[x, y] == 1)
                        {
                            ai[x, y] = -1;
                        }
                        else if (geo[x, y] == 2)
                        {
                            ai[x, y] = -0.5f;
                        }
                        else
                        {
                            ai[x, y] = 0;
                        }

                        if ((x == 0 || x == 15 || y == 0 || y == 15) && geo[x, y] != 1)
                        {
                            if (o.surrender)
                            {
                                ai[x, y] = 7;
                            }
                        }


                        if (block[x, y] != null)
                        {
                            if (o.surrender)
                            {
                                ai[x, y] += block[x, y].team != o.team ? (-10) : (0.0f);
                            }
                            else
                            {
                                ai[x, y] += block[x, y].team != o.team ? (10) : -0.5f;
                            }
                        }


                    }
                }


                //Blending
                #region Blending
                int[,] W = { { 1, 0 }, { 0, -1 }, { -1, 0 }, { 0, 1 } };
                for (int x = 0; x < 16; ++x)
                {
                    for (int y = 0; y < 16; ++y)
                    {
                        for (int w = 0; w < 4; ++w)
                        {
                            int x2 = x + W[w, 0], y2 = y + W[w, 1];
                            if (x2 >= 0 && x2 < 16 && y2 >= 0 && y2 < 16)
                            {
                                forConnectness(x, y, x2, y2);
                            }
                        }
                    }
                    for (int y = 15; y >= 0; --y)
                    {
                        for (int w = 0; w < 4; ++w)
                        {
                            int x2 = x + W[w, 0], y2 = y + W[w, 1];
                            if (x2 >= 0 && x2 < 16 && y2 >= 0 && y2 < 16)
                            {
                                forConnectness(x, y, x2, y2);
                            }
                        }
                    }
                }
                for (int x = 15; x >= 0; --x)
                {
                    for (int y = 0; y < 16; ++y)
                    {
                        for (int w = 0; w < 4; ++w)
                        {
                            int x2 = x + W[w, 0], y2 = y + W[w, 1];
                            if (x2 >= 0 && x2 < 16 && y2 >= 0 && y2 < 16)
                            {
                                forConnectness(x, y, x2, y2);
                            }
                        }
                    }
                    for (int y = 15; y >= 0; --y)
                    {
                        for (int w = 0; w < 4; ++w)
                        {
                            int x2 = x + W[w, 0], y2 = y + W[w, 1];
                            if (x2 >= 0 && x2 < 16 && y2 >= 0 && y2 < 16)
                            {
                                forConnectness(x, y, x2, y2);
                            }
                        }
                    }
                }
                #endregion

                int dir = -1;
                max_value = ai[pos.X, pos.Y] - block[pos.X, pos.Y].mor / 1000_000;
                for (int w = 0; w < 4; ++w)
                {
                    int x2 = pos.X + W[w, 0], y2 = pos.Y + W[w, 1];
                    if (x2 >= 0 && x2 < 16 && y2 >= 0 && y2 < 16)
                    {
                        if (block[x2, y2] != null)
                        {
                            if (block[x2, y2].team == o.team && o.surrender && !block[x2, y2].surrender)
                            {
                                block[x2, y2].mor -= 100;
                            }
                        }
                        if (ai[x2, y2] > max_value)
                        {
                            if (block[x2, y2] == null)
                            {
                                if (geo[x2, y2] != 1)
                                {
                                    max_value = ai[x2, y2];
                                    dir = w;
                                }
                            }
                            else if (block[x2, y2].team != o.team)
                            {
                                if ((block[x2, y2].size < o.size * 2 || block[x2, y2].mor < o.mor * 2) && o.mor > 0)
                                {
                                    max_value = ai[x2, y2];
                                    dir = w;
                                }
                                else
                                {
                                    o.surrender = true;
                                }
                            }
                        }
                    }
                }
                o.mor -= 1;
                if (o.surrender && (pos.X == 0 || pos.X == 15 || pos.Y == 0 || pos.Y == 15))
                {
                    //System.Diagnostics.Debug.WriteLine($"Team{TeamName[o.team]} : < {o.size} / {o.mor} / {o.org} >");
                    TeamRun[o.team] += o.size;
                    block[pos.X, pos.Y] = null;
                }
                else if (dir != -1)
                {
                    o.prio = 0;
                    var newPos = new Point(pos.X + W[dir, 0], pos.Y + W[dir, 1]);
                    if (block[newPos.X, newPos.Y] == null)
                    {
                        block[newPos.X, newPos.Y] = block[pos.X, pos.Y].Copy();
                        if (geo[newPos.X, newPos.Y] == 2)
                        {
                            block[newPos.X, newPos.Y].prio = -2500;
                        }
                        block[pos.X, pos.Y] = null;

                    }
                    else
                    {
                        var p = block[newPos.X, newPos.Y];
                        if (p.mor > 0 && !p.surrender)
                        {
                            //int damage = (int)Math.Round(1 * p.size * p.att);
                            //o.size -= damage;
                            //TeamDeath[o.team] += damage;
                        }
                        else if (o.mor <= 1000 - 1)
                        {
                            o.mor += 1;
                        }

                        if (o.mor > 0 && !o.surrender)
                        {
                            int damage = (int)Math.Round(1 * o.size * o.att);
                            p.size -= damage;
                            TeamDeath[p.team] += damage;
                        }
                        else if (p.mor <= 1000 - 1)
                        {
                            p.mor += 1;
                        }

                        const float percent = 0.04f;
                        var minMorale = (int)Math.Ceiling(percent * Math.Sqrt(Math.Min(o.mor, p.mor)));

                        if (o.mor > p.mor)
                        {
                            if (p.size > 0) o.mor -= minMorale * 10;
                            if (o.size > 0) p.mor -= minMorale * minMorale;
                        }
                        else
                        {
                            if (p.size > 0) o.mor -= minMorale * minMorale;
                            if (o.size > 0) p.mor -= minMorale * 10;
                        }
                        if (o.size <= 0)
                        {
                            block[pos.X, pos.Y] = null;
                        }
                        if (p.size <= 0)
                        {
                            block[newPos.X, newPos.Y] = null;
                        }
                    }
                }
                else
                {
                    o.org -= 10;
                    o.prio = 0;
                    if (o.surrender)
                    {
                        bool is_locked = true;
                        for (int w = 0; w < 4; ++w)
                        {
                            int x2 = pos.X + W[w, 0], y2 = pos.Y + W[w, 1];
                            if (x2 >= 0 && x2 < 16 && y2 >= 0 && y2 < 16)
                            {
                                if (block[x2, y2] == null && geo[x2, y2] == 0)
                                {
                                    is_locked = false;
                                    break;
                                }
                                else if (block[x2, y2] != null)
                                {
                                    if (block[x2, y2].team == o.team && block[x2, y2].surrender)
                                    {
                                        is_locked = false;
                                        break;
                                    }
                                }
                                
                            }
                        }
                        if (is_locked) { o.surrender = false; o.mor = 1000; }
                    }
                }
            }

            for (int i = 0; i < MaxTeam; ++i)
            {
                if (TeamInto[i] - TeamDeath[i] - TeamRun[i] + TeamStartArmy[i] < MaxArmyInField && !TeamSurrender[i])
                {
                    if (block[TeamLoc[i].X, TeamLoc[i].Y] == null)
                    {
                        block[TeamLoc[i].X, TeamLoc[i].Y] = new Block()
                        {
                            team = i,
                            size = TeamStartArmy[i]
                        };
                        TeamInto[i] += TeamStartArmy[i];
                    }
                }
            }

            richTextBox1.Text = "";

            for (int i = 0; i < MaxTeam; ++i)
            {
                if (TeamSurrender[i])
                {
                    richTextBox1.Text += $"{i}군 항복, ";
                }
                else
                {
                    richTextBox1.Text += $"{i}군 {(int)((MaxArmyInBattle - (0.0 + TeamDeath[i] / 2 + TeamRun[i])) / MaxArmyInBattle * 100)}%, ";
                }
            }

            for (int i = 0; i < MaxTeam; ++i) if (TeamDeath[i] / 2 + TeamRun[i] >= MaxArmyInBattle) { TeamSurrender[i] = true; }

            //Drawing
            var g = myBuffer.Graphics;
            g.Clear(panel1.BackColor);
            for (int x = 0; x < 16; ++x)
            {
                for (int y = 0; y < 16; ++y)
                {
                    g.DrawRectangle(Pens.Gray, new Rectangle(x * 40, y * 40, 40, 40));
                    if (geo[x, y] == 1) g.FillPolygon(Brushes.Black, new Point[] { new Point(x * 40 + 20, y * 40 + 7), new Point(x * 40 + 7, y * 40 + 30), new Point(x * 40 + 33, y * 40 + 30) });
                    if (geo[x, y] == 2)
                    {
                        g.FillRectangle(Brushes.Black, new Rectangle(x * 40 + 5, y * 40 + 5, 30, 30));
                    }

                    if (block[x, y] != null)
                    {
                        var o = block[x, y];
                        brs.Color = TeamColor[o.team];
                        g.FillRectangle(brs, new Rectangle(x * 40 + 1, y * 40 + 1, 38, 38));
                        g.FillRectangle(Brushes.Red, new Rectangle(x * 40 + 5, y * 40 + 28, 30, 3));
                        g.FillRectangle(Brushes.GreenYellow, new RectangleF(x * 40 + 5, y * 40 + 28, 30.0f * o.mor / 1000, 3));
                        g.FillRectangle(Brushes.Black, new Rectangle(x * 40 + 5, y * 40 + 33, 30, 3));
                        g.FillRectangle(Brushes.LightGray, new RectangleF(x * 40 + 5, y * 40 + 33, 30.0f * o.org / 1000, 3));
                        g.DrawString($"{o.size / 1_000}", font0, o.prio < 0 ? Brushes.Red : (o.surrender ? Brushes.SkyBlue : Brushes.White), new RectangleF(x * 40, y * 40, 40, 35), center_fmt);
                    }
                    //else
                    {
                        //g.DrawString($"{ai[x, y]}", font0, Brushes.Black, new RectangleF(x * 40, y * 40, 40, 35), center_fmt);
                    }
                    //g.FillEllipse(Brushes.Red, new RectangleF(x * 40 + 20 - ai[x, y], y * 40 + 20 - ai[x, y], ai[x, y] * 2, ai[x, y] * 2));
                }
            }

            pos = new Point();
            max_value = 0;
            for (int x = 0; x < 16; ++x)
            {
                for (int y = 0; y < 16; ++y)
                {
                    if (block[x, y] != null)
                    {
                        var o = block[x, y];
                        o.prio += o.mor / 100 + 10;
                        if (o.mor <= 0)
                        {
                            o.mor = 0;
                            o.surrender = true;
                        }
                        else if (o.mor > 1000) o.mor = 1000;
                        if (max_value < o.prio)
                        {
                            pos = new Point(x, y);
                            max_value = o.prio;
                        }
                    }
                }
            }

            if (max_value > 0)
            {
                var o = block[pos.X, pos.Y];
                g.DrawRectangle(Pens.Fuchsia, new Rectangle(pos.X * 40, pos.Y * 40, 40, 40));
            }
            myBuffer.Render();


        }
        

    }
}
class Block
{
    public bool enable = false;
    public bool surrender = false;
    public int team = -1;
    public int size = 100_000;
    public int mor = 1000;
    public int org = 1000;
    public float att = 0.022f;
    public float prio = 0;
    public Block Copy()
    {
        return new Block()
        {
            enable = enable,
            team = team,
            size = size,
            mor = mor,
            org = org,
            prio = prio,
            surrender = surrender,
            att = att
        };
    }
}

