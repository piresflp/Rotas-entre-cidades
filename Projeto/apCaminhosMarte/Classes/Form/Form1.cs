﻿// 19169 - Felipe Pires Araujo
// 19196 - Rafael Romanhole Borrozino

using apCaminhosMarte.Classes.GPS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace apCaminhosMarte
{
    public partial class frmBuscaCaminhos : Form
    {
        GPS gps;
        public frmBuscaCaminhos()
        {
            InitializeComponent();
        }

        /**
         * Tratamento do clique do botão de buscar caminhos.
         * Primeiramente, são realizadas verificações para garantir que as Listboxs estão devidamente selecionadas.
         * Se sim, é criado um vetor com o nome de cada cidade presente nos arquivos textos (de forma ordenada).
         * Em seguida, são definidas quais são as cidades de origem e destino selecionadas pelo usuário, de acordo com o índice das Listboxs.
         * Feito isso, o programa percorre a lista de cidades para encontrar o Id de tais cidades e chama o método de buscar caminhoe e melhor caminho.
         * Por fim, os resultados são armazenados em listas que serão passadas como parâmetro dos métodos de exibição.
         */
        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                int idCidadeOrigem = -1, idCidadeDestino = -1;
                LerCidadesSelecionadas(ref idCidadeOrigem, ref idCidadeDestino);
                LerRadioButtonsSelecionados();

                gps.CaminhosEncontrados = new List<Caminho>();
                MetodoDeBusca metodoEscolhido = gps.Metodo;
                switch (metodoEscolhido)
                {
                    case MetodoDeBusca.Pilhas:
                        gps.BuscarCaminhosPilhas(idCidadeOrigem, idCidadeDestino);
                        break;

                    case MetodoDeBusca.Recursao:
                        gps.BuscarCaminhosRecursivo(idCidadeOrigem, idCidadeDestino);
                        break;

                    case MetodoDeBusca.Dijkstra:
                        gps.BuscarCaminhosDijkstra(idCidadeOrigem, idCidadeDestino);
                        break;
                }

                Caminho melhorCaminho = BuscarMelhorCaminho(Extensoes.Clone(gps.CaminhosEncontrados));
                ExibirCaminhos(Extensoes.Clone(gps.CaminhosEncontrados), melhorCaminho);                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }        
        }
        
        private void LerCidadesSelecionadas(ref int idCidadeOrigem, ref int idCidadeDestino)
        {
            if (lsbOrigem.SelectedIndex == -1)
                throw new Exception("Selecione a cidade de origem!");

            if (lsbDestino.SelectedIndex == -1)
                throw new Exception("Selecione a cidade de destino!");

            String[] cidadesMarte = new String[23] { "Acheron", "Arena", "Arrakeen", "Bakhuysen",
                                                "Bradbury", "Burroughs", "Cairo", "Dumont", "Echus Overlook",
                                                "Esperança", "Gondor", "Lakefront", "Lowell", "Moria", "Nicosia",
                                                "Odessa", "Perseverança", "Rowan", "Senzeni Na", "Sheffield", "Temperança",
                                                "Tharsis", "Underhill"};

            String cidadeOrigem = cidadesMarte[lsbOrigem.SelectedIndex];
            String cidadeDestino = cidadesMarte[lsbDestino.SelectedIndex];

            foreach (Cidade cidade in gps.ListaCidades) // percorre o vetor de cidades para encontrar o Id das cidades selecionadas
            {
                if (cidade.Nome.Equals(cidadeOrigem))
                    idCidadeOrigem = cidade.Id;

                else if (cidade.Nome.Equals(cidadeDestino))
                    idCidadeDestino = cidade.Id;

                if (idCidadeOrigem != -1 && idCidadeDestino != -1) // se o Id de ambas cidades já foram encontrados
                    break; // sai do foreach
            }
            if (idCidadeOrigem == -1 || idCidadeDestino == -1)
                throw new Exception("Erro ao descobrir Id de alguma das cidades selecionadas.");
        }

        private void LerRadioButtonsSelecionados()
        {
            String criterioMelhorCaminho = null, metodoDeBusca = null;

            foreach (RadioButton rb in groupBox1.Controls.OfType<RadioButton>())
                if (rb.Checked)
                    criterioMelhorCaminho = rb.Name;

            foreach (RadioButton rb in groupBox2.Controls.OfType<RadioButton>())
                if (rb.Checked)
                    metodoDeBusca = rb.Name;

            if (criterioMelhorCaminho == null) // se nenhum botão foi selecionado pelo usuário
                throw new Exception("Selecione o critério desejado!");

            if (metodoDeBusca == null) // se nenhum botão foi selecionado pelo usuário
                throw new Exception("Selecione o método de busca desejado!");

            switch (criterioMelhorCaminho)
            {
                case "rbTempo":
                    gps.Criterio = CriterioMelhorCaminho.Tempo;
                    break;

                case "rbDistancia":
                    gps.Criterio = CriterioMelhorCaminho.Distancia;
                    break;

                case "rbCusto":
                    gps.Criterio = CriterioMelhorCaminho.Custo;
                    break;
            }

            switch (metodoDeBusca)
            {
                case "rbPilhas":
                    gps.Metodo = MetodoDeBusca.Pilhas;
                    break;

                case "rbRecursao":
                    gps.Metodo = MetodoDeBusca.Recursao;
                    break;

                case "rbDijkstra":
                    gps.Metodo = MetodoDeBusca.Dijkstra;
                    break;
            }
        }

        /**
         * Método com a função de exibir os resultados para o usuário.
         * Percorre a lista de caminhos encontrados e de melhor caminho, exibindo cada passo em uma coluna do dataGridView.
         * Caso nenhuma coluna seja exibida, nenhum caminho foi encontrado e o usuário é alertado.
         */
        private void ExibirCaminhos(List<Caminho> listaCaminhos, Caminho melhorCaminho)
        {                      
            dgvCaminhos.ColumnCount = 0;
            dgvCaminhos.RowCount = listaCaminhos.Count;
            int caminhosExibidos = 0;

            int qtdMovimentos;
            foreach (Caminho caminho in listaCaminhos)
            {
                qtdMovimentos = caminho.Tamanho;
                if (qtdMovimentos > dgvCaminhos.ColumnCount)
                {
                    dgvCaminhos.ColumnCount = qtdMovimentos; // quantidade de colunas do dataGridView se iguala ao tamanho de passos do maior caminho
                    for (int i = 0; i < qtdMovimentos; i++)
                        dgvCaminhos.Columns[i].HeaderText = "Cidade";
                }

                // exibe cada movimento do caminho em questão
                for (int i = qtdMovimentos - 1; !caminho.Movimentos.EstaVazia; i--) 
                {
                    Movimento mov = (Movimento)caminho.RemoverMovimento();
                    dgvCaminhos.Rows[caminhosExibidos].Cells[i].Value = mov.ToString();
                }
                caminhosExibidos++;
            }
            
            if (melhorCaminho.Tamanho > 0) // se a lista de melhorCaminho não estiver vazia
            {
                qtdMovimentos = melhorCaminho.Tamanho;
                dgvMelhorCaminho.ColumnCount = qtdMovimentos;
                dgvMelhorCaminho.RowCount = 1;
                for (int i = qtdMovimentos - 1; !melhorCaminho.Movimentos.EstaVazia; i--) // exibe cada movimento do melhor caminho
                {
                    Movimento mov = melhorCaminho.RemoverMovimento();
                    dgvMelhorCaminho.Rows[0].Cells[i].Value = mov.ToString();
                }
                lbTotalMenorPercurso.Text = melhorCaminho.PesoTotal.ToString();
            }
            else // se nenhum caminho foi encontrado, o usuário é alertado e os dataGridViews limpados
            {
                LimparPictureBox();
                dgvMelhorCaminho.RowCount = 0;
                lbTotalMenorPercurso.Text = "";
                MessageBox.Show("Não foi encontrado nenhum caminho.");
            }
        }

        /**
         * Chama o método que busca o melhor caminho, da classe GPS.
         */
        private Caminho BuscarMelhorCaminho(List<Caminho> caminhos)
        {
            return gps.BuscarMelhorCaminho(caminhos);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                gps = new GPS();
            }
            catch(Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void pbMapa_Paint(object sender, PaintEventArgs e)
        {
            double xProporcional = 4096.00 / pbMapa.Width;
            double yProporcional = 2048.00 / pbMapa.Height;
            foreach (Cidade c in gps.ListaCidades)
            {
                e.Graphics.FillEllipse(Brushes.Black, c.CoordenadaX / Convert.ToSingle(xProporcional), c.CoordenadaY / Convert.ToSingle(yProporcional), 7f, 7f);
                e.Graphics.DrawString(c.Nome, new Font("Comic Sans", 8, FontStyle.Bold), Brushes.Black, c.CoordenadaX / Convert.ToSingle(xProporcional) - 15, c.CoordenadaY / Convert.ToSingle(yProporcional) - 15);
            }
        }

        private void tpArvore_Paint(object sender, PaintEventArgs e)
        {
            gps.Arvore.DesenharArvore(true, gps.Arvore.Raiz, (int)tpArvore.Width / 2, 0, Math.PI / 2, Math.PI / 2.5, 300, e.Graphics);
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            LimparPictureBox();
            Application.DoEvents();
            DesenharCaminho(this.gps.CaminhosEncontrados[dgvCaminhos.CurrentCell.RowIndex]);
        }

        /**
         * Método responsável por desenhar as retas ligando cada movimento do caminho passado como parâmetro.
         */
        private void DesenharCaminho(Caminho caminhoParaDesenhar)
        {
            if (caminhoParaDesenhar != null)
            {
                Graphics g = pbMapa.CreateGraphics();
                Pen pen = new Pen(Color.Blue, 3);

                int xProporcional = 4096 / pbMapa.Width +1;
                int yProporcional = 2048 / pbMapa.Height +1;
                Caminho caminhoClone = (Caminho) caminhoParaDesenhar.Clone();

                int coordenadaXOrigem = 0, coordenadaXDestino = 0, coordenadaYOrigem = 0, coordenadaYDestino = 0;
                while (!caminhoClone.Movimentos.EstaVazia)
                {
                    foreach (Cidade c in gps.ListaCidades)
                    {
                        if (caminhoClone.Movimentos.Primeiro.Info.IdCidadeOrigem == c.Id)
                        {
                            coordenadaXOrigem = c.CoordenadaX;
                            coordenadaYOrigem = c.CoordenadaY;
                        }
                        if (caminhoClone.Movimentos.Primeiro.Info.IdCidadeDestino == c.Id)
                        {
                            coordenadaXDestino = c.CoordenadaX;
                            coordenadaYDestino = c.CoordenadaY;
                        }
                    }
                    g.DrawLine(pen, new Point(coordenadaXOrigem / xProporcional, coordenadaYOrigem / yProporcional), new Point(coordenadaXDestino / xProporcional, coordenadaYDestino / yProporcional));
                    caminhoClone.RemoverMovimento();
                }
            }           
        }

        /**
         * Limpa a pictureBox do mapa de marte para exibição de novos caminhos, evitando sobreposição.
         */
        private void LimparPictureBox()
        {
            Graphics g = pbMapa.CreateGraphics();
            g.Clear(Color.White);
            pbMapa.Image = Image.FromFile("mars_political_map_by_axiaterraartunion_d4vfxdf-pre.jpg");
        }
    }
}
