using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace SecurityAgencyApp
{
    public class AddEditOrderForm : Form
    {
        private int? orderId = null;
        private TextBox txtOrderNumber, txtDescription;
        private DateTimePicker dtpOrderDate;
        private ComboBox cmbEmployee, cmbOrderType;
        private Button btnSave;

        public AddEditOrderForm(int id = -1)
        {
            orderId = id == -1 ? null : (int?)id;
            InitializeComponents();
            if (orderId.HasValue)
            {
                LoadData();
                this.Text = "Редактировать приказ";
            }
            else
            {
                this.Text = "Новый приказ";
            }
        }

        private void InitializeComponents()
        {
            this.Size = new System.Drawing.Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(10),
                AutoSize = true
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Номер приказа
            layout.Controls.Add(new Label { Text = "Номер" }, 0, 0);
            txtOrderNumber = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtOrderNumber, 1, 0);

            // Дата
            layout.Controls.Add(new Label { Text = "Дата" }, 0, 1);
            dtpOrderDate = new DateTimePicker();
            layout.Controls.Add(dtpOrderDate, 1, 1);

            // Тип приказа
            layout.Controls.Add(new Label { Text = "Тип" }, 0, 2);
            cmbOrderType = new ComboBox
            {
                Items = { "Прием на работу", "Увольнение", "Отпуск", "Перевод", "Прочее" },
                SelectedIndex = 0
            };
            layout.Controls.Add(cmbOrderType, 1, 2);

            // Сотрудник
            layout.Controls.Add(new Label { Text = "Сотрудник" }, 0, 3);
            cmbEmployee = new ComboBox();
            layout.Controls.Add(cmbEmployee, 1, 3);

            // Кнопка сохранения
            btnSave = new Button { Text = "Сохранить", Width = 100, Anchor = AnchorStyles.Right };
            btnSave.Click += BtnSave_Click;

            var panel = new Panel { Dock = DockStyle.Bottom };
            panel.Controls.Add(btnSave);

            this.Controls.Add(layout);
            this.Controls.Add(panel);

            LoadEmployees();
        }

        private void LoadEmployees()
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();

                    var cmd = new SqlCommand("SELECT EmployeeID, LastName + ' ' + FirstName AS Name FROM Employees WHERE IsActive = 1", conn);
                    var reader = cmd.ExecuteReader();

                    if (!reader.HasRows)
                    {
                        MessageBox.Show("Сотрудники не найдены.");
                        return;
                    }

                    while (reader.Read())
                    {
                        cmbEmployee.Items.Add(reader["Name"].ToString());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки сотрудников: " + ex.Message);
                }
            }
        }

        private void LoadData()
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM Orders WHERE OrderID = @id", conn);
                cmd.Parameters.AddWithValue("@id", orderId.Value);
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    txtOrderNumber.Text = reader["OrderNumber"]?.ToString();
                    dtpOrderDate.Value = Convert.ToDateTime(reader["OrderDate"]);
                    cmbOrderType.SelectedItem = reader["OrderType"]?.ToString();

                    if (reader["EmployeeID"] != DBNull.Value)
                    {
                        var employeeId = Convert.ToInt32(reader["EmployeeID"]);
                        // Здесь можно реализовать поиск по списку сотрудников
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtOrderNumber.Text))
            {
                MessageBox.Show("Введите номер приказа");
                return;
            }

            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    if (orderId.HasValue)
                    {
                        var cmd = new SqlCommand(@"
                            UPDATE Orders SET 
                                OrderNumber = @num, OrderDate = @date, OrderType = @type, EmployeeID = @emp
                            WHERE OrderID = @id", conn);
                        cmd.Parameters.AddWithValue("@num", txtOrderNumber.Text);
                        cmd.Parameters.AddWithValue("@date", dtpOrderDate.Value);
                        cmd.Parameters.AddWithValue("@type", cmbOrderType.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@emp", cmbEmployee.SelectedIndex >= 0 ? cmbEmployee.SelectedIndex : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@id", orderId.Value);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        var cmd = new SqlCommand(@"
                            INSERT INTO Orders (OrderNumber, OrderDate, OrderType, EmployeeID)
                            VALUES (@num, @date, @type, @emp)", conn);
                        cmd.Parameters.AddWithValue("@num", txtOrderNumber.Text);
                        cmd.Parameters.AddWithValue("@date", dtpOrderDate.Value);
                        cmd.Parameters.AddWithValue("@type", cmbOrderType.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@emp", cmbEmployee.SelectedIndex >= 0 ? cmbEmployee.SelectedIndex : (object)DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка сохранения: " + ex.Message);
                }
            }
        }
    }
}