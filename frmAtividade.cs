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
                    
                    // mostra o labirinto lido
                    foreach(var item in maze)
                    {
                        Console.WriteLine("key: " + item.Key + " value: " + item.Value);
                    }

                    // verifica se no labirinto existe o ponto de origem (X)
                    string o = null;
                    if (maze.ContainsValue("X"))
                    {
                        // identifica e salva a posição de origem
                        for (int l = 1; l <= lines; l++)
                        {
                            for (int c = 1; c <= columns; c++)
                            {
                                if (maze[l + "," + c].Equals("X"))
                                {
                                    o = l + "," + c;
                                }
                            }
                        }
                        Console.WriteLine("Ponto de origem encontrado na posição: " + o);
                    } else
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
