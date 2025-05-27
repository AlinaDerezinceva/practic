using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SecurityAgencyApp
{
    public class AddEditEmployeeForm : Form
    {
        private int? employeeId = null;
        private TextBox txtFirstName, txtLastName, txtMiddleName, txtPosition;
        private Button btnSave;

        public AddEditEmployeeForm(int id = -1)
        {
            employeeId = id == -1 ? null : (int?)id;
            InitializeComponents();
            if (employeeId.HasValue)
            {
                LoadData();
                this.Text = "Редактировать сотрудника";
            }
            else
            {
                this.Text = "Добавить сотрудника";
            }
        }

        private void InitializeComponents()
        {
            this.Size = new Size(400, 300);
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

            layout.Controls.Add(new Label { Text = "Фамилия" }, 0, 0);
            txtLastName = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtLastName, 1, 0);

            layout.Controls.Add(new Label { Text = "Имя" }, 0, 1);
            txtFirstName = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtFirstName, 1, 1);

            layout.Controls.Add(new Label { Text = "Отчество" }, 0, 2);
            txtMiddleName = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtMiddleName, 1, 2);

            layout.Controls.Add(new Label { Text = "Должность" }, 0, 3);
            txtPosition = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtPosition, 1, 3);

            btnSave = new Button { Text = "Сохранить", Width = 100, Anchor = AnchorStyles.Right };
            btnSave.Click += BtnSave_Click;

            var panel = new Panel { Dock = DockStyle.Bottom };
            panel.Controls.Add(btnSave);

            this.Controls.Add(layout);
            this.Controls.Add(panel);
        }

        private void LoadData()
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM Employees WHERE EmployeeID = @id", conn);
                cmd.Parameters.AddWithValue("@id", employeeId.Value);
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    txtLastName.Text = reader["LastName"]?.ToString();
                    txtFirstName.Text = reader["FirstName"]?.ToString();
                    txtMiddleName.Text = reader["MiddleName"]?.ToString();
                    txtPosition.Text = reader["Position"]?.ToString();
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Заполните обязательные поля");
                return;
            }

            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    if (employeeId.HasValue)
                    {
                        var cmd = new SqlCommand(@"
                            UPDATE Employees SET 
                                FirstName = @fn, LastName = @ln, MiddleName = @mn, Position = @pos
                            WHERE EmployeeID = @id", conn);
                        cmd.Parameters.AddWithValue("@fn", txtFirstName.Text);
                        cmd.Parameters.AddWithValue("@ln", txtLastName.Text);
                        cmd.Parameters.AddWithValue("@mn", txtMiddleName.Text);
                        cmd.Parameters.AddWithValue("@pos", txtPosition.Text);
                        cmd.Parameters.AddWithValue("@id", employeeId.Value);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        var cmd = new SqlCommand(@"
                            INSERT INTO Employees (FirstName, LastName, MiddleName, Position)
                            VALUES (@fn, @ln, @mn, @pos)", conn);
                        cmd.Parameters.AddWithValue("@fn", txtFirstName.Text);
                        cmd.Parameters.AddWithValue("@ln", txtLastName.Text);
                        cmd.Parameters.AddWithValue("@mn", txtMiddleName.Text);
                        cmd.Parameters.AddWithValue("@pos", txtPosition.Text);
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