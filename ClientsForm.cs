using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace SecurityAgencyApp
{
    public class ClientsForm : Form
    {
        private DataGridView dgvClients;
        private Button btnAdd, btnEdit, btnDelete, btnSwitchType;

        private bool isLegalClientMode = true; // Режим просмотра: юридические лица по умолчанию

        public ClientsForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Клиенты";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Таблица клиентов
            dgvClients = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 400,
                ReadOnly = true,
                AutoGenerateColumns = true
            };

            // Панель с кнопками
            var panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50
            };

            btnAdd = new Button { Text = "Добавить", Location = new System.Drawing.Point(10, 10), Width = 100 };
            btnEdit = new Button { Text = "Редактировать", Location = new System.Drawing.Point(120, 10), Width = 100 };
            btnDelete = new Button { Text = "Удалить", Location = new System.Drawing.Point(230, 10), Width = 100 };
            btnSwitchType = new Button { Text = "Показать физ. лица", Location = new System.Drawing.Point(340, 10), Width = 150 };

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnSwitchType.Click += BtnSwitchType_Click;

            panel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnSwitchType });

            this.Controls.Add(dgvClients);
            this.Controls.Add(panel);
        }

        private void LoadData()
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();

                    string query = isLegalClientMode ?
                        @"SELECT LegalClientID AS ID, CompanyName AS Название, Address AS Адрес, ContactPerson AS Контактное_лицо, Phone AS Телефон, Email AS Email FROM LegalClients" :
                        @"SELECT PrivateClientID AS ID, Surname + ' ' + Name + ISNULL(' ' + Patr, '') AS ФИО, Addres AS Адрес, PassportSeries + ' ' + PassportNumber AS Паспорт, Phone AS Телефон, Email AS Email FROM PrivateClients";

                    var adapter = new SqlDataAdapter(query, conn);
                    var table = new DataTable();
                    adapter.Fill(table);
                    dgvClients.DataSource = table;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки клиентов: " + ex.Message);
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new AddEditClientForm(isLegalClientMode);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvClients.SelectedRows.Count > 0)
            {
                int clientId = Convert.ToInt32(dgvClients.SelectedRows[0].Cells["ID"].Value);

                var form = new AddEditClientForm(isLegalClientMode, clientId);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvClients.SelectedRows.Count > 0)
            {
                int clientId = Convert.ToInt32(dgvClients.SelectedRows[0].Cells["ID"].Value);
                string tableName = isLegalClientMode ? "LegalClients" : "PrivateClients";
                string idColumn = isLegalClientMode ? "LegalClientID" : "PrivateClientID";

                if (MessageBox.Show("Вы уверены, что хотите удалить этого клиента?", "Удаление клиента", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    DeleteClient(tableName, idColumn, clientId);
                    LoadData();
                }
            }
        }

        private void DeleteClient(string tableName, string idColumn, int id)
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    var cmd = new SqlCommand($"DELETE FROM {tableName} WHERE {idColumn} = @id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка удаления клиента: " + ex.Message);
                }
            }
        }

        private void BtnSwitchType_Click(object sender, EventArgs e)
        {
            isLegalClientMode = !isLegalClientMode;
            btnSwitchType.Text = isLegalClientMode ? "Показать физ. лица" : "Показать юр. лица";
            LoadData();
        }
    }
}