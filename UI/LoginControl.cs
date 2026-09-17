using moju.service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace moju.UI
{
    public partial class LoginControl : UserControl
    {
        public LoginControl()
        {
            InitializeComponent();
            ErrorMessage.Hide();
        }

        public event EventHandler LoginSuccess;

        private void OnLoginSuccess()
        {
            LoginSuccess.Invoke(this, EventArgs.Empty);
        }

        private void Title_Click(object sender, EventArgs e)
        {

        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            string username = LoginNameInput.Text;
            string password = PasswordInput.Text;
            bool result = await UserService.Instance.login(username, password);
            if (result)
            {
                OnLoginSuccess();
            }
            else
            {
                ErrorMessage.Text = "用户名或密码错误，请重试";
                ErrorMessage.ForeColor = Color.Red;
                ErrorMessage.Show();
            }
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void LoginNameInput_Click(object sender, EventArgs e)
        {

        }

        private void PasswordInput_Click(object sender, EventArgs e)
        {

        }

        private void LoginNameLabel_click(object sender, EventArgs e)
        {

        }

        private void PasswordLabel_Click(object sender, EventArgs e)
        {

        }

    }

}
