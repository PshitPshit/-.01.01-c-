using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using MaterialSkin;
using MaterialSkin.Controls;

namespace Recording_of_requests_for_equipment_repair
{
    public partial class RegisterForm : MaterialForm
    {
        private string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=RepairRequestSystem;Integrated Security=True";
        private MaterialSingleLineTextField txtLogin, txtPassword, txtConfirmPassword, txtFirstName, txtLastName, txtEmail, txtPhone;
        private ComboBox cmbRole;
        private MaterialLabel lblError;

        public RegisterForm()
        {
            InitializeComponent();
            InitializeMaterialSkin();
            InitializeUI();
        }

        private void InitializeMaterialSkin()
        {
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
        }

        private void InitializeUI()
        {
            this.Text = "Регистрация";
            this.Size = new Size(400, 600);

            // Поля для ввода
            txtLogin = new MaterialSingleLineTextField { Hint = "Логин", Location = new Point(50, 70), Width = 300 };
            this.Controls.Add(txtLogin);

            txtPassword = new MaterialSingleLineTextField { Hint = "Пароль", Location = new Point(50, 120), Width = 300, UseSystemPasswordChar = true };
            this.Controls.Add(txtPassword);

            txtConfirmPassword = new MaterialSingleLineTextField { Hint = "Подтвердите пароль", Location = new Point(50, 170), Width = 300, UseSystemPasswordChar = true };
            this.Controls.Add(txtConfirmPassword);

            txtFirstName = new MaterialSingleLineTextField { Hint = "Имя", Location = new Point(50, 220), Width = 300 };
            this.Controls.Add(txtFirstName);

            txtLastName = new MaterialSingleLineTextField { Hint = "Фамилия", Location = new Point(50, 270), Width = 300 };
            this.Controls.Add(txtLastName);

            txtEmail = new MaterialSingleLineTextField { Hint = "Email", Location = new Point(50, 320), Width = 300 };
            this.Controls.Add(txtEmail);

            txtPhone = new MaterialSingleLineTextField { Hint = "Телефон", Location = new Point(50, 370), Width = 300 };
            this.Controls.Add(txtPhone);

            // Выпадающий список для роли
            cmbRole = new ComboBox { Location = new Point(50, 420), Width = 300 };
            cmbRole.Items.AddRange(new string[] { "Клиент", "Сотрудник", "Администратор" });
            cmbRole.SelectedIndex = 0;
            this.Controls.Add(cmbRole);

            // Кнопка регистрации
            MaterialFlatButton btnRegister = new MaterialFlatButton { Text = "Зарегистрироваться", Location = new Point(50, 470), Width = 150 };
            btnRegister.Click += (s, e) => RegisterButton_Click(s, e);
            this.Controls.Add(btnRegister);

            // Кнопка "Войти"
            MaterialFlatButton btnBack = new MaterialFlatButton { Text = "Войти", Location = new Point(240, 470), Width = 80 };
            btnBack.Click += (s, e) => BackButton_Click();
            this.Controls.Add(btnBack);

            // Метка для ошибок
            lblError = new MaterialLabel { ForeColor = Color.Red, AutoSize = true, Location = new Point(50, 510) };
            this.Controls.Add(lblError);

            // Кастомный крестик для выхода
            Button btnClose = new Button
            {
                Text = "X",
                Location = new Point(this.ClientSize.Width - 30, 5),
                Size = new Size(25, 25),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Red,
                BackColor = Color.Transparent
            };
            btnClose.Click += (s, e) => Application.Exit();
            this.Controls.Add(btnClose);
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string role = cmbRole.SelectedItem.ToString();
            lblError.Text = "";

            // Проверка заполненности всех полей
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword) ||
                string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phone))
            {
                lblError.Text = "Заполните все поля.";
                return;
            }

            // Проверка длины пароля
            if (password.Length < 6)
            {
                lblError.Text = "Пароль должен содержать минимум 6 символов.";
                return;
            }

            // Проверка совпадения паролей
            if (password != confirmPassword)
            {
                lblError.Text = "Пароли не совпадают.";
                return;
            }

            // Проверка уникальности логина
            if (LoginExists(login))
            {
                lblError.Text = "Такой аккаунт уже существует.";
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Вставка в Users
                    string userQuery = @"
                    INSERT INTO Users ([Login], [Password], [Role])
                    OUTPUT INSERTED.UserID
                    VALUES (@Login, @Password, @Role)";
                    int userId;
                    using (SqlCommand userCommand = new SqlCommand(userQuery, conn))
                    {
                        userCommand.Parameters.AddWithValue("@Login", login);
                        userCommand.Parameters.AddWithValue("@Password", password);
                        userCommand.Parameters.AddWithValue("@Role", role);
                        try
                        {
                            userId = (int)userCommand.ExecuteScalar();
                        }
                        catch (SqlException ex) when (ex.Number == 2627) // Нарушение уникальности Login
                        {
                            lblError.Text = "Такой аккаунт уже существует.";
                            return;
                        }
                    }

                    // Вставка в Clients, если роль - Клиент
                    if (role == "Клиент")
                    {
                        string clientQuery = @"
                        INSERT INTO Clients (UserID, FirstName, LastName, Email, Phone)
                        VALUES (@UserID, @FirstName, @LastName, @Email, @Phone)";
                        using (SqlCommand clientCommand = new SqlCommand(clientQuery, conn))
                        {
                            clientCommand.Parameters.AddWithValue("@UserID", userId);
                            clientCommand.Parameters.AddWithValue("@FirstName", firstName);
                            clientCommand.Parameters.AddWithValue("@LastName", lastName);
                            clientCommand.Parameters.AddWithValue("@Email", email);
                            clientCommand.Parameters.AddWithValue("@Phone", phone);
                            try
                            {
                                clientCommand.ExecuteNonQuery();
                            }
                            catch (SqlException ex) when (ex.Number == 2627) // Нарушение уникальности Email
                            {
                                // Удаляем пользователя из Users
                                string deleteUserQuery = "DELETE FROM Users WHERE UserID = @UserID";
                                using (SqlCommand deleteCommand = new SqlCommand(deleteUserQuery, conn))
                                {
                                    deleteCommand.Parameters.AddWithValue("@UserID", userId);
                                    deleteCommand.ExecuteNonQuery();
                                }
                                lblError.Text = "Такой Email уже используется.";
                                return;
                            }
                        }
                    }

                    MessageBox.Show("Регистрация успешна!");
                    this.Hide();
                    new LoginForm().Show();
                }
                catch (Exception ex)
                {
                    lblError.Text = "Ошибка при регистрации: " + ex.Message;
                }
            }
        }

        private void BackButton_Click()
        {
            this.Hide();
            new LoginForm().Show();
        }

        private bool LoginExists(string login)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM Users WHERE [Login] = @Login";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Login", login);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}