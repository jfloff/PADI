using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ListaNomesApp
{
    public partial class Form1 : Form
    {
        ListaNomes nomes = new ListaNomes(); 

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void adicionarButton_Click(object sender, EventArgs e)
        {
            String nome = nomeTextBox.Text;
            if (nome == "")
            {
                infoLabel.Text = "Nome não pode ser vazio";
            }
            else
            {
                nomes.adicionar(nome);
                infoLabel.Text = "Nome adicionado:" + nome;
                nomeTextBox.Clear();
            }
        }

        private void listarButton_Click(object sender, EventArgs e)
        {
            infoLabel.Text = nomes.listar();
        }

        private void limparButton_Click(object sender, EventArgs e)
        {
            nomes.limpar();
            infoLabel.Text = "Lista de nomes limpa";
        }
    }
}
