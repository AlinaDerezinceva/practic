using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace SecurityAgencyApp
{
    public class EmployeesForm : Form
    {
        private DataGridView dgvEmployees;
        private Button btnAdd, btnEdit, btnDelete;

        public EmployeesForm()
        {
            InitializeComponents();
            LoadData();
        }

        private void InitializeComponents()
        {
            this.Text = "Сотрудники";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            dgvEmployees = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 400,
                ReadOnly = true,
                AutoGenerateColumns = true
            };

            var panel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50
            };

            btnAdd = new Button { Text = "Добавить", Location = new System.Drawing.Point(10, 10), Width = 100 };
            btnEdit = new Button { Text = "Редактировать", Location = new System.Drawing.Point(120, 10), Width = 100 };
            btnDelete = new Button { Text = "Удалить", Location = new System.Drawing.Point(230, 10), Width = 100 };

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;

            panel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });
            this.Controls.Add(dgvEmployees);
            this.Controls.Add(panel);
        }

        private void LoadData()
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    var adapter = new SqlDataAdapter("SELECT * FROM Employees", conn);
                    var table = new DataTable();
                    adapter.Fill(table);
                    dgvEmployees.DataSource = table;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new AddEditEmployeeForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.SelectedRows.Count > 0)
            {
                var id = Convert.ToInt32(dgvEmployees.SelectedRows[0].Cells["EmployeeID"].Value);
                var form = new AddEditEmployeeForm(id);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.SelectedRows.Count > 0)
            {
                var id = Convert.ToInt32(dgvEmployees.SelectedRows[0].Cells["EmployeeID"].Value);
                if (MessageBox.Show("Вы уверены?", "Удаление", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    DeleteEmployee(id);
                    LoadData();
                }
            }
        }

        private void DeleteEmployee(int id)
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    var cmd = new SqlCommand("DELETE FROM Employees WHERE EmployeeID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка удаления: " + ex.Message);
                }
            }
        }
    }
}