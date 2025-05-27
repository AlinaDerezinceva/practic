using Microsoft.Data.SqlClient;
using System;
using System.Windows.Forms;

namespace SecurityAgencyApp
{
    public class AddEditClientForm : Form
    {
        private readonly bool _isLegalClient;
        private readonly int? _clientId = null;

        private TextBox txtName, txtAddress, txtPhone, txtEmail;
        private TextBox txtContactPersonOrPatronymic;
        private TextBox txtPassportOrCompany;
        private DateTimePicker dtpContractDate;

        public AddEditClientForm(bool isLegalClient, int? clientId = null)
        {
            _isLegalClient = isLegalClient;
            _clientId = clientId;

            InitializeComponent();
            if (_clientId.HasValue)
            {
                LoadData();
                this.Text = _isLegalClient ? "Редактировать юридическое лицо" : "Редактировать физическое лицо";
            }
            else
            {
                this.Text = _isLegalClient ? "Новое юридическое лицо" : "Новое физическое лицо";
            }
        }

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = _isLegalClient ? 6 : 7,
                AutoSize = true
            };

            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(SizeType.Percent, 100));

            int row = 0;

            // Название или ФИО
            layout.Controls.Add(new Label { Text = _isLegalClient ? "Название" : "Фамилия" }, 0, row);
            txtName = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtName, 1, row++);

            // Адрес
            layout.Controls.Add(new Label { Text = "Адрес" }, 0, row);
            txtAddress = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtAddress, 1, row++);

            // Контактное лицо / Отчество
            layout.Controls.Add(new Label { Text = _isLegalClient ? "Контактное лицо" : "Отчество" }, 0, row);
            txtContactPersonOrPatronymic = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtContactPersonOrPatronymic, 1, row++);

            if (!_isLegalClient)
            {
                // Паспортные данные
                layout.Controls.Add(new Label { Text = "Серия паспорта" }, 0, row);
                txtPassportOrCompany = new TextBox { Dock = DockStyle.Fill };
                layout.Controls.Add(txtPassportOrCompany, 1, row++);
            }

            // Телефон
            layout.Controls.Add(new Label { Text = "Телефон" }, 0, row);
            txtPhone = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtPhone, 1, row++);

            // Email
            layout.Controls.Add(new Label { Text = "Email" }, 0, row);
            txtEmail = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtEmail, 1, row++);

            // Кнопка сохранения
            var btnSave = new Button { Text = "Сохранить", Width = 100, Anchor = AnchorStyles.Right };
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

                if (_isLegalClient)
                {
                    var cmd = new SqlCommand("SELECT * FROM LegalClients WHERE LegalClientID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", _clientId.Value);
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        txtName.Text = reader["CompanyName"].ToString();
                        txtAddress.Text = reader["Address"].ToString();
                        txtContactPersonOrPatronymic.Text = reader["ContactPerson"].ToString();
                        txtPhone.Text = reader["Phone"].ToString();
                        txtEmail.Text = reader["Email"].ToString();
                    }
                }
                else
                {
                    var cmd = new SqlCommand("SELECT * FROM PrivateClients WHERE PrivateClientID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", _clientId.Value);
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        txtName.Text = reader["Surname"].ToString();
                        txtAddress.Text = reader["Addres"].ToString();
                        txtContactPersonOrPatronymic.Text = reader["Patr"].ToString();
                        txtPassportOrCompany.Text = reader["PassportSeries"].ToString() + " " + reader["PassportNumber"].ToString();
                        txtPhone.Text = reader["Phone"].ToString();
                        txtEmail.Text = reader["Email"].ToString();
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите имя");
                return;
            }

            using (var conn = Database.GetConnection())
            {
                conn.Open();

                if (_isLegalClient)
                {
                    if (_clientId.HasValue)
                    {
                        var cmd = new SqlCommand(@"
                            UPDATE LegalClients SET 
                                CompanyName = @name, Address = @address, ContactPerson = @contact, 
                                Phone = @phone, Email = @email
                            WHERE LegalClientID = @id", conn);
                        cmd.Parameters.AddWithValue("@id", _clientId.Value);
                        cmd.Parameters.AddWithValue("@name", txtName.Text);
                        cmd.Parameters.AddWithValue("@address", txtAddress.Text);
                        cmd.Parameters.AddWithValue("@contact", txtContactPersonOrPatronymic.Text);
                        cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        var cmd = new SqlCommand(@"
                            INSERT INTO LegalClients (CompanyName, Address, ContactPerson, Phone, Email)
                            VALUES (@name, @addres, @contact, @phone, @email)", conn);
                        cmd.Parameters.AddWithValue("@name", txtName.Text);
                        cmd.Parameters.AddWithValue("@addres", txtAddress.Text);
                        cmd.Parameters.AddWithValue("@contact", txtContactPersonOrPatronymic.Text);
                        cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    string passport = txtPassportOrCompany.Text.Trim();
                    string[] parts = passport.Split(' ');

                    if (parts.Length < 2)
                    {
                        MessageBox.Show("Введите корректные паспортные данные");
                        return;
                    }

                    string series = parts[0];
                    string number = parts[1];

                    if (_clientId.HasValue)
                    {
                        var cmd = new SqlCommand(@"
                            UPDATE PrivateClients SET 
                                Surname = @surname, Addres = @address, Patr = @patr, 
                                PassportSeries = @series, PassportNumber = @number, Phone = @phone, Email = @email
                            WHERE PrivateClientID = @id", conn);
                        cmd.Parameters.AddWithValue("@id", _clientId.Value);
                        cmd.Parameters.AddWithValue("@surname", txtName.Text);
                        cmd.Parameters.AddWithValue("@address", txtAddress.Text);
                        cmd.Parameters.AddWithValue("@patr", txtContactPersonOrPatronymic.Text);
                        cmd.Parameters.AddWithValue("@series", series);
                        cmd.Parameters.AddWithValue("@number", number);
                        cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        var cmd = new SqlCommand(@"
                            INSERT INTO PrivateClients (Name, Surname, Patr, Addres, PassportSeries, PassportNumber, Phone, Email)
                            VALUES ('', @surname, @patr, @address, @series, @number, @phone, @email)", conn);
                        cmd.Parameters.AddWithValue("@surname", txtName.Text);
                        cmd.Parameters.AddWithValue("@patr", txtContactPersonOrPatronymic.Text);
                        cmd.Parameters.AddWithValue("@address", txtAddress.Text);
                        cmd.Parameters.AddWithValue("@series", series);
                        cmd.Parameters.AddWithValue("@number", number);
                        cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                        cmd.ExecuteNonQuery();
                    }
                }

                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}