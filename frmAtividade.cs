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
            this.Invoke(new MethodInvoker(delegate()
            {
                txtArquivo.Enabled = false;
                btnRun.Enabled = false;
            }));

            try
            {
                CodigoAtividade(filePath);

                this.Invoke(new MethodInvoker(delegate()
                {
                    MessageBox.Show(this, "Finalizado!");
                }));
            }
            catch (Exception ex)
            {
                this.Invoke(new MethodInvoker(delegate()
                {
                    MessageBox.Show(this, ex.Message);
                }));
            }
            finally
            {
                this.Invoke(new MethodInvoker(delegate()
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
                // ler o arquivo texto de entrada
                using (StreamReader sr = File.OpenText(filePath))
                {
                    // ler e salva a dimensão da matriz do labirinto
                    string[] vector = sr.ReadLine().Split(' ');
                    int lines = int.Parse(vector[0]);
                    int columns = int.Parse(vector[1]);

                    // ler e salva o labirinto em si
                    Dictionary<string, string> maze = new Dictionary<string, string>();
                    for (int l = 1; l <= lines; l++)
                    {
                        string[] vector2 = sr.ReadLine().Split(' ');
                        for (int c = 1; c <= columns; c++)
                        {
                            maze[l + "," + c] = vector2[c-1];
                        }
                    }

                    // salva as posições visitadas
                    List<string> visitedPositions = new List<string>();
                    List<string> auxRetorno = new List<string>();

                    // salva a posição atual
                    int currentPositionLine = 0;
                    int currentPositionColumn = 0;

                    // verifica se no labirinto existe o ponto de origem (X)
                    if (maze.ContainsValue("X"))
                    {
                        // identifica e salva a posição de origem
                        for (int l = 1; l <= lines; l++)
                        {
                            for (int c = 1; c <= columns; c++)
                            {
                                if (maze[l + "," + c].Equals("X"))
                                {
                                    currentPositionLine = l;
                                    currentPositionColumn = c;
                                    visitedPositions.Add("O [" + l + ", " + c + "]");
                                }
                            }
                        }
                        string nextPosition = "", previousPosition = "O";
                        int countPossibleMovements;
                        do
                        {
                            // testa se pode deslocar para baixo
                            countPossibleMovements = 0;
                            if ((currentPositionLine + 1) > 0)
                            {
                                if (maze[(currentPositionLine + 1) + "," + currentPositionColumn].Equals("0") && !previousPosition.Equals("C"))
                                {
                                    nextPosition = "B";
                                    countPossibleMovements++;
                                }
                            }
                            // testa se pode deslocar para direita
                            if ((currentPositionColumn + 1) > 0)
                            {
                                if (maze[currentPositionLine + "," + (currentPositionColumn + 1)].Equals("0") && !previousPosition.Equals("E"))
                                {
                                    nextPosition = "D";
                                    countPossibleMovements++;
                                }
                            }
                            // testa se pode deslocar para esquerda
                            if ((currentPositionColumn - 1) > 0)
                            {
                                if (maze[currentPositionLine + "," + (currentPositionColumn - 1)].Equals("0") && !previousPosition.Equals("D"))
                                {
                                    nextPosition = "E";
                                    countPossibleMovements++;
                                }
                            }
                            // testa se pode deslocar para cima
                            if ((currentPositionLine - 1) > 0)
                            {
                                if (maze[(currentPositionLine - 1) + "," + currentPositionColumn].Equals("0") && !previousPosition.Equals("B"))
                                {
                                    nextPosition = "C";
                                    countPossibleMovements++;
                                }
                            }
                            if (countPossibleMovements == 0)
                            {
                                nextPosition = auxRetorno.Last().Substring(0, 1);
                                string[] vector3 = auxRetorno.Last().Substring(3,4).Split(',');
                                currentPositionLine = int.Parse(vector3[0]);
                                currentPositionColumn = int.Parse(vector3[1].Trim());
                                auxRetorno.Remove(auxRetorno.Last());
                                switch (nextPosition)
                                {
                                    case "C":
                                        nextPosition = "B";
                                        break;
                                    case "E":
                                        nextPosition = "D";
                                        break;
                                    case "D":
                                        nextPosition = "E";
                                        break;
                                    case "B":
                                        nextPosition = "C";
                                        break;
                                }
                            }
                            // faz o deslocamento
                            switch (nextPosition)
                            {
                                case "C":
                                    currentPositionLine--;
                                    break;
                                case "E":
                                    currentPositionColumn--;
                                    break;
                                case "D":
                                    currentPositionColumn++;
                                    break;
                                case "B":
                                    currentPositionLine++;
                                    break;
                            }
                            visitedPositions.Add(nextPosition + " [" + currentPositionLine + ", " + currentPositionColumn + "]");
                            if (countPossibleMovements != 0)
                            {
                                auxRetorno.Add(nextPosition + " [" + currentPositionLine + ", " + currentPositionColumn + "]");
                            }
                            maze[currentPositionLine + "," + currentPositionColumn] = "1";
                            previousPosition = nextPosition;
                        }
                        while (currentPositionColumn != columns && currentPositionColumn != 1 && currentPositionLine != lines && currentPositionLine != 1);

                        foreach(var item in visitedPositions)
                        {
                            Console.WriteLine(item);
                        }
                        
                    }
                    else
                    {
                        Console.WriteLine("O labirinto não possui ponto de origem (X).");
                    }
                }
            }
            catch(IOException e)
            {
                Console.WriteLine("An error occurred.");
                Console.WriteLine(e.Message);
            }
        }
    }
}
