using System;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Transactions;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;

namespace SalaryTracker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeComponents();
            DataBase();
            guna2Button2.Visible = false; // Editar/Remover
            guna2Button3.Visible = false; // Histórico
            guna2Button4.Visible = false; // Relatório Mensal
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.ControlBox = true;
        }

        private void InitializeComponents()
        {

        }

        private void DataBase()
        {
            string connectionString = "Data Source=dados_trabalho.db;Version=3;";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string criarTabela = @"CREATE TABLE IF NOT EXISTS Registos (
                                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                        Data TEXT,
                                        HorasTrabalhadas INTEGER,
                                        ValorHora DECIMAL,
                                        EntregasBG INTEGER,
                                        EntregasBS INTEGER,
                                        TotalDia DECIMAL
                                    );";

                using (var command = new SQLiteCommand(criarTabela, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //Recolher os valores das textboxs
                int horasTrabalhadas = int.Parse(txtHoras.Text);
                decimal valorHora = decimal.Parse(txtValor_h.Text);

                //Calcular o pagamento pelas horas trabalhadas
                decimal valorTotal = horasTrabalhadas * valorHora;

                //Calculo para os diferentes tipos de entregas
                int entregasBG = int.Parse(txtBG.Text);
                decimal valorEntregaBG = 0.4m;
                decimal totalEntregasBG = entregasBG * valorEntregaBG;

                int entregasBS = int.Parse(txtBS.Text);
                decimal valorEntregasBS = 0.25m;
                decimal totalEntregasBS = entregasBS * valorEntregasBS;

                //Calcular total do dia
                decimal totalDia = valorTotal + totalEntregasBG + totalEntregasBS;

                GuardarDados(horasTrabalhadas, valorHora, entregasBG, entregasBS, totalDia);

                // Mostrar o total
                MessageBox.Show($"Total do dia: {totalDia:C}");

                // Chamar funcao para guardar os dados no ficheiro
                //GuardarDados(totalDia);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao processar os dados: " + ex.Message);
            }
        }

        //Guardar dados numa base de dados sqlite.
        private void GuardarDados(int horas, decimal valorHora, int bg, int bs, decimal totalDia)
        {
            string connectionString = "Data Source=dados_trabalho.db;Version=3;";
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                //Verificar se ja existe registo na data selecionada
                if (VerificarDados(dateTimePicker1.Value))
                {
                    MessageBox.Show("Ja existe um registo nesta data. Por favor, selecione outra!");
                    return;
                }

                string inserirDados = @"INSERT INTO Registos (Data, HorasTrabalhadas, ValorHora, EntregasBG, EntregasBS, TotalDia)
                                        VALUES (@Data, @Horas, @ValorHora, @BG, @BS, @TotalDia);";

                using (var command = new SQLiteCommand(inserirDados, connection))
                {
                    command.Parameters.AddWithValue("@Data", dateTimePicker1.Value.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@Horas", horas);
                    command.Parameters.AddWithValue("@ValorHora", valorHora);
                    command.Parameters.AddWithValue("@BG", bg);
                    command.Parameters.AddWithValue("@BS", bs);
                    command.Parameters.AddWithValue("@TotalDia", totalDia);

                    command.ExecuteNonQuery();
                }
            }
            MessageBox.Show("Dados guardados na base de dados com sucesso!");
        }

        private bool VerificarDados(DateTime data)
        {
            string connectionString = "Data Source=dados_trabalho.db;Version=3;";
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string verificarData = "SELECT COUNT(*) FROM Registos WHERE Data = @Data";//variavel que cont�m o comando que conta o numero de registos que a data � encontrada.
                using (var command = new SQLiteCommand(verificarData, connection))
                {
                    command.Parameters.AddWithValue("@Data", data.ToString("yyyy-MM-dd"));
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    //MessageBox.Show("Count:" + count); Mensagem de teste
                    if (count == 0)
                        return false;
                    return true;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int ano = dateTimePicker2.Value.Year;
            int mes = dateTimePicker2.Value.Month;

            DateTime mesSelecionado = new DateTime(ano, mes, 1);

            decimal totalMes = CalcularTotalMes(mesSelecionado);

            MessageBox.Show($"Total a receber no mes selecionado: {totalMes:C}");
        }

        private decimal CalcularTotalMes(DateTime mesSelecionado)
        {
            string connectionString = "Data Source=dados_trabalho.db;Version=3;";
            decimal totalMes = 0;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                //strftime converte a Data para yyyy-MM.
                string query = "SELECT SUM(TotalDia) FROM Registos WHERE strftime('%Y-%m', Data) = @MesAno";

                using (var command = new SQLiteCommand(query, connection))
                {
                    //insere o ano e o mes no formato correto para leitura
                    command.Parameters.AddWithValue("@MesAno", mesSelecionado.ToString("yyyy-MM"));
                    var resultado = command.ExecuteScalar();

                    if (resultado != DBNull.Value)
                    {
                        totalMes = Convert.ToDecimal(resultado);
                    }
                }
            }
            return totalMes;
        }

        //Guardar dados num ficheiro de texto.
        //private void GuardarDados(decimal valorTotal)
        //{
        //    string caminho = "dados_trabalho.txt";
        //    using (StreamWriter sw = new StreamWriter(caminho, true)) // "true" adiciona ao ficheiro
        //    {
        //        sw.WriteLine($"{DateTime.Now:yyyy-MM-dd}; Horas: {txtHoras.Text}; Total do dia: {valorTotal:C}");
        //    }
        //    MessageBox.Show("Dados guardados com sucesso!");
        //}
    }
}