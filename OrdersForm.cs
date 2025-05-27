using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace SecurityAgencyApp
{
    public class OrdersForm : Form
    {
        private DataGridView dgvOrders;
        private Button btnAdd, btnEdit, btnDelete;

        public OrdersForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Приказы";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Таблица
            dgvOrders = new DataGridView
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

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;

            panel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });

            this.Controls.Add(dgvOrders);
            this.Controls.Add(panel);
        }

        private void LoadData()
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    var adapter = new SqlDataAdapter("SELECT * FROM Orders", conn);
                    var table = new DataTable();
                    adapter.Fill(table);
                    dgvOrders.DataSource = table;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new AddEditOrderForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count > 0)
            {
                var id = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["OrderID"].Value);
                var form = new AddEditOrderForm(id);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count > 0)
            {
                var id = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["OrderID"].Value);
                if (MessageBox.Show("Вы уверены?", "Удаление", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    DeleteOrder(id);
                    LoadData();
                }
            }
        }

        private void DeleteOrder(int id)
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    var cmd = new SqlCommand("DELETE FROM Orders WHERE OrderID = @id", conn);
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