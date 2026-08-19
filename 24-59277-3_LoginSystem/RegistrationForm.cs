using System;
using System.Windows.Forms;

namespace _24_59277_3_LoginSystem
{
    public partial class RegistrationForm : Form
    {
        public RegistrationForm()
        {
            InitializeComponent();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;
            string email = txtEmail.Text.Trim();
            string fullName = txtFullName.Text.Trim();

            string validationError = Validate(username, password, confirmPassword, email, fullName);
            if (validationError != null)
            {
                lblStatus.Text = validationError;
                return;
            }

            try
            {
                // TASK 3: check for a duplicate username BEFORE inserting.
                if (DatabaseHelper.UsernameExists(username))
                {
                    lblStatus.Text = "Username already taken";
                    txtUsername.Focus();
                    return;
                }

                // Hash first - the real password is never written to SQL.
                string passwordHash = PasswordHelper.Hash(password);

                // Parameterized INSERT (see DatabaseHelper.RegisterUser).
                DatabaseHelper.RegisterUser(username, passwordHash, email, fullName);

                MessageBox.Show("Registration successful! You can now log in.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearForm();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                // Covers e.g. a UNIQUE KEY violation that slips through a
                // race between the exists-check and the insert.
                if (ex.Message.IndexOf("UNIQUE KEY", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    lblStatus.Text = "Username already taken";
                }
                else
                {
                    MessageBox.Show("A database error occurred:\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string Validate(string username, string password, string confirmPassword, string email, string fullName)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(fullName))
            {
                return "All fields are required.";
            }

            if (password.Length < 6)
            {
                return "Password must be at least 6 characters.";
            }

            if (password != confirmPassword)
            {
                return "Passwords do not match.";
            }

            if (!email.Contains("@"))
            {
                return "Enter a valid email address.";
            }

            return null;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            ClearForm();
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ClearForm()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            txtConfirmPassword.Clear();
            txtEmail.Clear();
            txtFullName.Clear();
            lblStatus.Text = "";
        }
    }
}
