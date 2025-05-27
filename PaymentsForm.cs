using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace SecurityAgencyApp
{
    public class PaymentsForm : Form
    {
        private DataGridView dgvPayments;
        private Button btnAdd, btnEdit, btnDelete;

        public PaymentsForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Платежи по договорам";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Таблица платежей
            dgvPayments = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 400,
                ReadOnly = true,
                AutoGenerateColumns = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            };

            // Настройка колонок
            dgvPayments.Columns.Add("PaymentID", "ID");
            dgvPayments.Columns.Add("ContractNumber", "Номер договора");
            dgvPayments.Columns.Add("Amount", "Сумма");
            dgvPayments.Columns.Add("PaymentDate", "Дата платежа");
            dgvPayments.Columns.Add("DocumentNumber", "Номер документа");

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

            this.Controls.Add(dgvPayments);
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
                            p.PaymentID,
                            c.ContracNumber AS ContractNumber,
                            p.Amount,
                            p.PaymentDate,
                            p.DocumentNumber
                        FROM Payments p
                        JOIN Contracts c ON p.ContractID = c.ContractID";

                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        var table = new DataTable();
                        table.Load(reader);
                        dgvPayments.DataSource = table;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки платежей: " + ex.Message);
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new AddEditPaymentForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvPayments.SelectedRows.Count > 0)
            {
                int paymentId = Convert.ToInt32(dgvPayments.SelectedRows[0].Cells["PaymentID"].Value);
                var form = new AddEditPaymentForm(paymentId);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvPayments.SelectedRows.Count > 0)
            {
                int paymentId = Convert.ToInt32(dgvPayments.SelectedRows[0].Cells["PaymentID"].Value);

                if (MessageBox.Show("Вы уверены, что хотите удалить этот платеж?", "Удаление платежа", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    DeletePayment(paymentId);
                    LoadData();
                }
            }
        }

        private void DeletePayment(int id)
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    var cmd = new SqlCommand("DELETE FROM Payments WHERE PaymentID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка удаления платежа: " + ex.Message);
                }
            }
        }
    }
}