using System;
using System.Reflection;
using System.Windows.Forms;

namespace SecurityAgencyApp
{
    public partial class MainForm : Form
    {
        private string[] formNames = new[]
        {
            "ClientsForm",
            "EmployeesForm",
            "ContractsForm",
            "DutyScheduleForm",
            "PaymentsForm",
            "OrdersForm",
            "UsersForm",
            "ReportsForm"
        };

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Охранное агентство 'Security'";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            var menuStrip = new MenuStrip();

            foreach (var name in formNames)
            {
                var menuItem = new ToolStripMenuItem(name.Replace("Form", ""));
                menuItem.Click += (s, e) => OpenForm(name);
                menuStrip.Items.Add(menuItem);
            }

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private void OpenForm(string formName)
        {
            try
            {
                string fullFormName = $"SecurityAgencyApp.{formName}";

                Type formType = Type.GetType(fullFormName);

                if (formType != null && typeof(Form).IsAssignableFrom(formType))
                {
                    var formInstance = (Form)Activator.CreateInstance(formType);
                    formInstance.Show();
                }
                else
                {
                    MessageBox.Show($"Форма '{formName}' не найдена или не является Windows Forms формой.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка открытия формы: " + ex.Message);
            }
        }
    }
}