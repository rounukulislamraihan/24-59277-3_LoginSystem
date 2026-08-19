# 24-59277-3_LoginSystem

A WinForms (C#, .NET Framework 4.7.2) Login / Registration / Logout application backed by SQL Server, built for Lab 1.

> **Before you submit:** fill in the `[bracketed]` placeholders below and add the screenshots listed at the bottom.

**A note on naming:** my student ID (`24-59277-3`) starts with a digit and contains hyphens, which the project name, solution/database name, and GitHub repo name all tolerate fine as plain text - but two places in this codebase legally can't use it as-is:
- **C# namespace** - C# identifiers can't start with a digit or contain a hyphen, so every `.cs` file uses `namespace _24_59277_3_LoginSystem` instead (leading underscore, hyphens → underscores). `AssemblyName` in the `.csproj` is unaffected since it's just an output filename, not a code identifier, so it stays `24-59277-3_LoginSystem`.
- **SQL database name** - `CREATE DATABASE`/`USE` in `Schema.sql` wrap the name as `[24-59277-3_LoginDB]`. Square brackets mark it as a *delimited* identifier, which SQL Server needs here since a leading digit and hyphens aren't legal in a plain unquoted identifier. The `Initial Catalog=24-59277-3_LoginDB` value in `App.config` does NOT need brackets, since that's just a string inside a connection string, not T-SQL being parsed.

## Environment

- SQL Server: `[e.g. SQL Server 2022 Express / Developer edition]`
- Visual Studio: `[e.g. Visual Studio 2022 17.9]`
- .NET version: .NET Framework 4.7.2
- Connection string format (no real password/credentials, since Integrated Security is used):
  ```
  Data Source=.\SQLEXPRESS;Initial Catalog=24-59277-3_LoginDB;Integrated Security=True;Connect Timeout=30
  ```

## Database setup

`Schema.sql` (included in this repo, also submitted separately) creates the `24-59277-3_LoginDB` database and two tables:

- **dbo.Users** — `UserID` (PK, identity), `Username` (unique), `PasswordHash`, `Email`, `FullName`, `CreatedAt`.
- **dbo.LoginHistory** (bonus) — `LoginHistoryID` (PK, identity), `UserID` (FK → Users), `LoginTime`, `LogoutTime`.

I ran `Schema.sql` in `[SSMS / Visual Studio SQL Server Object Explorer]` against my local instance, then pointed `App.config`'s `LoginDbConnection` at the resulting database.

## How registration, login, and logout work

All ADO.NET code lives in **`DatabaseHelper.cs`** — the forms never open a `SqlConnection` themselves, they only call static helper methods. The connection string is read once via `ConfigurationManager.ConnectionStrings["LoginDbConnection"]`, so it only exists in one place: `App.config`.

- **Registration** (`RegistrationForm.cs`) — validates that no field is empty, the password is ≥ 6 characters, both password fields match, and the email contains `@`. It then calls `DatabaseHelper.UsernameExists()` (a parameterized `SELECT COUNT(*) ... ExecuteScalar()`) to reject duplicate usernames before ever attempting an insert. The password is hashed with `PasswordHelper.Hash()` and only the hash is passed to `DatabaseHelper.RegisterUser()`, which inserts it with a parameterized `ExecuteNonQuery()`. On success it shows a confirmation, clears the form, and closes back to the login form.

- **Login** (`LoginForm.cs`) — calls `DatabaseHelper.FindUserByUsername()`, a parameterized query executed with a `SqlDataReader`, which returns the stored hash (never a plain-text password). `PasswordHelper.Verify()` re-hashes the entered password and compares it to the stored hash. On success it opens `HomeForm` with `"Welcome, {FullName}"` and hides the login form; on failure it shows a message and, after 3 consecutive failed attempts, disables the Login button for the rest of that session.

- **Logout** (`HomeForm.cs`) — `btnLogout_Click` closes `HomeForm` only. `LoginForm` subscribes to `HomeForm.FormClosed`, and in that handler clears its own textboxes, re-shows itself, and focuses the username box. Because `Application.Run(new LoginForm())` in `Program.cs` only exits when `LoginForm` itself closes, logging out never exits the application and never leaves an orphan `HomeForm` running.

## Password hashing

`PasswordHelper.Hash()` runs the password through `SHA256.Create()` and stores the result as a lowercase hex string. `PasswordHelper.Verify()` hashes the candidate password the same way and compares strings — the plain-text password itself is never sent to, or stored in, the database. Storing plain text is unacceptable because anyone with read access to the table (a DBA, a backup, a breach) would see every user's real password immediately, and because many people reuse passwords across sites, a single leaked table can compromise accounts elsewhere too. Hashing means even someone with full read access to `Users` only sees irreversible digests.

## SQL injection demo (Task 6)

- **Vulnerable code:** built the login query by string concatenation, e.g.
  `"SELECT * FROM Users WHERE Username='" + user + "' AND Password='" + pass + "'"`.
- **Exploit input:** entering `' OR '1'='1` into the password field turned the query into
  `...WHERE Username='x' AND Password='' OR '1'='1'`, which is always true, so the query returned a row and logged in with no real password.
- **Fixed code:** `DatabaseHelper.FindUserByUsername()` uses a parameterized query — `WHERE Username = @Username` with `cmd.Parameters.AddWithValue("@Username", username)` — and the password is never even part of the SQL text; it's compared as a hash in C# after the row comes back.
- **Why parameters stop it:** with parameters, the value travels to SQL Server separately from the SQL text and is only ever treated as *data*, never parsed as part of the command — so `' OR '1'='1` just becomes a literal (and wrong) password string instead of altering the query's logic.
- Screenshots: `[before-injection.png]`, `[after-fix.png]`.

## Bonus tasks attempted

1. **DatabaseHelper class** — all `SqlConnection`/`SqlCommand` code moved out of every form and into `DatabaseHelper.cs`; forms only call its static methods.
2. **LoginHistory table** — `dbo.LoginHistory` has a foreign key to `Users`. `DatabaseHelper.RecordLogin()` inserts a row (via `OUTPUT INSERTED.LoginHistoryID`) when login succeeds; `DatabaseHelper.RecordLogout()` stamps `LogoutTime` on that same row when the user logs out.
3. *(Optional third bonus if attempted, e.g. search/filter by username via `LIKE @term` in `HomeForm`'s search box — already included in the base build.)*

## Bugs found in the sample project (for the viva)

1. SQL injection via string-concatenated queries instead of parameters.
2. Two conflicting connection strings in the same form (`Initial Catalog=dbEmployeeDetails` vs `=Login`).
3. Code queries `LoginMst`, but the shipped script creates a table called `Table` — mismatch means it can't run as-is.
4. `Form1_Load` opens a connection it never uses and never closes.
5. No try/catch anywhere, so an unreachable SQL Server crashes the app.
6. `con.Close()` sits outside any `finally`/`using`, so it's skipped whenever an exception is thrown above it.
7. Passwords are stored and compared as plain text.
8. A missing space in concatenated SQL produces `'x'and` (malformed SQL).
9. A successful login opens a website instead of the app's own home screen.
10. Default control names (`button1`, `textBox1`, `label3`) instead of meaningful ones.
11. No registration form, and no logout at all, despite the project being called "Login Logout".

## Screenshots
<img width="1883" height="998" alt="Screenshot 2026-08-19 185816" src="https://github.com/user-attachments/assets/fb64fc6d-e740-4612-8dfd-7095d3ccbfbb" />
<img width="1883" height="1004" alt="Screenshot 2026-08-19 185736" src="https://github.com/user-attachments/assets/3f1cd8c3-ee19-4b01-aef0-578dbc648884" />
<img width="1876" height="1005" alt="Screenshot 2026-08-19 185557" src="https://github.com/user-attachments/assets/f1aa484b-6ab5-4906-857b-3a714832ed95" />
<img width="1903" height="1021" alt="Screenshot 2026-08-19 185338" src="https://github.com/user-attachments/assets/512f0919-3d4e-4c2c-9c06-645086813fbd" />


## Problems hit and how I solved them
1. SQL Connection Error
   Fixed by configuring connection string in app.config and using Windows Authentication with localhost.
2. Missing Database Table
   Added automatic table creation check in Form_Load to create Users table if it doesn't exist.
3. Login Failed Despite Correct Credentials
   Applied same password hashing algorithm during login that was used during registration.
4. App Didn't Close on Logout
   Used Application.Exit() and Environment.Exit(0) instead of just this.Hide().
5. Empty Fields Crashed the App
   Added string.IsNullOrEmpty() validation with warning messages before processing.
6. Duplicate Username Registration
   Added UNIQUE constraint in database and checked existence before inserting.
7. Bin/Obj Folders Uploaded to GitHub
   Added .gitignore file to exclude unnecessary build folders.
