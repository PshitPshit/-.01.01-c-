using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace Recording_of_requests_for_equipment_repair
{
    public partial class LoginForm : MaterialForm
    {
        private string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=RepairRequestSystem;Integrated Security=True";
        private MaterialSingleLineTextField txtLogin;
        private MaterialSingleLineTextField txtPassword;
        private MaterialLabel lblError;

        public LoginForm()
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
            this.Text = "Авторизация";
            this.Size = new Size(400, 300);

            // Поле для логина
            txtLogin = new MaterialSingleLineTextField { Hint = "Логин", Location = new Point(50, 70), Width = 300 };
            this.Controls.Add(txtLogin);

            // Поле для пароля
            txtPassword = new MaterialSingleLineTextField { Hint = "Пароль", Location = new Point(50, 120), Width = 300, UseSystemPasswordChar = true };
            this.Controls.Add(txtPassword);

            // Кнопка входа
            MaterialFlatButton btnLogin = new MaterialFlatButton { Text = "Войти", Location = new Point(50, 170), Width = 120 };
            btnLogin.Click += (s, e) => LoginButton_Click(s, e);
            this.Controls.Add(btnLogin);

            // Кнопка регистрации
            MaterialFlatButton btnRegister = new MaterialFlatButton { Text = "Регистрация", Location = new Point(180, 170), Width = 120 };
            btnRegister.Click += (s, e) => RegisterButton_Click();
            this.Controls.Add(btnRegister);

            // Метка для ошибок
            lblError = new MaterialLabel { ForeColor = Color.Red, AutoSize = true, Location = new Point(50, 220) };
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

        private void LoginButton_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text.Trim();
            lblError.Text = "";

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                lblError.Text = "Заполните все поля.";
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT UserID, [Role] FROM Users WHERE [Login] = @Login AND [Password] = @Password";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Login", login);
                        cmd.Parameters.AddWithValue("@Password", password);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = reader.GetInt32(0);
                                string role = reader.GetString(1);

                                MessageBox.Show($"Успешный вход! Роль: {role}, UserID: {userId}");
                                this.Hide();

                                if (role == "Клиент")
                                {
                                    new ClientForm(userId).Show();
                                }
                                else if (role == "Сотрудник")
                                {
                                    new EmployeeForm(userId).Show();
                                }
                                else if (role == "Администратор")
                                {
                                    new AdminForm().Show();
                                }
                                else
                                {
                                    lblError.Text = "Неизвестная роль пользователя.";
                                    this.Show();
                                }
                            }
                            else
                            {
                                lblError.Text = "Неверный логин или пароль.";
                                MessageBox.Show("Ошибка: Неверный логин или пароль");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblError.Text = $"Ошибка подключения: {ex.Message}";
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }

        private void RegisterButton_Click()
        {
            RegisterForm registerForm = new RegisterForm();
            registerForm.Show();
            this.Hide();
        }
    }
}