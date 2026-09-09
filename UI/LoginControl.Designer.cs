namespace moju.UI
{
    partial class LoginControl
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.button2 = new System.Windows.Forms.Button();
            this.login = new System.Windows.Forms.Button();
            this.title = new System.Windows.Forms.Label();
            this.PasswordInput = new System.Windows.Forms.TextBox();
            this.LoginNameInput = new System.Windows.Forms.TextBox();
            this.Password = new System.Windows.Forms.Label();
            this.LoginName = new System.Windows.Forms.Label();
            this.ErrorMessage = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ErrorMessage);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.login);
            this.panel1.Controls.Add(this.title);
            this.panel1.Controls.Add(this.PasswordInput);
            this.panel1.Controls.Add(this.LoginNameInput);
            this.panel1.Controls.Add(this.Password);
            this.panel1.Controls.Add(this.LoginName);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(318, 251);
            this.panel1.TabIndex = 0;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(205, 200);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 30);
            this.button2.TabIndex = 6;
            this.button2.Text = "退出";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.ExitButton_Click);
            // 
            // login
            // 
            this.login.Location = new System.Drawing.Point(71, 200);
            this.login.Name = "login";
            this.login.Size = new System.Drawing.Size(75, 30);
            this.login.TabIndex = 5;
            this.login.Text = "登录";
            this.login.UseVisualStyleBackColor = true;
            this.login.Click += new System.EventHandler(this.LoginButton_Click);
            // 
            // title
            // 
            this.title.AutoSize = true;
            this.title.Font = new System.Drawing.Font("微软雅黑", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.title.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.title.Location = new System.Drawing.Point(96, 17);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(125, 46);
            this.title.TabIndex = 4;
            this.title.Text = "请登录";
            this.title.Click += new System.EventHandler(this.Title_Click);
            // 
            // PasswordInput
            // 
            this.PasswordInput.Location = new System.Drawing.Point(71, 147);
            this.PasswordInput.Name = "PasswordInput";
            this.PasswordInput.PasswordChar = '*';
            this.PasswordInput.Size = new System.Drawing.Size(209, 21);
            this.PasswordInput.TabIndex = 3;
            this.PasswordInput.Click += new System.EventHandler(this.PasswordInput_Click);
            // 
            // LoginNameInput
            // 
            this.LoginNameInput.Location = new System.Drawing.Point(71, 89);
            this.LoginNameInput.Name = "LoginNameInput";
            this.LoginNameInput.Size = new System.Drawing.Size(209, 21);
            this.LoginNameInput.TabIndex = 2;
            this.LoginNameInput.Click += new System.EventHandler(this.LoginNameInput_Click);
            // 
            // Password
            // 
            this.Password.AutoSize = true;
            this.Password.Location = new System.Drawing.Point(30, 150);
            this.Password.Name = "Password";
            this.Password.Size = new System.Drawing.Size(35, 12);
            this.Password.TabIndex = 1;
            this.Password.Text = "密码:";
            this.Password.Click += new System.EventHandler(this.PasswordLabel_Click);
            // 
            // LoginName
            // 
            this.LoginName.AutoSize = true;
            this.LoginName.Location = new System.Drawing.Point(18, 92);
            this.LoginName.Name = "LoginName";
            this.LoginName.Size = new System.Drawing.Size(47, 12);
            this.LoginName.TabIndex = 0;
            this.LoginName.Text = "用户名:";
            this.LoginName.Click += new System.EventHandler(this.LoginNameLabel_click);
            // 
            // ErrorMessage
            // 
            this.ErrorMessage.AutoSize = true;
            this.ErrorMessage.Location = new System.Drawing.Point(69, 171);
            this.ErrorMessage.Name = "ErrorMessage";
            this.ErrorMessage.Size = new System.Drawing.Size(0, 12);
            this.ErrorMessage.TabIndex = 7;
            // 
            // LoginControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "LoginControl";
            this.Size = new System.Drawing.Size(318, 250);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label LoginName;
        private System.Windows.Forms.Label Password;
        private System.Windows.Forms.TextBox LoginNameInput;
        private System.Windows.Forms.Label title;
        private System.Windows.Forms.TextBox PasswordInput;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button login;
        private System.Windows.Forms.Label ErrorMessage;
    }
}
