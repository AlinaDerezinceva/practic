using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace SecurityAgencyApp
{
    public class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnCancel;

        public string UserRole { get; private set; }
        public int UserID { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Вход в систему";
            this.Size = new System.Drawing.Size(400, 200);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(10),
                AutoSize = true
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Логин
            layout.Controls.Add(new Label { Text = "Логин" }, 0, 0);
            txtUsername = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtUsername, 1, 0);

            // Пароль
            layout.Controls.Add(new Label { Text = "Пароль" }, 0, 1);
            txtPassword = new TextBox { Dock = DockStyle.Fill, UseSystemPasswordChar = true };
            layout.Controls.Add(txtPassword, 1, 1);

            // Кнопки
            var panel = new Panel { Dock = DockStyle.Fill, AutoSize = true };
            btnLogin = new Button { Text = "Войти", Width = 100 };
            btnCancel = new Button { Text = "Отмена", Width = 100 };
            btnLogin.Click += BtnLogin_Click;
            btnCancel.Click += (s, e) => this.Close();

            panel.Controls.Add(btnLogin);
            panel.Controls.Add(btnCancel);
            layout.Controls.Add(panel, 1, 2);

            this.Controls.Add(layout);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();

                    var cmd = new SqlCommand("SELECT * FROM Users WHERE Username = @username", conn);
                    cmd.Parameters.AddWithValue("@username", username);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedPassword = reader["PasswordHash"].ToString();
                            bool isActive = Convert.ToBoolean(reader["IsActive"]);
                            string role = reader["Role"].ToString();

                            // Простая проверка хэша (в реальном проекте используйте Hash.Verify)
                            if (isActive && password == storedPassword)
                            {
                                UserRole = role;
                                UserID = Convert.ToInt32(reader["UserID"]);
                                DialogResult = DialogResult.OK;
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Неверный логин или пароль");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Пользователь не найден");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка подключения к базе данных: " + ex.Message);
                }
            }
        }
    }
}