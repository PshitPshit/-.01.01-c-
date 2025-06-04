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
    public partial class AdminForm : MaterialForm
    {
        private string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=RepairRequestSystem;Integrated Security=True";
        private MaterialLabel lblStatus;

        public AdminForm()
        {
            InitializeComponent();
            InitializeMaterialSkin();
            InitializeUI();
            LoadRequests();
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
            this.Text = "Панель администратора";
            this.Size = new Size(800, 600);

            // Таблица заявок
            DataGridView dgvRequests = new DataGridView
            {
                Name = "dgvRequests",
                Location = new Point(50, 70),
                Size = new Size(this.ClientSize.Width - 100, 350),
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            this.Controls.Add(dgvRequests);

            // Кнопка удаления заявки
            MaterialFlatButton btnDeleteRequest = new MaterialFlatButton
            {
                Text = "Удалить заявку",
                Location = new Point(50, 430),
                Width = 150
            };
            btnDeleteRequest.Click += (s, e) => DeleteRequest(dgvRequests);
            this.Controls.Add(btnDeleteRequest);

            // Кнопка назначения сотрудника
            MaterialFlatButton btnAssignEmployee = new MaterialFlatButton
            {
                Text = "Назначить сотрудника",
                Location = new Point(200, 430),
                Width = 150
            };
            btnAssignEmployee.Click += (s, e) => AssignEmployee(dgvRequests);
            this.Controls.Add(btnAssignEmployee);

            // Кнопка просмотра деталей
            MaterialFlatButton btnViewDetails = new MaterialFlatButton
            {
                Text = "Просмотреть детали",
                Location = new Point(400, 430),
                Width = 150
            };
            btnViewDetails.Click += (s, e) => ViewDetails(dgvRequests);
            this.Controls.Add(btnViewDetails);

            // Метка статуса
            lblStatus = new MaterialLabel
            {
                AutoSize = true,
                Location = new Point(50, 470),
                ForeColor = Color.Red
            };
            this.Controls.Add(lblStatus);

            // Кастомный крестик для выхода
            MaterialFlatButton btnClose = new MaterialFlatButton
            {
                Text = "X",
                Location = new Point(this.ClientSize.Width - 50, 10),
                Size = new Size(30, 30),
                ForeColor = Color.Red
            };
            btnClose.Click += (s, e) => Application.Exit();
            this.Controls.Add(btnClose);
        }

        private void LoadRequests()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT r.RequestID, r.RequestNumber, r.RequestDate, e.[Name] AS Equipment, 
                               ft.TypeName AS FailureType, c.FirstName + ' ' + c.LastName AS Client, 
                               rs.StatusName, r.AssignedEmployeeID, em.FirstName + ' ' + em.LastName AS EmployeeName
                        FROM RepairRequests r
                        LEFT JOIN Equipment e ON r.EquipmentID = e.EquipmentID
                        LEFT JOIN FailureTypes ft ON r.FailureTypeID = ft.FailureTypeID
                        LEFT JOIN Clients c ON r.ClientID = c.ClientID
                        LEFT JOIN RequestStatus rs ON r.StatusID = rs.StatusID
                        LEFT JOIN Employees em ON r.AssignedEmployeeID = em.EmployeeID";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    DataGridView dgvRequests = this.Controls.Find("dgvRequests", true)[0] as DataGridView;
                    dgvRequests.DataSource = dt;
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "Ошибка загрузки: " + ex.Message;
                    lblStatus.ForeColor = Color.Red;
                }
            }
        }

        private void DeleteRequest(DataGridView dgvRequests)
        {
            if (dgvRequests.SelectedRows.Count > 0)
            {
                int requestId = Convert.ToInt32(dgvRequests.SelectedRows[0].Cells["RequestID"].Value);
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string query = "DELETE FROM RepairRequests WHERE RequestID = @RequestID";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@RequestID", requestId);
                        cmd.ExecuteNonQuery();
                        LoadRequests();
                        lblStatus.Text = "Заявка удалена.";
                        lblStatus.ForeColor = Color.Green;
                    }
                    catch (Exception ex)
                    {
                        lblStatus.Text = "Ошибка удаления: " + ex.Message;
                        lblStatus.ForeColor = Color.Red;
                    }
                }
            }
            else
            {
                lblStatus.Text = "Выберите заявку для удаления.";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void AssignEmployee(DataGridView dgvRequests)
        {
            if (dgvRequests.SelectedRows.Count > 0)
            {
                int requestId = Convert.ToInt32(dgvRequests.SelectedRows[0].Cells["RequestID"].Value);
                using (MaterialForm assignForm = new MaterialForm { Text = "Назначить сотрудника", Size = new Size(300, 200) })
                {
                    var materialSkinManager = MaterialSkinManager.Instance;
                    materialSkinManager.AddFormToManage(assignForm);
                    materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
                    materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);

                    MaterialLabel lblEmployee = new MaterialLabel { Text = "Выберите сотрудника:", AutoSize = true, Location = new Point(20, 20) };
                    assignForm.Controls.Add(lblEmployee);

                    ComboBox cmbEmployees = new ComboBox { Location = new Point(20, 50), Width = 250 };
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "SELECT EmployeeID, FirstName + ' ' + LastName AS FullName FROM Employees";
                        SqlDataAdapter da = new SqlDataAdapter(query, conn);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        cmbEmployees.DataSource = dt;
                        cmbEmployees.DisplayMember = "FullName";
                        cmbEmployees.ValueMember = "EmployeeID";
                    }
                    assignForm.Controls.Add(cmbEmployees);

                    MaterialFlatButton btnAssign = new MaterialFlatButton { Text = "Назначить", Location = new Point(100, 100), Width = 80 };
                    btnAssign.Click += (s, e) =>
                    {
                        int employeeId = Convert.ToInt32(cmbEmployees.SelectedValue);
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            try
                            {
                                conn.Open();
                                string query = "UPDATE RepairRequests SET AssignedEmployeeID = @EmployeeID WHERE RequestID = @RequestID";
                                SqlCommand cmd = new SqlCommand(query, conn);
                                cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                                cmd.Parameters.AddWithValue("@RequestID", requestId);
                                cmd.ExecuteNonQuery();
                                LoadRequests();
                                lblStatus.Text = "Сотрудник назначен.";
                                lblStatus.ForeColor = Color.Green;
                                assignForm.Close();
                            }
                            catch (Exception ex)
                            {
                                lblStatus.Text = "Ошибка назначения: " + ex.Message;
                                lblStatus.ForeColor = Color.Red;
                            }
                        }
                    };
                    assignForm.Controls.Add(btnAssign);

                    MaterialFlatButton btnCancel = new MaterialFlatButton { Text = "Отмена", Location = new Point(190, 100), Width = 80 };
                    btnCancel.Click += (s, e) => assignForm.Close();
                    assignForm.Controls.Add(btnCancel);

                    assignForm.ShowDialog();
                }
            }
            else
            {
                lblStatus.Text = "Выберите заявку для назначения.";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void ViewDetails(DataGridView dgvRequests)
        {
            if (dgvRequests.SelectedRows.Count > 0)
            {
                int requestId = Convert.ToInt32(dgvRequests.SelectedRows[0].Cells["RequestID"].Value);
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string query = @"
                            SELECT r.RequestNumber, r.RequestDate, r.ProblemDescription, 
                                   e.[Name] AS Equipment, e.SerialNumber, e.[Type], e.[Location],
                                   ft.TypeName AS FailureType, 
                                   c.FirstName + ' ' + c.LastName AS Client, c.Email, c.Phone,
                                   rs.StatusName, 
                                   em.FirstName + ' ' + em.LastName AS EmployeeName
                            FROM RepairRequests r
                            LEFT JOIN Equipment e ON r.EquipmentID = e.EquipmentID
                            LEFT JOIN FailureTypes ft ON r.FailureTypeID = ft.FailureTypeID
                            LEFT JOIN Clients c ON r.ClientID = c.ClientID
                            LEFT JOIN RequestStatus rs ON r.StatusID = rs.StatusID
                            LEFT JOIN Employees em ON r.AssignedEmployeeID = em.EmployeeID
                            WHERE r.RequestID = @RequestID";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@RequestID", requestId);
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            using (MaterialForm detailsForm = new MaterialForm { Text = "Детали заявки", Size = new Size(400, 500) })
                            {
                                var materialSkinManager = MaterialSkinManager.Instance;
                                materialSkinManager.AddFormToManage(detailsForm);
                                materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
                                materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);

                                int yPosition = 20;
                                AddDetailLabel(detailsForm, "Номер заявки:", reader["RequestNumber"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Дата заявки:", reader["RequestDate"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Описание проблемы:", reader["ProblemDescription"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Оборудование:", reader["Equipment"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Серийный номер:", reader["SerialNumber"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Тип оборудования:", reader["Type"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Местоположение:", reader["Location"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Тип неисправности:", reader["FailureType"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Клиент:", reader["Client"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Email клиента:", reader["Email"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Телефон клиента:", reader["Phone"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Статус:", reader["StatusName"].ToString(), ref yPosition);
                                AddDetailLabel(detailsForm, "Назначенный сотрудник:", reader["EmployeeName"].ToString(), ref yPosition);

                                MaterialFlatButton btnCloseDetails = new MaterialFlatButton { Text = "Закрыть", Location = new Point(150, yPosition + 20), Width = 100 };
                                btnCloseDetails.Click += (s, e) => detailsForm.Close();
                                detailsForm.Controls.Add(btnCloseDetails);

                                detailsForm.ShowDialog();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        lblStatus.Text = "Ошибка просмотра деталей: " + ex.Message;
                        lblStatus.ForeColor = Color.Red;
                    }
                }
            }
            else
            {
                lblStatus.Text = "Выберите заявку для просмотра деталей.";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void AddDetailLabel(Form form, string labelText, string value, ref int yPosition)
        {
            MaterialLabel lbl = new MaterialLabel
            {
                Text = $"{labelText} {value}",
                AutoSize = true,
                Location = new Point(20, yPosition),
                MaximumSize = new Size(360, 0)
            };
            form.Controls.Add(lbl);
            yPosition += lbl.Height + 5;
        }
    }
}