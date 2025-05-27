using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace SecurityAgencyApp
{
    public class DutyScheduleForm : Form
    {
        private DataGridView dgvDutySchedule;
        private Button btnAdd, btnEdit, btnDelete, btnFilterByEmployee, btnFilterByContract;

        private ComboBox cmbEmployeeFilter, cmbContractFilter;

        public DutyScheduleForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "График дежурств";
            this.Size = new System.Drawing.Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Таблица
            dgvDutySchedule = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ReadOnly = true
            };

            dgvDutySchedule.Columns.Add("SheduleID", "ID");
            dgvDutySchedule.Columns.Add("ContractNumber", "Номер договора");
            dgvDutySchedule.Columns.Add("EmployeeName", "Сотрудник");
            dgvDutySchedule.Columns.Add("DutyDate", "Дата дежурства");
            dgvDutySchedule.Columns.Add("Reason", "Причина замены");

            // Панель фильтров
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50
            };

            cmbEmployeeFilter = new ComboBox { Location = new System.Drawing.Point(10, 10), Width = 200 };
            cmbContractFilter = new ComboBox { Location = new System.Drawing.Point(220, 10), Width = 200 };

            var lblEmployee = new Label { Text = "Фильтр по сотруднику:", Location = new System.Drawing.Point(10, 35) };
            var lblContract = new Label { Text = "Фильтр по договору:", Location = new System.Drawing.Point(220, 35) };

            var btnClearFilters = new Button { Text = "Очистить фильтры", Location = new System.Drawing.Point(440, 10), Width = 150 };
            btnClearFilters.Click += (s, e) =>
            {
                cmbEmployeeFilter.SelectedIndex = -1;
                cmbContractFilter.SelectedIndex = -1;
                LoadData();
            };

            filterPanel.Controls.AddRange(new Control[]
            {
                cmbEmployeeFilter,
                cmbContractFilter,
                lblEmployee,
                lblContract,
                btnClearFilters
            });

            // Кнопки управления
            var buttonPanel = new Panel
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

            buttonPanel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });

            // Загрузка фильтров
            LoadEmployeesToFilter();
            LoadContractsToFilter();

            // Добавление элементов на форму
            this.Controls.Add(filterPanel);
            this.Controls.Add(dgvDutySchedule);
            this.Controls.Add(buttonPanel);
        }

        private void LoadEmployeesToFilter()
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    var cmd = new SqlCommand("SELECT EmployeeID, LastName + ' ' + FirstName AS Name FROM Employees WHERE IsActive = 1", conn);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        cmbEmployeeFilter.Items.Add(new EmployeeItem
                        {
                            ID = Convert.ToInt32(reader["EmployeeID"]),
                            Name = reader["Name"].ToString()
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки сотрудников: " + ex.Message);
                }
            }
        }

        private void LoadContractsToFilter()
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    var cmd = new SqlCommand("SELECT ContractID, ContracNumber FROM Contracts", conn);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        cmbContractFilter.Items.Add(new ContractItem
                        {
                            ContractID = Convert.ToInt32(reader["ContractID"]),
                            Number = reader["ContracNumber"].ToString()
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки договоров: " + ex.Message);
                }
            }
        }

        private void LoadData(int? employeeId = null, int? contractId = null)
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            d.SheduleID,
                            c.ContracNumber AS ContractNumber,
                            e.LastName + ' ' + e.FirstName AS EmployeeName,
                            d.DutyDate,
                            d.Reason
                        FROM DutyShedule d
                        JOIN Employees e ON d.EmployeeID = e.EmployeeID
                        JOIN Contracts c ON d.ContractID = c.ContractID";

                    if (employeeId.HasValue || contractId.HasValue)
                    {
                        query += " WHERE ";
                        if (employeeId.HasValue)
                        {
                            query += "d.EmployeeID = @employeeId";
                        }
                        if (contractId.HasValue)
                        {
                            if (employeeId.HasValue)
                                query += " AND ";
                            query += "d.ContractID = @contractId";
                        }
                    }

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        if (employeeId.HasValue)
                            cmd.Parameters.AddWithValue("@employeeId", employeeId.Value);

                        if (contractId.HasValue)
                            cmd.Parameters.AddWithValue("@contractId", contractId.Value);

                        var table = new DataTable();
                        var adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(table);
                        dgvDutySchedule.DataSource = table;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки графика дежурств: " + ex.Message);
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var form = new AddEditDutyScheduleForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvDutySchedule.SelectedRows.Count > 0)
            {
                int scheduleId = Convert.ToInt32(dgvDutySchedule.SelectedRows[0].Cells["SheduleID"].Value);
                var form = new AddEditDutyScheduleForm(scheduleId);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvDutySchedule.SelectedRows.Count > 0)
            {
                int scheduleId = Convert.ToInt32(dgvDutySchedule.SelectedRows[0].Cells["SheduleID"].Value);

                if (MessageBox.Show("Вы уверены, что хотите удалить запись?", "Удаление", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    DeleteSchedule(scheduleId);
                    LoadData();
                }
            }
        }

        private void DeleteSchedule(int id)
        {
            using (var conn = Database.GetConnection())
            {
                try
                {
                    conn.Open();
                    var cmd = new SqlCommand("DELETE FROM DutyShedule WHERE SheduleID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка удаления записи: " + ex.Message);
                }
            }
        }

        private void cmbEmployeeFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = cmbEmployeeFilter.SelectedItem as EmployeeItem;
            int? employeeId = item?.ID;
            var contractItem = cmbContractFilter.SelectedItem as ContractItem;
            int? contractId = contractItem?.ContractID;

            LoadData(employeeId, contractId);
        }

        private void cmbContractFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = cmbContractFilter.SelectedItem as ContractItem;
            int? contractId = item?.ContractID;
            var employeeItem = cmbEmployeeFilter.SelectedItem as EmployeeItem;
            int? employeeId = employeeItem?.ID;

            LoadData(employeeId, contractId);
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