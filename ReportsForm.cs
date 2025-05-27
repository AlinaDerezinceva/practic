using System;
using System.Data;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace SecurityAgencyApp
{
    public partial class ReportsForm : Form
    {
        private DataGridView dgvReport;
        private DateTimePicker dtpFrom, dtpTo;
        private Button btnGenerate;

        public ReportsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Финансовый отчёт";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(10),
                AutoSize = true
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            layout.Controls.Add(new Label { Text = "С:" }, 0, 0);
            dtpFrom = new DateTimePicker();
            layout.Controls.Add(dtpFrom, 1, 0);

            layout.Controls.Add(new Label { Text = "По:" }, 2, 0);
            dtpTo = new DateTimePicker();
            layout.Controls.Add(dtpTo, 3, 0);

            btnGenerate = new Button { Text = "Сформировать отчёт" };
            btnGenerate.Click += BtnGenerate_Click;
            layout.Controls.Add(btnGenerate, 0, 1);

            dgvReport = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = true
            };
            layout.Controls.Add(dgvReport, 0, 2);       
            layout.SetColumnSpan(dgvReport, 4);          
            layout.SetRowSpan(dgvReport, 1);             

            this.Controls.Add(layout);
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            var from = dtpFrom.Value.Date;
            var to = dtpTo.Value.Date.AddDays(1).AddSeconds(-1); 

            if (from > to)
            {
                MessageBox.Show("Дата 'С' не может быть позже даты 'По'");
                return;
            }

            var reportData = GenerateFinancialReport(from, to);
            dgvReport.DataSource = reportData;
        }

        private DataTable GenerateFinancialReport(DateTime from, DateTime to)
        {
            var reportData = new DataTable();
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(@"
                    SELECT 
                        c.ContractID, 
                        ISNULL(l.CompanyName, p.Surname + ' ' + p.Name) AS ClientName,
                        SUM(pmt.Amount) AS TotalPaid
                    FROM Payments pmt
                    JOIN Contracts c ON pmt.ContractID = c.ContractID
                    LEFT JOIN LegalClients l ON c.LegalClientID = l.LegalClientID
                    LEFT JOIN PrivateClients p ON c.PrivateClientID = p.PrivateClientID
                    WHERE pmt.PaymentDate BETWEEN @from AND @to
                    GROUP BY c.ContractID, l.CompanyName, p.Surname, p.Name", conn))
                {
                    cmd.Parameters.AddWithValue("@from", from);
                    cmd.Parameters.AddWithValue("@to", to);
                    var adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(reportData);
                }
            }

            return reportData;
        }
    }
}