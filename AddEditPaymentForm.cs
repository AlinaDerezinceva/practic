using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace SecurityAgencyApp
{
    public class AddEditPaymentForm : Form
    {
        private int? paymentId = null;
        private TextBox txtAmount, txtDocumentNumber;
        private DateTimePicker dtpPaymentDate;
        private ComboBox cmbContract;

        public AddEditPaymentForm(int id = -1)
        {
            paymentId = id == -1 ? null : (int?)id;
            InitializeComponent();
            LoadContracts();
            if (paymentId.HasValue)
            {
                LoadData();
                this.Text = "Редактировать платеж";
            }
            else
            {
                this.Text = "Добавить платеж";
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Платеж";
            this.Size = new System.Drawing.Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(10),
                AutoSize = true
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Договор
            layout.Controls.Add(new Label { Text = "Договор" }, 0, 0);
            cmbContract = new ComboBox { Dock = DockStyle.Fill };
            layout.Controls.Add(cmbContract, 1, 0);

            // Сумма
            layout.Controls.Add(new Label { Text = "Сумма" }, 0, 1);
            txtAmount = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtAmount, 1, 1);

            // Дата платежа
            layout.Controls.Add(new Label { Text = "Дата платежа" }, 0, 2);
            dtpPaymentDate = new DateTimePicker();
            layout.Controls.Add(dtpPaymentDate, 1, 2);

            // Номер документа
            layout.Controls.Add(new Label { Text = "Номер документа" }, 0, 3);
            txtDocumentNumber = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtDocumentNumber, 1, 3);

            // Кнопка сохранения
            var btnSave = new Button { Text = "Сохранить", Width = 100, Anchor = AnchorStyles.Right };
            btnSave.Click += BtnSave_Click;

            var panel = new Panel { Dock = DockStyle.Bottom };
            panel.Controls.Add(btnSave);

            this.Controls.Add(layout);
            this.Controls.Add(panel);
        }

        private void LoadContracts()
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT ContractID, ContracNumber FROM Contracts", conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    cmbContract.Items.Add(new ContractItem
                    {
                        ContractID = Convert.ToInt32(reader["ContractID"]),
                        Number = reader["ContracNumber"]?.ToString()
                    });
                }
            }
        }

        private void LoadData()
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM Payments WHERE PaymentID = @id", conn);
                cmd.Parameters.AddWithValue("@id", paymentId.Value);
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    txtAmount.Text = reader["Amount"].ToString();
                    dtpPaymentDate.Value = Convert.ToDateTime(reader["PaymentDate"]);
                    txtDocumentNumber.Text = reader["DocumentNumber"]?.ToString();

                    if (reader["ContractID"] != DBNull.Value)
                    {
                        int contractId = Convert.ToInt32(reader["ContractID"]);
                        foreach (var item in cmbContract.Items)
                        {
                            if (item is ContractItem ci && ci.ContractID == contractId)
                            {
                                cmbContract.SelectedItem = ci;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbContract.SelectedItem == null || string.IsNullOrWhiteSpace(txtAmount.Text))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            var contractItem = (ContractItem)cmbContract.SelectedItem;
            decimal amount;
            if (!decimal.TryParse(txtAmount.Text, out amount))
            {
                MessageBox.Show("Введите корректную сумму");
                return;
            }

            using (var conn = Database.GetConnection())
            {
                conn.Open();

                if (paymentId.HasValue)
                {
                    var cmd = new SqlCommand(@"
                        UPDATE Payments SET 
                            ContractID = @contractId, 
                            Amount = @amount, 
                            PaymentDate = @date, 
                            DocumentNumber = @doc
                        WHERE PaymentID = @id", conn);
                    cmd.Parameters.AddWithValue("@contractId", contractItem.ContractID);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    cmd.Parameters.AddWithValue("@date", dtpPaymentDate.Value);
                    cmd.Parameters.AddWithValue("@doc", txtDocumentNumber.Text);
                    cmd.Parameters.AddWithValue("@id", paymentId.Value);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    var cmd = new SqlCommand(@"
                        INSERT INTO Payments (ContractID, Amount, PaymentDate, DocumentNumber)
                        VALUES (@contractId, @amount, @date, @doc)", conn);
                    cmd.Parameters.AddWithValue("@contractId", contractItem.ContractID);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    cmd.Parameters.AddWithValue("@date", dtpPaymentDate.Value);
                    cmd.Parameters.AddWithValue("@doc", txtDocumentNumber.Text);
                    cmd.ExecuteNonQuery();
                }

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private class ContractItem
        {
            public int ContractID { get; set; }
            public string Number { get; set; }

            public override string ToString()
            {
                return Number;
            }
        }
    }
}