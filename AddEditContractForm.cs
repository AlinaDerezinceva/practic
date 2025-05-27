using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace SecurityAgencyApp
{
    public class AddEditContractForm : Form
    {
        private readonly int? _contractId = null;

        private TextBox txtContractNumber;
        private DateTimePicker dtpContractDate, dtpEventStart, dtpEventEnd;
        private ComboBox cmbLegalClient, cmbPrivateClient, cmbEventType;
        private TextBox txtEventAddress, txtParticipantsCount, txtTotalAmount;

        public AddEditContractForm(int contractId = -1)
        {
            _contractId = contractId == -1 ? null : (int?)contractId;
            InitializeComponent();
            LoadClientsAndEventTypes();

            if (_contractId.HasValue)
            {
                LoadData();
                this.Text = "Редактировать договор";
            }
            else
            {
                this.Text = "Новый договор";
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Договор";
            this.Size = new System.Drawing.Size(500, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 9,
                AutoSize = true
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Номер договора
            layout.Controls.Add(new Label { Text = "Номер договора" }, 0, 0);
            txtContractNumber = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtContractNumber, 1, 0);

            // Дата договора
            layout.Controls.Add(new Label { Text = "Дата договора" }, 0, 1);
            dtpContractDate = new DateTimePicker();
            layout.Controls.Add(dtpContractDate, 1, 1);

            // Клиенты
            layout.Controls.Add(new Label { Text = "Юридическое лицо" }, 0, 2);
            cmbLegalClient = new ComboBox { Dock = DockStyle.Fill };
            layout.Controls.Add(cmbLegalClient, 1, 2);

            layout.Controls.Add(new Label { Text = "Физическое лицо" }, 0, 3);
            cmbPrivateClient = new ComboBox { Dock = DockStyle.Fill };
            layout.Controls.Add(cmbPrivateClient, 1, 3);

            // Тип мероприятия
            layout.Controls.Add(new Label { Text = "Тип мероприятия" }, 0, 4);
            cmbEventType = new ComboBox { Dock = DockStyle.Fill };
            layout.Controls.Add(cmbEventType, 1, 4);

            // Адрес мероприятия
            layout.Controls.Add(new Label { Text = "Адрес мероприятия" }, 0, 5);
            txtEventAddress = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtEventAddress, 1, 5);

            // Время начала
            layout.Controls.Add(new Label { Text = "Дата и время начала" }, 0, 6);
            dtpEventStart = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm" };
            layout.Controls.Add(dtpEventStart, 1, 6);

            // Время окончания
            layout.Controls.Add(new Label { Text = "Дата и время окончания" }, 0, 7);
            dtpEventEnd = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm" };
            layout.Controls.Add(dtpEventEnd, 1, 7);

            // Участники
            layout.Controls.Add(new Label { Text = "Количество участников" }, 0, 8);
            txtParticipantsCount = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtParticipantsCount, 1, 8);

            // Сумма
            layout.Controls.Add(new Label { Text = "Общая сумма" }, 0, 9);
            txtTotalAmount = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtTotalAmount, 1, 9);

            // Кнопка сохранения
            var btnSave = new Button { Text = "Сохранить", Width = 100, Anchor = AnchorStyles.Right };
            btnSave.Click += BtnSave_Click;

            var panel = new Panel { Dock = DockStyle.Bottom };
            panel.Controls.Add(btnSave);

            this.Controls.Add(layout);
            this.Controls.Add(panel);
        }

        private void LoadClientsAndEventTypes()
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();

                // Загрузка юр.лиц
                var legalCmd = new SqlCommand("SELECT LegalClientID, CompanyName FROM LegalClients", conn);
                var legalReader = legalCmd.ExecuteReader();
                while (legalReader.Read())
                {
                    cmbLegalClient.Items.Add(new ClientItem
                    {
                        ID = Convert.ToInt32(legalReader["LegalClientID"]),
                        Name = legalReader["CompanyName"].ToString(),
                        IsLegal = true
                    });
                }
                legalReader.Close();

                // Загрузка физ.лиц
                var privateCmd = new SqlCommand("SELECT PrivateClientID, Surname + ' ' + Name AS FullName FROM PrivateClients", conn);
                var privateReader = privateCmd.ExecuteReader();
                while (privateReader.Read())
                {
                    cmbPrivateClient.Items.Add(new ClientItem
                    {
                        ID = Convert.ToInt32(privateReader["PrivateClientID"]),
                        Name = privateReader["FullName"].ToString(),
                        IsLegal = false
                    });
                }
                privateReader.Close();

                // Типы мероприятий
                var eventTypeCmd = new SqlCommand("SELECT EventTypeID, TypeName FROM EventTypes", conn);
                var eventTypeReader = eventTypeCmd.ExecuteReader();
                while (eventTypeReader.Read())
                {
                    cmbEventType.Items.Add(new EventTypeItem
                    {
                        EventTypeID = Convert.ToInt32(eventTypeReader["EventTypeID"]),
                        TypeName = eventTypeReader["TypeName"].ToString()
                    });
                }
            }
        }

        private void LoadData()
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();

                var cmd = new SqlCommand("SELECT * FROM Contracts WHERE ContractID = @id", conn);
                cmd.Parameters.AddWithValue("@id", _contractId.Value);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    txtContractNumber.Text = reader["ContracNumber"].ToString();
                    dtpContractDate.Value = Convert.ToDateTime(reader["ContractData"]);

                    txtEventAddress.Text = reader["EventAddress"].ToString();
                    dtpEventStart.Value = Convert.ToDateTime(reader["EventStart"]);
                    dtpEventEnd.Value = Convert.ToDateTime(reader["EventEnd"]);
                    txtParticipantsCount.Text = reader["ParticipantsCount"].ToString();
                    txtTotalAmount.Text = reader["TotalAmount"].ToString();
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtContractNumber.Text))
            {
                MessageBox.Show("Введите номер договора");
                return;
            }

            using (var conn = Database.GetConnection())
            {
                conn.Open();

                if (_contractId.HasValue)
                {
                    var cmd = new SqlCommand(@"
                        UPDATE Contracts SET 
                            ContracNumber = @number, ContractData = @date, EndData = @endDate,
                            LegalClientID = @legalClientId, PrivateClientID = @privateClientId,
                            EventTypeID = @eventType, EventAddress = @eventAddress,
                            EventStart = @eventStart, EventEnd = @eventEnd,
                            ParticipantsCount = @participants, TotalAmount = @amount
                        WHERE ContractID = @id", conn);

                    cmd.Parameters.AddWithValue("@id", _contractId.Value);
                    SetCommonParameters(cmd);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    var cmd = new SqlCommand(@"
                        INSERT INTO Contracts (
                            ContracNumber, ContractData, EndData,
                            LegalClientID, PrivateClientID,
                            EventTypeID, EventAddress,
                            EventStart, EventEnd,
                            ParticipantsCount, TotalAmount
                        ) VALUES (
                            @number, @date, @endDate,
                            @legalClientId, @privateClientId,
                            @eventType, @eventAddress,
                            @eventStart, @eventEnd,
                            @participants, @amount
                        )", conn);

                    SetCommonParameters(cmd);
                    cmd.ExecuteNonQuery();
                }

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void SetCommonParameters(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@number", txtContractNumber.Text);
            cmd.Parameters.AddWithValue("@date", dtpContractDate.Value.Date);
            cmd.Parameters.AddWithValue("@endDate", DBNull.Value); // Если не указано

            var legalClient = cmbLegalClient.SelectedItem as ClientItem;
            var privateClient = cmbPrivateClient.SelectedItem as ClientItem;
            var eventType = cmbEventType.SelectedItem as EventTypeItem;

            cmd.Parameters.AddWithValue("@legalClientId", legalClient?.ID ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@privateClientId", privateClient?.ID ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@eventType", eventType?.EventTypeID ?? (object)DBNull.Value);

            cmd.Parameters.AddWithValue("@eventAddress", txtEventAddress.Text);
            cmd.Parameters.AddWithValue("@eventStart", dtpEventStart.Value);
            cmd.Parameters.AddWithValue("@eventEnd", dtpEventEnd.Value);
            cmd.Parameters.AddWithValue("@participants", Convert.ToInt32(txtParticipantsCount.Text));
            cmd.Parameters.AddWithValue("@amount", Convert.ToDecimal(txtTotalAmount.Text));
        }

        private class ClientItem
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public bool IsLegal { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        private class EventTypeItem
        {
            public int EventTypeID { get; set; }
            public string TypeName { get; set; }

            public override string ToString()
            {
                return TypeName;
            }
        }
    }
}