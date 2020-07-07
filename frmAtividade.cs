using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Atividade
{
    public partial class frmAtividade : Form
    {
        public frmAtividade()
        {
            InitializeComponent();
        }



        private void btnRun_Click(object sender, EventArgs e)
        {
            if (txtArquivo.Text.Trim().Equals(""))
            {
                MessageBox.Show(this, "Caminho do arquivo deve ser informado");
                txtArquivo.Focus();
                return;
            }

            if (!File.Exists(txtArquivo.Text.Trim()))
            {
                MessageBox.Show(this, "Arquivo inexistente!");
                txtArquivo.Focus();
                return;
            }

            Thread thread = new Thread(() => ExecutaAtividade(txtArquivo.Text.Trim()));
            thread.Name = "Atividade - Run";
            thread.Start();
        }


        private void ExecutaAtividade(string filePath)
        {
            this.Invoke(new MethodInvoker(delegate ()
            {
                txtArquivo.Enabled = false;
                btnRun.Enabled = false;
            }));

            try
            {
                CodigoAtividade(filePath);

                this.Invoke(new MethodInvoker(delegate ()
                {
                    MessageBox.Show(this, "Finalizado!");
                }));
            }
            catch (Exception ex)
            {
                this.Invoke(new MethodInvoker(delegate ()
                {
                    MessageBox.Show(this, ex.Message);
                }));
            }
            finally
            {
                this.Invoke(new MethodInvoker(delegate ()
                {
                    txtArquivo.Enabled = true;
                    btnRun.Enabled = true;
                }));
            }
        }

        private void CodigoAtividade(string filePath)
        {
            // CÓDIGO AQUI!!
            try
            {
                int lines, columns;
                // ler o arquivo texto de entrada
                StreamReader sr = File.OpenText(filePath);

                // ler e salva a dimensão da matriz do labirinto
                string[] vector = sr.ReadLine().Split(' ');
                lines = int.Parse(vector[0]);
                columns = int.Parse(vector[1]);

                Maze maze1 = new Maze(lines, columns);

                // ler e salva o labirinto em si
                for (int l = 1; l <= lines; l++)
                {
                    string[] vector2 = sr.ReadLine().Split(' ');
                    for (int c = 1; c <= columns; c++)
                    {
                        Position positionAux = new Position(l, c);
                        maze1.AddComponent(positionAux, new MazeComponent(positionAux, maze1, vector2[c - 1]));
                    }
                }

                MazeChallenge mazeChallenge = new MazeChallenge(maze1);

                // verifica se no labirinto existe algum ponto de origem (X)
                mazeChallenge.IdentifyOriginPosition();

                // tenta encontrar a saída
                while (!mazeChallenge.ExitFound)
                {
                    mazeChallenge.ExecuteMovement();
                }

                // cria um arquivo para gravar a saída
                string outputFilePath = Path.GetDirectoryName(filePath) + @"\saida-" + Path.GetFileName(filePath);
                using (StreamWriter sw = File.CreateText(outputFilePath))
                {
                    foreach (var item in mazeChallenge.VisitedPositions)
                    {
                        sw.WriteLine(item);
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("An error occurred.");
                Console.WriteLine(e.Message);
            }
        }

        class Position
        {
            public int Line { get; set; }
            public int Column { get; set; }

            public Position(int line, int column)
            {
                Line = line;
                Column = column;
            }

            public override string ToString()
            {
                return Line + "," + Column;
            }

            public void SetValue(int line, int column)
            {
                Line = line;
                Column = column;
            }
        }

        class MazeComponent
        {
            public Position Position { get; set; }
            public Maze Maze { get; protected set; }
            public string ComponentValue { get; protected set; }
            public bool Visited { get; protected set; }

            public MazeComponent(Position position, Maze maze, string componentValue)
            {
                Position = position;
                Maze = maze;
                ComponentValue = componentValue;
                Visited = false;
            }

            public void SetVisited()
            {
                Visited = true;
            }

            private bool CanMove(Position pos)
            {
                MazeComponent aux = Maze.GetComponent(pos);
                return aux.ComponentValue.Equals("0") && !aux.Visited;
            }

            public Dictionary<string, bool> PossibleMovements()
            {
                Dictionary<string, bool> aux = new Dictionary<string, bool>();
                Position pos = new Position(0, 0);

                // testa se pode deslocar para baixo
                pos.SetValue((Position.Line + 1), Position.Column);
                aux.Add("B", (pos.Line) > 0 && CanMove(pos));
                // testa se pode deslocar para direita
                pos.SetValue(Position.Line, (Position.Column + 1));
                aux.Add("D", (pos.Column) > 0 && CanMove(pos));
                // testa se pode deslocar para esquerda
                pos.SetValue(Position.Line, (Position.Column - 1));
                aux.Add("E", (pos.Column) > 0 && CanMove(pos));
                // testa se pode deslocar para cima
                pos.SetValue((Position.Line - 1), Position.Column);
                aux.Add("C", (pos.Line > 0 && CanMove(pos)));
                
                return aux;
            }
        }

        class MazeException : Exception
        {
            public MazeException(string msg) : base (msg)
            {

            }
        }

        class Maze
        {
            public int Lines { get; private set; }
            public int Columns { get; private set; }
            private MazeComponent[,] MazeComponents;

            public Maze(int lines, int columns)
            {
                Lines = lines;
                Columns = columns;
                MazeComponents = new MazeComponent[lines + 1, columns + 1];
            }

            public void AddComponent(Position position, MazeComponent mazeComponent)
            {
                MazeComponents[position.Line,position.Column] = mazeComponent;
            }

            public MazeComponent GetComponent(Position position)
            {
                return MazeComponents[position.Line, position.Column];
            }
        }

        class MazeChallenge
        {
            public Position CurrentPosition { get; private set; }
            public Maze Maze { get; set; }
            public bool ExitFound { get; private set; }
            public List<string> VisitedPositions { get; private set; }
            private List<string> AuxReturn { get; set; }

            public MazeChallenge(Maze maze)
            {
                Maze = maze;
                CurrentPosition = null;
                ExitFound = false;
                VisitedPositions = new List<string>();
                AuxReturn = new List<string>();
            }

            public void IdentifyOriginPosition()
            {
                // percorre todo o labirinto verificando se existe algum ponto de origem (X)
                for (int l = 1; l <= Maze.Lines; l++)
                {
                    for (int c = 1; c <= Maze.Columns; c++)
                    {
                        if (Maze.GetComponent(new Position(l, c)).ComponentValue.Equals("X"))
                        {
                            CurrentPosition = new Position(l, c);
                            VisitedPositions.Add("O [" + CurrentPosition.Line + ", " + CurrentPosition.Column + "]");
                        }
                    }
                }
                if (CurrentPosition == null)
                {
                    throw new MazeException("O labirinto não possui nenhum ponto de origem (X).");
                }
            }

            public void ExecuteMovement()
            {
                // verifica as possibilidades de se movimentar a partir da posição atual
                Dictionary<string, bool> aux = Maze.GetComponent(CurrentPosition).PossibleMovements();
                int countPossibleMovements = 0;
                string movement = "";
                foreach(var item in aux)
                {
                    if (item.Value)
                    {
                        countPossibleMovements++;
                        movement = item.Key;
                    }
                }
                // movimenta seguindo a ordem de prioridade
                if (countPossibleMovements > 0)
                {
                    UpdateCurrentPosition(movement, false);
                } else
                {
                    // retornar usando o mesmo caminho utilizado até este ponto “sem - saída” até o último ponto onde teve mais de uma posição possível de movimento
                    movement = AuxReturn.Last().Substring(0, 1);
                    string[] vector3 = AuxReturn.Last().Substring(3, 4).Split(',');
                    CurrentPosition.Line = int.Parse(vector3[0]);
                    CurrentPosition.Column = int.Parse(vector3[1].Trim());
                    AuxReturn.Remove(AuxReturn.Last());
                    UpdateCurrentPosition(movement, true);
                }
                // verifica se encontrou a saída do labirinto
                if(CurrentPosition.Column == Maze.Columns || CurrentPosition.Column == 1 || CurrentPosition.Line == Maze.Lines || CurrentPosition.Line == 1)
                {
                    ExitFound = true;
                }
            }

            private void UpdateCurrentPosition(string movement, bool returning)
            {
                // movimenta-se no labirinto seguindo as regras
                switch (movement)
                {
                    case "C":
                        if (returning)
                        {
                            movement = "B";
                            CurrentPosition.Line++;
                        } else
                        {
                            CurrentPosition.Line--;
                        }
                        break;
                    case "E":
                        if (returning)
                        {
                            movement = "D";
                            CurrentPosition.Column++;
                        } else
                        {
                            CurrentPosition.Column--;
                        }
                        break;
                    case "D":
                        if (returning)
                        {
                            movement = "E";
                            CurrentPosition.Column--;
                        }
                        else
                        {
                            CurrentPosition.Column++;
                        }
                        break;
                    case "B":
                        if (returning)
                        {
                            movement = "C";
                            CurrentPosition.Line--;
                        } else
                        {
                            CurrentPosition.Line++;
                        }
                        break;
                }
                // seta e guarda a posição que foi visitada
                VisitedPositions.Add(movement + " [" + CurrentPosition.Line + ", " + CurrentPosition.Column + "]");
                Maze.GetComponent(CurrentPosition).SetVisited();
                // caso seja uma posição que ainda não foi visitada, registra a ocorrência em uma lista que auxiliará a retornar pelo caminho (quando for o caso)
                if (!returning)
                {
                    AuxReturn.Add(movement + " [" + CurrentPosition.Line + ", " + CurrentPosition.Column + "]");
                }
            }
        }
    }
}
