using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace SecurityAgencyApp
{
    public class ContractsForm : Form
    {
        private DataGridView dgvContracts;
        private Button btnAdd, btnEdit, btnDelete, btnGenerateInvoice;

        public ContractsForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Договоры";
            this.Size = new System.Drawing.Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Таблица договоров
            dgvContracts = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 400,
                ReadOnly = true,
                AutoGenerateColumns = false
            };

            // Настройка колонок
            dgvContracts.Columns.Add("ContractID", "ID");
            dgvContracts.Columns.Add("ContractNumber", "Номер договора");
            dgvContracts.Columns.Add("ClientName", "Клиент");
            dgvContracts.Columns.Add("EventAddress", "Адрес мероприятия");
            dgvContracts.Columns.Add("EventStart", "Дата начала");
            dgvContracts.Columns.Add("EventEnd", "Дата окончания");
            dgvContracts.Columns.Add("TotalAmount", "Сумма");

            // Панель с кнопками
            var panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50
            };

            btnAdd = new Button { Text = "Добавить", Location = new System.Drawing.Point(10, 10), Width = 100 };
            btnEdit = new Button { Text = "Редактировать", Location = new System.Drawing.Point(120, 10), Width = 100 };
            btnDelete = new Button { Text = "Удалить", Location = new System.Drawing.Point(230, 10), Width = 100 };
            btnGenerateInvoice = new Button { Text = "Сформировать счет", Location = new System.Drawing.Point(340, 10), Width = 150 };

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnGenerateInvoice.Click += BtnGenerateInvoice_Click;

            panel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnGenerateInvoice });

            this.Controls.Add(dgvContracts);
            this.Controls.Add(panel);
        }

        private void LoadData()
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            c.ContractID,
                            c.ContracNumber AS ContractNumber,
                            ISNULL(l.CompanyName, p.Surname + ' ' + p.Name) AS ClientName,
                            c.EventAddress,
                            c.EventStart,
                            c.EventEnd,
                            c.TotalAmount
                        FROM Contracts c
                        LEFT JOIN LegalClients l ON c.LegalClientID = l.LegalClientID
                        LEFT JOIN PrivateClients p ON c.PrivateClientID = p.PrivateClientID";

                    var adapter = new SqlDataAdapter(query, conn);
                    var table = new DataTable();
                    adapter.Fill(table);

                    dgvContracts.DataSource = table;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки договоров: " + ex.Message);
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new AddEditContractForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvContracts.SelectedRows.Count > 0)
            {
                int contractId = Convert.ToInt32(dgvContracts.SelectedRows[0].Cells["ContractID"].Value);
                var form = new AddEditContractForm(contractId);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvContracts.SelectedRows.Count > 0)
            {
                int contractId = Convert.ToInt32(dgvContracts.SelectedRows[0].Cells["ContractID"].Value);

                if (MessageBox.Show("Вы уверены, что хотите удалить этот договор?", "Удаление договора", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    DeleteContract(contractId);
                    LoadData();
                }
            }
        }

        private void DeleteContract(int id)
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    var cmd = new SqlCommand("DELETE FROM Contracts WHERE ContractID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка удаления договора: " + ex.Message);
                }
            }
        }

        private void BtnGenerateInvoice_Click(object sender, EventArgs e)
        {
            if (dgvContracts.SelectedRows.Count > 0)
            {
                int contractId = Convert.ToInt32(dgvContracts.SelectedRows[0].Cells["ContractID"].Value);
                GenerateInvoice(contractId);
            }
            else
            {
                MessageBox.Show("Выберите договор для формирования счета.");
            }
        }

        private void GenerateInvoice(int contractId)
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            c.ContracNumber AS Number,
                            c.ContractData AS Date,
                            ISNULL(l.CompanyName, p.Surname + ' ' + p.Name) AS ClientName,
                            c.EventAddress,
                            c.EventStart,
                            c.EventEnd,
                            c.TotalAmount
                        FROM Contracts c
                        LEFT JOIN LegalClients l ON c.LegalClientID = l.LegalClientID
                        LEFT JOIN PrivateClients p ON c.PrivateClientID = p.PrivateClientID
                        WHERE c.ContractID = @id";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", contractId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string clientName = reader["ClientName"]?.ToString();
                                string eventAddress = reader["EventAddress"]?.ToString();
                                DateTime contractDate = Convert.ToDateTime(reader["Date"]);
                                DateTime eventStart = Convert.ToDateTime(reader["EventStart"]);
                                DateTime eventEnd = Convert.ToDateTime(reader["EventEnd"]);
                                decimal totalAmount = Convert.ToDecimal(reader["TotalAmount"]);

                                // Пример вывода счета
                                string invoiceText = $@"СЧЕТ №{reader["Number"]}
Дата: {contractDate:dd.MM.yyyy}

Клиент: {clientName}
Адрес мероприятия: {eventAddress}
Период охраны: {eventStart} - {eventEnd}
Сумма: {totalAmount:C}";

                                MessageBox.Show(invoiceText, "Счет клиенту");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка формирования счета: " + ex.Message);
                }
            }
        }
    }
}