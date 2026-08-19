using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace _24_59277_3_LoginSystem
{
    /// <summary>
    /// Every SqlConnection / SqlCommand in the whole application lives here.
    /// The forms only ever call these methods - they never open a
    /// SqlConnection themselves. This is Bonus Task: "Move all database code
    /// out of the forms into a DatabaseHelper class".
    ///
    /// The connection string is read once from App.config via
    /// ConfigurationManager, so there is no hard-coded connection string
    /// anywhere in this project.
    /// </summary>
    internal static class DatabaseHelper
    {
        private static string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["LoginDbConnection"].ConnectionString; }
        }

        /// <summary>
        /// Opens and immediately closes a connection just to prove the
        /// database is reachable. Called from LoginForm_Load so a bad
        /// connection string shows a friendly message instead of the app
        /// crashing the first time a query runs.
        /// </summary>
        public static bool TestConnection(out string errorMessage)
        {
            errorMessage = null;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                }
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public static bool UsernameExists(string username)
        {
            const string sql = "SELECT COUNT(*) FROM dbo.Users WHERE Username = @Username";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                con.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        /// <summary>
        /// Inserts a new user with a parameterized ExecuteNonQuery(). The
        /// password argument here must already be a SHA-256 hash - see
        /// PasswordHelper.Hash - this method never sees the real password.
        /// </summary>
        public static void RegisterUser(string username, string passwordHash, string email, string fullName)
        {
            const string sql =
                "INSERT INTO dbo.Users (Username, PasswordHash, Email, FullName) " +
                "VALUES (@Username, @PasswordHash, @Email, @FullName)";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                cmd.Parameters.AddWithValue("@Email", (object)email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FullName", (object)fullName ?? DBNull.Value);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Looks the user up with a parameterized query and returns their
        /// stored hash + full name + UserID so the caller can compare
        /// hashes itself. Returns null if the username does not exist.
        /// This is deliberately the ONLY place a raw password-related value
        /// is read from the database, and it is always a hash, never plain
        /// text.
        /// </summary>
        public static UserRecord FindUserByUsername(string username)
        {
            const string sql =
                "SELECT UserID, Username, PasswordHash, FullName, Email " +
                "FROM dbo.Users WHERE Username = @Username";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new UserRecord
                        {
                            UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                            Username = reader.GetString(reader.GetOrdinal("Username")),
                            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                            FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? "" : reader.GetString(reader.GetOrdinal("FullName")),
                            Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? "" : reader.GetString(reader.GetOrdinal("Email"))
                        };
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// Fills a DataTable for the DataGridView on HomeForm. Deliberately
        /// selects only non-secret columns - the password hash column is
        /// never pulled into the grid.
        /// </summary>
        public static DataTable GetAllUsers()
        {
            const string sql = "SELECT UserID, Username, Email, CreatedAt FROM dbo.Users ORDER BY UserID";

            DataTable table = new DataTable();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                adapter.Fill(table);
            }
            return table;
        }

        /// <summary>
        /// Bonus: Search/filter the grid by username, still parameterized
        /// (LIKE @term), so a search box cannot be used for injection either.
        /// </summary>
        public static DataTable SearchUsersByUsername(string searchTerm)
        {
            const string sql =
                "SELECT UserID, Username, Email, CreatedAt FROM dbo.Users " +
                "WHERE Username LIKE @Term ORDER BY UserID";

            DataTable table = new DataTable();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("@Term", "%" + searchTerm + "%");
                adapter.Fill(table);
            }
            return table;
        }

        // ---------- Bonus: LoginHistory ----------

        /// <summary>
        /// Writes a LoginHistory row when a user logs in and returns the new
        /// LoginHistoryID so LogoutTime can be stamped on the same row later.
        /// </summary>
        public static int RecordLogin(int userId)
        {
            const string sql =
                "INSERT INTO dbo.LoginHistory (UserID, LoginTime) OUTPUT INSERTED.LoginHistoryID " +
                "VALUES (@UserID, GETDATE())";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                object result = cmd.ExecuteScalar();
                return Convert.ToInt32(result);
            }
        }

        public static void RecordLogout(int loginHistoryId)
        {
            const string sql =
                "UPDATE dbo.LoginHistory SET LogoutTime = GETDATE() WHERE LoginHistoryID = @LoginHistoryID";

            using (SqlConnection con = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@LoginHistoryID", loginHistoryId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }

    /// <summary>Small data-carrier returned by FindUserByUsername.</summary>
    internal class UserRecord
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }
}
