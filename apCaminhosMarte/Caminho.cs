﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apCaminhosMarte
{
    class Caminho
    {
        private int idCidadeOrigem, idCidadeDestino;
        private int distancia, tempo, custo;

        public Caminho(int idCidadeOrigem, int idCidadeDestino, int distancia, int tempo, int custo)
        {
            this.idCidadeOrigem = idCidadeOrigem;
            this.idCidadeDestino = idCidadeDestino;
            this.distancia = distancia;
            this.tempo = tempo;
            this.custo = custo;
        }

        public int IdCidadeOrigem { get => idCidadeOrigem; set => idCidadeOrigem = value; }
        public int IdCidadeDestino { get => idCidadeDestino; set => idCidadeDestino = value; }
        public int Distancia { get => distancia; set => distancia = value; }
        public int Tempo { get => tempo; set => tempo = value; }
        public int Custo { get => custo; set => custo = value; }
    }
}