using System;
using System.Windows.Forms;

namespace _24_59277_3_LoginSystem
{
    public partial class HomeForm : Form
    {
        private readonly int loginHistoryId;

        public HomeForm(string fullName, int loginHistoryId)
        {
            InitializeComponent();
            this.loginHistoryId = loginHistoryId;
            lblWelcome.Text = "Welcome, " + fullName;
        }

        private void HomeForm_Load(object sender, EventArgs e)
        {
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                // TASK 7: DataGridView via SqlDataAdapter + DataTable.
                // GetAllUsers() deliberately never selects the password
                // hash column, so it can never end up in the grid.
                dgvUsers.DataSource = DatabaseHelper.GetAllUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load users:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string term = txtSearch.Text.Trim();
                dgvUsers.DataSource = string.IsNullOrEmpty(term)
                    ? DatabaseHelper.GetAllUsers()
                    : DatabaseHelper.SearchUsersByUsername(term);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search failed:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            LoadUsers();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            try
            {
                if (loginHistoryId > 0)
                {
                    // Bonus: stamp LogoutTime on the LoginHistory row.
                    DatabaseHelper.RecordLogout(loginHistoryId);
                }
            }
            catch
            {
                // Logging the logout time is a bonus feature - it should
                // never block the user from actually logging out.
            }

            // TASK 5: close HomeForm only. LoginForm subscribes to
            // FormClosed to clear itself and re-show, so the app keeps
            // running and no orphan HomeForm is left behind.
            this.Close();
        }
    }
}
