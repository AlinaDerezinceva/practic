using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace SecurityAgencyApp
{
    public class AddEditDutyScheduleForm : Form
    {
        private readonly int? _scheduleId = null;

        private ComboBox cmbEmployee, cmbContract;
        private DateTimePicker dtpDutyDate;
        private TextBox txtReason;

        public AddEditDutyScheduleForm(int scheduleId = -1)
        {
            _scheduleId = scheduleId == -1 ? null : (int?)scheduleId;

            InitializeComponent();
            LoadEmployeesAndContracts();

            if (_scheduleId.HasValue)
            {
                LoadData();
                this.Text = "Редактировать график дежурств";
            }
            else
            {
                this.Text = "Новый график дежурств";
            }
        }

        private void InitializeComponent()
        {
            this.Text = "График дежурств";
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

            // Сотрудник
            layout.Controls.Add(new Label { Text = "Сотрудник" }, 0, 0);
            cmbEmployee = new ComboBox { Dock = DockStyle.Fill };
            layout.Controls.Add(cmbEmployee, 1, 0);

            // Договор
            layout.Controls.Add(new Label { Text = "Договор" }, 0, 1);
            cmbContract = new ComboBox { Dock = DockStyle.Fill };
            layout.Controls.Add(cmbContract, 1, 1);

            // Дата дежурства
            layout.Controls.Add(new Label { Text = "Дата дежурства" }, 0, 2);
            dtpDutyDate = new DateTimePicker();
            layout.Controls.Add(dtpDutyDate, 1, 2);

            // Причина замены
            layout.Controls.Add(new Label { Text = "Причина замены" }, 0, 3);
            txtReason = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 80 };
            layout.Controls.Add(txtReason, 1, 3);

            // Кнопка сохранения
            var btnSave = new Button { Text = "Сохранить", Width = 100, Anchor = AnchorStyles.Right };
            btnSave.Click += BtnSave_Click;

            var panel = new Panel { Dock = DockStyle.Bottom };
            panel.Controls.Add(btnSave);

            this.Controls.Add(layout);
            this.Controls.Add(panel);
        }

        private void LoadEmployeesAndContracts()
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();

                // Загрузка сотрудников
                var empCmd = new SqlCommand("SELECT EmployeeID, LastName + ' ' + FirstName AS Name FROM Employees WHERE IsActive = 1", conn);
                var empReader = empCmd.ExecuteReader();
                while (empReader.Read())
                {
                    cmbEmployee.Items.Add(new EmployeeItem
                    {
                        ID = Convert.ToInt32(empReader["EmployeeID"]),
                        Name = empReader["Name"].ToString()
                    });
                }
                empReader.Close();

                // Загрузка договоров
                var contractCmd = new SqlCommand("SELECT ContractID, ContracNumber FROM Contracts", conn);
                var contractReader = contractCmd.ExecuteReader();
                while (contractReader.Read())
                {
                    cmbContract.Items.Add(new ContractItem
                    {
                        ContractID = Convert.ToInt32(contractReader["ContractID"]),
                        Number = contractReader["ContracNumber"].ToString()
                    });
                }
                contractReader.Close();
            }
        }

        private void LoadData()
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();

                var cmd = new SqlCommand(@"
                    SELECT * FROM DutyShedule WHERE SheduleID = @id", conn);
                cmd.Parameters.AddWithValue("@id", _scheduleId.Value);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    foreach (var item in cmbEmployee.Items)
                    {
                        if (item is EmployeeItem ei && ei.ID == Convert.ToInt32(reader["EmployeeID"]))
                        {
                            cmbEmployee.SelectedItem = ei;
                            break;
                        }
                    }

                    foreach (var item in cmbContract.Items)
                    {
                        if (item is ContractItem ci && ci.ContractID == Convert.ToInt32(reader["ContractID"]))
                        {
                            cmbContract.SelectedItem = ci;
                            break;
                        }
                    }

                    dtpDutyDate.Value = Convert.ToDateTime(reader["DutyDate"]);
                    txtReason.Text = reader["Reason"]?.ToString();
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbEmployee.SelectedItem == null || cmbContract.SelectedItem == null)
            {
                MessageBox.Show("Выберите сотрудника и договор");
                return;
            }

            var employee = (cmbEmployee.SelectedItem as AddEditDutyScheduleForm.EmployeeItem);
            var contract = (cmbContract.SelectedItem as AddEditDutyScheduleForm.ContractItem);

            using (var conn = Database.GetConnection())
            {
                conn.Open();

                if (_scheduleId.HasValue)
                {
                    var cmd = new SqlCommand(@"
                        UPDATE DutyShedule SET
                            EmployeeID = @employeeId,
                            ContractID = @contractId,
                            DutyDate = @date,
                            Reason = @reason
                        WHERE SheduleID = @id", conn);

                    cmd.Parameters.AddWithValue("@id", _scheduleId.Value);
                    cmd.Parameters.AddWithValue("@employeeId", employee.ID);
                    cmd.Parameters.AddWithValue("@contractId", contract.ContractID);
                    cmd.Parameters.AddWithValue("@date", dtpDutyDate.Value.Date);
                    cmd.Parameters.AddWithValue("@reason", txtReason.Text ?? "");

                    cmd.ExecuteNonQuery();
                }
                else
                {
                    var cmd = new SqlCommand(@"
                        INSERT INTO DutyShedule (EmployeeID, ContractID, DutyDate, Reason)
                        VALUES (@employeeId, @contractId, @date, @reason)", conn);

                    cmd.Parameters.AddWithValue("@employeeId", employee.ID);
                    cmd.Parameters.AddWithValue("@contractId", contract.ContractID);
                    cmd.Parameters.AddWithValue("@date", dtpDutyDate.Value.Date);
                    cmd.Parameters.AddWithValue("@reason", string.IsNullOrEmpty(txtReason.Text) ? (object)DBNull.Value : txtReason.Text);

                    cmd.ExecuteNonQuery();
                }

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private class EmployeeItem
        {
            public int ID { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return Name;
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