using System;
using System.Windows.Forms;

namespace _24_59277_3_LoginSystem
{
    public partial class LoginForm : Form
    {
        private int failedAttempts = 0;
        private const int MaxFailedAttempts = 3;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            ClearForm();

            // TASK 2: prove the DB is reachable before the user tries to do
            // anything, and fail with a message box instead of crashing.
            string errorMessage;
            if (!DatabaseHelper.TestConnection(out errorMessage))
            {
                MessageBox.Show(
                    "Could not connect to the database.\n\n" + errorMessage +
                    "\n\nCheck the connection string in App.config.",
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblStatus.Text = "Enter both username and password.";
                return;
            }

            UserRecord user;
            try
            {
                // TASK 4: parameterized lookup (no string concatenation) -
                // see DatabaseHelper.FindUserByUsername.
                user = DatabaseHelper.FindUserByUsername(username);
            }
            catch (Exception ex)
            {
                MessageBox.Show("A database error occurred:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Compare hashes, never plain text, and never reveal whether it
            // was the username or the password that was wrong.
            bool loginOk = user != null && PasswordHelper.Verify(password, user.PasswordHash);

            if (loginOk)
            {
                failedAttempts = 0;

                int loginHistoryId = 0;
                try
                {
                    // Bonus: LoginHistory
                    loginHistoryId = DatabaseHelper.RecordLogin(user.UserID);
                }
                catch
                {
                    // A missing LoginHistory table should not block a
                    // successful login - it's a bonus feature, not core.
                }

                HomeForm home = new HomeForm(user.FullName, loginHistoryId);
                home.FormClosed += (s, args) =>
                {
                    // TASK 5: logout returns to a CLEARED login form, the
                    // app itself does not exit.
                    ClearForm();
                    this.Show();
                    txtUsername.Focus();
                };

                this.Hide();
                home.Show();
            }
            else
            {
                failedAttempts++;
                int remaining = MaxFailedAttempts - failedAttempts;

                if (remaining > 0)
                {
                    MessageBox.Show("Invalid username or password. " + remaining + " attempt(s) remaining.",
                        "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    lblStatus.Text = "Invalid username or password.";
                }
                else
                {
                    btnLogin.Enabled = false;
                    lblStatus.Text = "Too many failed attempts. Login disabled.";
                    MessageBox.Show("Too many failed attempts. The Login button has been disabled.",
                        "Account Locked (this session)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                txtPassword.Clear();
                txtPassword.Focus();
            }
        }

        private void btnGoToRegister_Click(object sender, EventArgs e)
        {
            using (RegistrationForm registrationForm = new RegistrationForm())
            {
                registrationForm.ShowDialog(this);
            }
            // In case the user just registered, clear the form so they
            // type their new credentials fresh instead of re-using
            // whatever was left in the boxes.
            ClearForm();
        }

        private void ClearForm()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            lblStatus.Text = "";
            failedAttempts = 0;
            btnLogin.Enabled = true;
            txtUsername.Focus();
        }
    }
}
