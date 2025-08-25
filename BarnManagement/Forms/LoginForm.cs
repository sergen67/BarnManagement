using BarnManagement.WinForms.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Windows.Forms;

namespace BarnManagement.Forms
{
    public partial class LoginForm : Form
    {
        private readonly ILogger<LoginForm> _logger;
        public LoginForm()
        {
            InitializeComponent();
            _logger = Program.Services.GetRequiredService<ILogger<LoginForm>>();
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {

        }
        private void linkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var reg = Program.Services.GetRequiredService<RegisterForm>();
            if (reg.ShowDialog(this) == DialogResult.OK)
            {
                txtUsername.Text = reg.RegisteredUsername;
                txtPassword.Focus();
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                var username = txtUsername.Text;
                var password = txtPassword.Text;

                using (var db = new BarnContext())
                {
                    var user = db.Users
                        .FirstOrDefault(u => u.Username == username);
                    if (user == null)
                    {
                        MessageBox.Show("Kullanıcı bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    var hash = WinForms.Utils.Crypto.HashPassword(password, user.PasswordSalt);
                    if (!hash.SequenceEqual(user.PasswordHash))
                    {
                        MessageBox.Show("Parola yanlış.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        // Giriş başarılı, kullanıcıyı yönlendir
                        _logger.LogInformation($"Kullanıcı {username} giriş yaptı.");
                    }

                    var main = Program.Services.GetRequiredService<MainForm>();
                    main.Initialize(user);
                    Hide();
                    main.Show();

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Giriş sırasında hata oluştu.");
                MessageBox.Show("Giriş sırasında bir hata oluştu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
