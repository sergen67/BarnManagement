using BarnManagement.Models.Entities;
using BarnManagement.WinForms.Models;
using BarnManagement.WinForms.Models.Entities;
using System;
using System.Linq;
using System.Windows.Forms;

namespace BarnManagement.Forms
{
    public partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();
        }
        public string RegisteredUsername { get; private set; }
        private void btnRegister_Click(object sender, EventArgs e)
        {
            try
            {
                var username = txtUsername.Text;
                var password = txtPassword.Text;

                using (var db = new BarnContext())
                {
                    if (!db.Barns.Any())
                    {
                        db.Barns.Add(new Barn
                        {
                            TotalCapacity = 100,
                            CurrentAnimalCount = 0,
                            CreatedAt = DateTime.Now
                        });
                        db.SaveChanges();
                    }
                    if (db.Users.Any(u => u.Username == username))
                    {
                        MessageBox.Show("Bu kullanıcı adı zaten alınmış.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var salt = WinForms.Utils.Crypto.NewSalt();
                    var hash = WinForms.Utils.Crypto.HashPassword(password, salt);

                    db.Users.Add(new User
                    {
                        Username = username,
                        PasswordHash = hash,
                        PasswordSalt = salt,
                        Role = "User", // Default role
                        CreatedAt = DateTime.Now
                    });
                    db.SaveChanges();
                }
                RegisteredUsername = username;
                this.DialogResult = DialogResult.OK;
                this.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kayıt sırasında bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
