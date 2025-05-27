using System;
using System.Windows.Forms;

namespace SecurityAgencyApp
{
    public class UsersForm : Form
    {
        public UsersForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Пользователи системы";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Здесь можно добавить элементы: DataGridView, кнопки добавления/редактирования и т.д.
        }
    }
}