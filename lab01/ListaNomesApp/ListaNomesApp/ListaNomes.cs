using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace ListaNomesApp
{
    interface IListaNomes
    {
        // Adicionar novo nome à lista de nomes
        void adicionar(String nome);

        // Retornar a listagem de todos os nomes presentes na lista de nomes, 
        // sob a forma de uma cadeia de caracteres
        String listar();

        // Limpar o conteúdo da lista de nomes
        void limpar();
    }

    class ListaNomes : IListaNomes
    {
        private ArrayList nomes = new ArrayList();

        public void adicionar(String nome)
        {
            nomes.Add(nome);
        }

        public String listar()
        {
            String final = "";
            if (nomes.Count != 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (String nome in nomes) sb.Append(nome + ',');
                final = sb.ToString();
                final = final.Remove(final.Length - 1);
            }
            return final; 
        }

        public void limpar()
        {
            nomes.Clear();
        }
    }
}
