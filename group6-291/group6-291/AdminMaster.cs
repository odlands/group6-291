﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace group6_291
{
    public partial class AdminMaster : Form
    {
        Form1 loginForm;
        DataSet patientList = new DataSet();
        DataSet doctorList = new DataSet();
        DataSet accountList = new DataSet();
        DataSet wardList = new DataSet();
        public AdminMaster(Form1 login)
        {
            InitializeComponent();
            loginForm = login;
        }
        private void AdminMaster_Load(object sender, EventArgs e)
        {
            addUsername.Leave += new EventHandler(addUsername_Leave);
            addPassword.Leave += new EventHandler(addPassword_Leave);
            populateAccountList();
            addWardNameBox.Leave += new EventHandler(addWardNameBox_Leave);
            AddWardCapacityBox.Leave += new EventHandler(addWardCapacityBox_Leave);
            addSINBox.TextChanged += new EventHandler(addSINBox_TextChanged);
            registerListBox.DoubleClick += new EventHandler(registerListBox_DoubleClick);
            populateWardList();
            populateDoctorList();
            populatePatientList();
        }

        //Admin Account Functions

        //Purpose: Populate the account list box with all the registered users
        private void populateDoctorList()
        {
            DoctorErrorLabel.Text = "";
            DoctorUpdateError.Text = "";
            //Open connection and create a dataset from the query
            SqlConnection conn = new SqlConnection(Globals.conn);
            conn.Open();
            DataSet ds = new DataSet();
            doctorList = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter("select concat(firstName, ' ',lastName) AS Name, * from [Doctor]", conn);
            //Fill the dataset, sort it, and bind it to the list box
            adapter.Fill(doctorList);
            adapter.Fill(ds);
            ds.Tables[0].DefaultView.Sort = "Name asc";
            DoctorListBox.DataSource = ds.Tables[0];
            DoctorListBox.DisplayMember = "Name";
            conn.Close();
        }


        //Purpose: Populate the account list box with all the registered users
        private void populateAccountList()
        {
            //Open connection and create a dataset from the query
            DataSet ds = new DataSet();
            SqlConnection conn = new SqlConnection(Globals.conn);
            conn.Open();
            SqlDataAdapter adapter = new SqlDataAdapter("select username, password, isAdmin from [User]", conn);
            //Fill the dataset, sort it, and bind it to the list box
            accountList = new DataSet();
            adapter.Fill(accountList);
            adapter.Fill(ds);
            ds.Tables[0].DefaultView.Sort = "username asc";
            accountListBox.DataSource = ds.Tables[0];
            accountListBox.DisplayMember = "username";
            conn.Close();
        }

        //Purpose: Change the Username textbox's error label if it is invalid after user leaves the texbox
        private void addUsername_Leave(object sender, EventArgs e)
        {
            bool validUsername = usernameIsValid();
        }

        //Purpose: Change the Password textbox's error label if it is invalid after user leaves the texbox
        private void addPassword_Leave(object sender, EventArgs e)
        {
            bool validPassword = passwordIsValid();

        }

        //Purpose: Uncheck the receptionist checkbox if the admin one was checked
        private void adminCheckBox_CheckChanged(object sender, EventArgs e)
        {
            if (adminCheckbox.Checked == true)
            {
                recepCheckbox.Checked = false;
                checkboxInfo.Text = "";
            }
        }

        //Purpose: Uncheck the admin checkbox if the receptionist one was checked
        private void recepCheckBox_CheckChanged(object sender, EventArgs e)
        {
            if (recepCheckbox.Checked == true)
            {
                adminCheckbox.Checked = false;
                checkboxInfo.Text = "";
            }
        }

        //Purpose: Chceck that the username texbox has a valid username as its input
        private bool usernameIsValid()
        {
            string inputedUser = addUsername.Text;
            //Check username against username constraints
            if (!Regex.IsMatch(inputedUser, @"^[a-zA-Z0-9]+$"))
            {
                usernameInfo.Text = "*Invalid username characters";
                usernameInfo.ForeColor = Color.Red;
                return false;
            }
            else if (inputedUser.Length < 4 || inputedUser.Length > 16)
            {
                usernameInfo.Text = "*Invalid username length";
                usernameInfo.ForeColor = Color.Red;
                return false;
            }
            //Check to see if the username already exists
            else
            {
                SqlConnection conn = new SqlConnection(Globals.conn);
                conn.Open();
                SqlCommand checkUsername = new SqlCommand("select count(*) from [User] where username = @user", conn);
                checkUsername.Parameters.AddWithValue("@user", inputedUser);
                int userExist = (int)checkUsername.ExecuteScalar();
                //Username already exists
                if (userExist > 0)
                {
                    usernameInfo.Text = "*Username already exists";
                    usernameInfo.ForeColor = Color.Red;
                    return false;
                }
                //Username does not already exist
                else
                {
                    usernameInfo.Text = "*Username is available";
                    usernameInfo.ForeColor = Color.Green;
                    return true;
                }
            }
        }

        //Purpose: Chceck that the password texbox has a valid password as its input
        private bool passwordIsValid()
        {
            string inputedPassword = addPassword.Text;
            //Check password against password constraints
            if (inputedPassword.Length < 4 || inputedPassword.Length > 16)
            {
                passwordInfo.Text = "*Invalid password length";
                passwordInfo.ForeColor = Color.Red;
                return false;
            }

            else if (!Regex.IsMatch(inputedPassword, @"^[a-zA-Z0-9]+$"))
            {
                passwordInfo.Text = "*Invalid password characters";
                passwordInfo.ForeColor = Color.Red;
                return false;
            }
            //Password is valid
            else
            {
                passwordInfo.Text = "";
                return true;
            }
        }

        //Purpose: Reset all reporting and input fields
        private void resetAddUserFields()
        {
            addUsername.Text = "";
            addPassword.Text = "";
            usernameInfo.Text = "";
            passwordInfo.Text = "";
            checkboxInfo.Text = "";
            adminCheckbox.Checked = false;
            recepCheckbox.Checked = false;
        }

        //Purpose: Add a new user to the User table in the database when the Add Account button is clicked
        private void addAccountButton_Click(object sender, EventArgs e)
        {
            //Make sure a role is checked
            if (adminCheckbox.Checked == false && recepCheckbox.Checked == false)
            {
                checkboxInfo.Text = "*Please select a role";
                checkboxInfo.ForeColor = Color.Red;
            }
            //Make sure username and password are valid
            else if (!usernameIsValid() || !passwordIsValid())
            {
                resetAddUserFields();
                requestInfo.Text = "*Invalid add user request. Please fix errors!";
                requestInfo.ForeColor = Color.Red;
            }
            //All criteria is met, add user to databse
            else
            {
                //Get role
                bool isAdmin;
                if (adminCheckbox.Checked)
                    isAdmin = true;
                else
                    isAdmin = false;
                //Insert into database
                SqlConnection conn = new SqlConnection(Globals.conn);
                conn.Open();
                SqlCommand addUser = new SqlCommand("insert into [User] (username, password, isAdmin) values (@username, @password, @isAdmin)", conn);
                addUser.Parameters.AddWithValue("@username", addUsername.Text);
                addUser.Parameters.AddWithValue("@password", addPassword.Text);
                addUser.Parameters.AddWithValue("@isAdmin", isAdmin);
                addUser.ExecuteNonQuery();
                //Update status and reset fields
                requestInfo.Text = "User added successfully";
                requestInfo.ForeColor = Color.Green;
                resetAddUserFields();
                conn.Close();
            }
            populateAccountList();
        }

        //Purpose: Reset all reporting fields INCLUDING the add user response when reset button is clicked
        private void resetUserButton_Click(object sender, EventArgs e)
        {
            resetAddUserFields();
            requestInfo.Text = "";
        }

        private void deleteAccountButton_Click(object sender, EventArgs e)
        {
            DataRowView accViewItem = accountListBox.SelectedItem as DataRowView;
            string username = accViewItem["username"].ToString();

            SqlConnection conn = new SqlConnection(Globals.conn);
            conn.Open();
            SqlCommand deleteUser = new SqlCommand("delete from [User] where username = @username", conn);
            deleteUser.Parameters.AddWithValue("@username", username);
            deleteUser.ExecuteNonQuery();
            conn.Close();
            //Update status and reset fields
            populateAccountList();
            requestInfo.Text = "";
        }

        private void accountListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            addUsername.Text = "";
            addPassword.Text = "";
            UpdateCheckLabel.Text = "";
            DataRowView drvItem = accountListBox.SelectedItem as DataRowView;
            string user = drvItem["username"].ToString();
            string admin = drvItem["isAdmin"].ToString();
            string password = drvItem["password"].ToString();

            if (admin.Equals("True"))
            { 
                UpdateRecpCheckBox.Checked = false;
                UpdateAdminCheckBox.Checked = true;
            }
            else
            {
                UpdateAdminCheckBox.Checked = false;
                UpdateRecpCheckBox.Checked = true;
            }
            AccountUpdateLabel.Text = user;
        }

        //Purpose: Update selected account on update button click 
        private void UpdateAccountButton_Click(object sender, EventArgs e)
        {
            //Get selected itme values
            DataRowView drvItem = accountListBox.SelectedItem as DataRowView;
            string user = drvItem["username"].ToString();
            string pass = drvItem["password"].ToString();

            //Make sure a role is checked
            if (UpdateRecpCheckBox.Checked == false && UpdateAdminCheckBox.Checked == false)
            {
                UpdateCheckLabel.Text = "*Please select a role";
                UpdateCheckLabel.ForeColor = Color.Red;
                return;
            }

            SqlConnection conn = new SqlConnection(Globals.conn);
            conn.Open();
            var sql = "UPDATE [User] SET username = @username, password = @password, isAdmin= @isAdmin where username=@userID";// repeat for all variables
            SqlCommand UpdateUser = new SqlCommand(sql, conn);

            // this needs to be seperate statements
            if (UpdateUserText.TextLength.Equals(0))
            {
                UpdateUser.Parameters.AddWithValue("@username", user);
            } else
            {
                UpdateUser.Parameters.AddWithValue("@username", UpdateUserText.Text);
            }
            if (UpdatePassText.TextLength.Equals(0))
            {
                UpdateUser.Parameters.AddWithValue("@password", pass);
            } else
            {
                UpdateUser.Parameters.AddWithValue("@password", UpdatePassText.Text);
            }
            if (UpdateRecpCheckBox.Checked)
            {
                UpdateUser.Parameters.AddWithValue("@isAdmin", false);
            }
            else
            {
                UpdateUser.Parameters.AddWithValue("@isAdmin", true);
            }
            UpdateUser.Parameters.AddWithValue("@userID", user);
            UpdateUser.ExecuteNonQuery();
            //Update status and reset fields
            UpdateCheckLabel.Text = "User updated successfully";
            UpdateCheckLabel.ForeColor = Color.Green;
            UpdateUserText.Clear();
            UpdatePassText.Clear();
            populateAccountList();
            
        }

        //Purpose: Change checkbox value based on reception checkbox change 
        private void UpdateRecpCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdateAdminCheckBox.Checked == true)
            {
                //UpdateRecpCheckBox.Checked = true;
                UpdateAdminCheckBox.Checked = false;

            } else
            {
                UpdateRecpCheckBox.Checked = true;
            }
        }

        //Purpose: Change checkbox value based on admin checkbox change 
        private void UpdateAdminCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdateRecpCheckBox.Checked == true)
            {
                //UpdateAdminCheckBox.Checked = true;
                UpdateRecpCheckBox.Checked = false;

            } else
            {
                UpdateAdminCheckBox.Checked = true;
            }

        }

        //Admin Ward Functions

        //Purpose: Populate the ward list box with all the registered wards
        private void populateWardList()
        {
            //Open connection and create a dataset from the query
            SqlConnection conn = new SqlConnection(Globals.conn);
            conn.Open();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter("select * from [Ward]", conn);
            //Fill the dataset, sort it, and bind it to the list box
            wardList = new DataSet();
            adapter.Fill(wardList);
            adapter.Fill(ds);
            ds.Tables[0].DefaultView.Sort = "wardName asc";
            wardListBox.DataSource = ds.Tables[0];
            wardListBox.DisplayMember = "wardName";
            conn.Close();
        }

        //Purpose: make sure ward name is available and valid after being inputed
        private void addWardNameBox_Leave(object sender, EventArgs e)
        {
            wardIsValid();
        }

        //Purpose: make sure ward capacity is valid after being inputed
        private void addWardCapacityBox_Leave(object sender, EventArgs e)
        {
            capacityIsValid();
        }

        //Purpose: Check if a ward name is available to use
        private bool wardIsValid()
        {
            string inputedWardName = addWardNameBox.Text;
            //Check username against username constraints
            if (!Regex.IsMatch(inputedWardName, @"^[a-zA-Z]+$"))
            {
                addWardInfo.Text = "*Invalid ward name characters";
                addWardInfo.ForeColor = Color.Red;
                return false;
            }
            else if (inputedWardName.Length < 2 || inputedWardName.Length > 32)
            {
                addWardInfo.Text = "*Invalid ward name length";
                addWardInfo.ForeColor = Color.Red;
                return false;
            }
            //Check to see if the username already exists
            else
            {
                SqlConnection conn = new SqlConnection(Globals.conn);
                conn.Open();
                SqlCommand checkUsername = new SqlCommand("select count(*) from [Ward] where wardName = @wardName", conn);
                checkUsername.Parameters.AddWithValue("@wardName", inputedWardName);
                int wardExist = (int)checkUsername.ExecuteScalar();
                //Username already exists
                if (wardExist > 0)
                {
                    addWardInfo.Text = "*Ward already exists";
                    addWardInfo.ForeColor = Color.Red;
                    return false;
                }
                //Username does not already exist
                else
                {
                    addWardInfo.Text = "*Ward name is available";
                    addWardInfo.ForeColor = Color.Green;
                    return true;
                }
            }
        }

        //Purpose: Check if a capacity is valid
        private bool capacityIsValid()
        {
            int capacity;
            if (Int32.TryParse(AddWardCapacityBox.Text, out capacity))
            {
                if (capacity > 0)
                {
                    addWardCapInfo.Text = "";
                    return true;
                }
                else
                {
                    addWardCapInfo.Text = "*Capacity must be greater than 0";
                    addWardCapInfo.ForeColor = Color.Red;
                    return false;
                }
            }
            else
            {
                addWardCapInfo.Text = "*Invalid capacity";
                addWardCapInfo.ForeColor = Color.Red;
                return false;
            }
        }

        //Purpose: Add a ward to the Ward table if all input is valid
        private void addWardButt_Click(object sender, EventArgs e)
        {
            //Make sure ward name and capacity are valid
            if (!wardIsValid() || !capacityIsValid())
            {
                addWardRequestInfo.Text = "*Invalid add ward request. Please fix errors!";
                addWardRequestInfo.ForeColor = Color.Red;
            }
            //All criteria is met, add user to databse
            else
            {
                //Insert into database
                SqlConnection conn = new SqlConnection(Globals.conn);
                conn.Open();
                SqlCommand addWard = new SqlCommand("insert into [Ward] (wardName, overall_capacity, current_capacity) values (@wardName, @overallCap, @currentCap)", conn);
                addWard.Parameters.AddWithValue("@wardName", addWardNameBox.Text);
                addWard.Parameters.AddWithValue("@overallCap", Int32.Parse(AddWardCapacityBox.Text));
                addWard.Parameters.AddWithValue("@currentCap", 0);
                addWard.ExecuteNonQuery();
                conn.Close();
                //Update status and reset fields
                addWardRequestInfo.Text = "Ward added successfully";
                addWardRequestInfo.ForeColor = Color.Green;
                resetAddWardFields();
                populateWardList();
            }
        }

        //Purpose: Reset all add ward reporting and input fields
        private void resetAddWardFields()
        {
            addWardNameBox.Text = "";
            AddWardCapacityBox.Text = "";
            addWardInfo.Text = "";
            addWardCapInfo.Text = "";
        }

        //Purpose: Reset all add ward reporting and input fields when reset button is clicked
        private void addWardReset_Click(object sender, EventArgs e)
        {
            resetAddWardFields();
            addWardRequestInfo.Text = "";
        }

        //Purpose: Delete the selected ward on delete click
        private void deleteWardButton_Click(object sender, EventArgs e)
        {
            //Get selected ward values
            DataRowView wardViewItem = wardListBox.SelectedItem as DataRowView;
            string wardName = wardViewItem["wardName"].ToString();

            SqlConnection conn = new SqlConnection(Globals.conn);
            conn.Open();
            SqlCommand addWard = new SqlCommand("delete from [Ward] where wardName = @wardName", conn);
            addWard.Parameters.AddWithValue("@wardName", wardName);
            addWard.ExecuteNonQuery();
            conn.Close();
            //Update status and reset fields
            populateWardList();
            wardUpdateReqInfo.Text = "";
            addWardInfo.Text = "";
            populateWardList();
        }

        //Purpose: Update selected ward with inputed info on update click
        private void WardUpdateButton_Click(object sender, EventArgs e)
        {
            //Get selected ward values
            DataRowView wardViewItem = wardListBox.SelectedItem as DataRowView;
            string wardName = wardViewItem["wardName"].ToString();
            int overallCap = Int32.Parse(wardViewItem["overall_capacity"].ToString());
            int currentCap = Int32.Parse(wardViewItem["current_capacity"].ToString());

            SqlConnection conn = new SqlConnection(Globals.conn);
            conn.Open();
            SqlCommand updateWard = new SqlCommand("update [Ward] set wardName = @newWardName, overall_capacity = @newOverallCap where wardName = @oldWardName", conn);
            updateWard.Parameters.AddWithValue("@oldWardName", wardName);

            //Use new ward name if one is given
            if (updateWardNameBox.Text.Length > 0)
                updateWard.Parameters.AddWithValue("@newWardName", updateWardNameBox.Text);
            else
                updateWard.Parameters.AddWithValue("@newWardName", wardName);

            //Use new capacity if a valid one is given
            int newCapacity;
            if (updateWardCapacityBox.Text.Length > 0)
            {
                updateSelectedWard.Text = "we here";
                if (Int32.TryParse(updateWardCapacityBox.Text, out newCapacity))
                {
                    if (newCapacity > 0 && (currentCap <= newCapacity))
                        updateWard.Parameters.AddWithValue("@newOverallCap", newCapacity);
                    else
                    {
                        wardUpdateReqInfo.Text = "*Invalid new capacity";
                        wardUpdateReqInfo.ForeColor = Color.Red;
                        conn.Close();
                        return;
                    }
                }
                else
                {
                    wardUpdateReqInfo.Text = "*Invalid new capacity number";
                    wardUpdateReqInfo.ForeColor = Color.Red;
                    conn.Close();
                    return;
                }
            }
            else
                updateWard.Parameters.AddWithValue("@newOverallCap", overallCap);

            updateWard.ExecuteNonQuery();
            wardUpdateReqInfo.Text = "Ward successfully update";
            wardUpdateReqInfo.ForeColor = Color.Green;
            resetUpdateWardFields();
            conn.Close();
            populateWardList();
        }

        //Purpose: Reset all update ward reporting and input fields
        private void resetUpdateWardFields()
        {
            updateWardNameBox.Text = "";
            updateWardCapacityBox.Text = "";
        }

        //Purpose: Update ward update tabs information when a new ward is selected
        private void wardListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentWardPatientInfo();
            //Get selected ward values
            DataRowView wardViewItem = wardListBox.SelectedItem as DataRowView;
            string wardName = wardViewItem["wardName"].ToString();
            string overallCap = wardViewItem["overall_capacity"].ToString();
            string currentCap = wardViewItem["current_capacity"].ToString();
            //Update labels
            updateCurrentName.Text = wardName;
            updateCurrentCap.Text = currentCap;
            updateOverallCap.Text = overallCap;
            //Check if ward is full or not
            if (Int32.Parse(overallCap) == Int32.Parse(currentCap))
                updateCurrentStatus.Text = "Full";
            else
                updateCurrentStatus.Text = "Not Full";
            updateWardNameBox.Text = "";
            updateWardCapacityBox.Text = "";
        }


        //Purpose: Reset all add ward reporting and input fields on reset click
        private void resetUpdateWard_Click(object sender, EventArgs e)
        {
            resetUpdateWardFields();
            wardUpdateReqInfo.Text = "";
        }

        private void DoctorListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DoctorUpdDeptError.Text = "";
            DoctorErrorLabel.Text = "";
            DoctorUpdateError.Text = "";
            docDeptFilter.Items.Clear();
            DoctorDeptBox.Items.Clear();
            DoctorUpdDeptBox.Items.Clear();
            DataRowView DoctorList = DoctorListBox.SelectedItem as DataRowView;
            string firstName = DoctorList["firstName"].ToString();
            string lastName = DoctorList["lastName"].ToString();
            string departmentName = DoctorList["departmentName"].ToString();
            string specialization = DoctorList["specialization"].ToString();
            string duties = DoctorList["duties"].ToString();

            DoctorUpdFirstName.Text = firstName;
            DoctorUpdLastName.Text = lastName;
            DoctorUpdDeptBox.SelectedIndex = DoctorUpdDeptBox.FindStringExact(departmentName);
            DoctorUpdSpec.Text = specialization;
            DoctorUpdDuty.Text = duties;

            SqlConnection conn = new SqlConnection(Globals.conn);
            conn.Open();
            DataTable Department = new DataTable("Department");
            SqlDataAdapter adap = new SqlDataAdapter("Select * from [Department]", conn);
            adap.Fill(Department);
            foreach (DataRow items in Department.Rows)
            {
                DoctorDeptBox.Items.Add(items[0].ToString());
                DoctorUpdDeptBox.Items.Add(items[0].ToString());
                docDeptFilter.Items.Add(items[0].ToString());
            }
            string departmentName1 = DoctorList["departmentName"].ToString();
            DoctorUpdDeptBox.SelectedItem = departmentName1;//DoctorUpdDeptBox.Items.IndexOf(departmentName1);//DoctorUpdDeptBox.FindStringExact(departmentName1);
            conn.Close();
            DoctorDeptBox.SelectedIndex = -1;
        }

        private void UpdateDoctorButton_Click(object sender, EventArgs e)
        {

            DataRowView DoctorList = DoctorListBox.SelectedItem as DataRowView;
            string doctorID = DoctorList["doctorID"].ToString();

            if (DoctorUpdDeptBox.SelectedIndex == -1)
            {
                DoctorUpdDeptError.Text = "Select a department name";
                DoctorUpdDeptError.ForeColor = Color.Red;
            }
            else if (DoctorUpdFirstName.TextLength.Equals(0) || DoctorUpdLastName.TextLength.Equals(0) 
               || DoctorUpdSpec.TextLength.Equals(0))
            {
                DoctorUpdateError.Text = "Cannot have empty required fields";
                DoctorUpdateError.ForeColor = Color.Red;
            }
            else
            {
                //Update database
                SqlConnection conn = new SqlConnection(Globals.conn);
                conn.Open();
                var sql = "UPDATE [Doctor] SET firstName=@firstName, lastName=@lastName, departmentName=@department, "
                    + "specialization=@spec, duties=@duty where doctorID=@doctorID";// repeat for all variables

                SqlCommand updateDoctor = new SqlCommand(sql, conn);
                updateDoctor.Parameters.AddWithValue("@firstName", DoctorUpdFirstName.Text);
                updateDoctor.Parameters.AddWithValue("@lastName", DoctorUpdLastName.Text);
                updateDoctor.Parameters.AddWithValue("@department", DoctorUpdDeptBox.SelectedItem.ToString());
                updateDoctor.Parameters.AddWithValue("@spec", DoctorUpdSpec.Text);

                if (DoctorUpdDuty.Text.Length > 0)
                {
                    updateDoctor.Parameters.AddWithValue("@duty", DoctorUpdDuty.Text);
                } else
                {
                    updateDoctor.Parameters.AddWithValue("@duty", DBNull.Value);
                }
                updateDoctor.Parameters.AddWithValue("@doctorID", Int32.Parse(doctorID));
                updateDoctor.ExecuteNonQuery();
                conn.Close();
                //Update status and reset fields
                //resetDoctorAddFields();
                populateDoctorList();
                DoctorUpdateError.Text = "Doctor updated successfully";
                DoctorUpdateError.ForeColor = Color.Green;
                
            }
        }

        private void AddDoctorButton_Click(object sender, EventArgs e)
        {
            if (DoctorFirstNameText.TextLength.Equals(0) || DoctorLastNameText.TextLength.Equals(0)
                || DoctorDeptBox.SelectedIndex == -1 || DoctorSpecText.TextLength.Equals(0))
            {
                DoctorErrorLabel.Text = "Cannot have empty required fields";
                DoctorErrorLabel.ForeColor = Color.Red;
            }
            else
            {
                //Insert into database
                SqlConnection conn = new SqlConnection(Globals.conn);
                conn.Open();
                SqlCommand addDoctor = new SqlCommand("insert into [Doctor] (firstName, lastName, departmentName, specialization, duties)" +
                    "values (@firstName, @lastName, @department, @spec, @duty)", conn);
                addDoctor.Parameters.AddWithValue("@firstName", DoctorFirstNameText.Text);
                addDoctor.Parameters.AddWithValue("@lastName", DoctorLastNameText.Text);
                addDoctor.Parameters.AddWithValue("@department", DoctorDeptBox.SelectedItem.ToString());
                addDoctor.Parameters.AddWithValue("@spec", DoctorSpecText.Text);
                if(DoctorDutyText.Text.Length > 0)
                    addDoctor.Parameters.AddWithValue("@duty", DoctorDutyText.Text);
                else
                    addDoctor.Parameters.AddWithValue("@duty", DBNull.Value);
                addDoctor.ExecuteNonQuery();
                conn.Close();
                //Update status and reset fields
                resetDoctorAddFields();
                populateDoctorList();
                DoctorErrorLabel.Text = "Doctor added successfully";
                DoctorErrorLabel.ForeColor = Color.Green;
            }
        }

        private void resetDoctorAddFields()
        {
            DoctorFirstNameText.Text = "";
            DoctorLastNameText.Text = "";
            DoctorDeptBox.SelectedIndex.Equals(0);
            DoctorDutyText.Text = "";
            DoctorSpecText.Text = "";
        }

        private void UpdateDoctorReset_Click(object sender, EventArgs e)
        {
            DoctorUpdFirstName.Text = "";
            DoctorUpdLastName.Text = "";
            DoctorUpdDeptBox.SelectedIndex.Equals(0);
            DoctorUpdSpec.Text = "";
            DoctorUpdDuty.Text = "";
        }


        private void DeleteDoctorButton_Click(object sender, EventArgs e)
        {
            DataRowView DoctorList = DoctorListBox.SelectedItem as DataRowView;
            int doctorID = Int32.Parse(DoctorList["doctorID"].ToString());

            SqlConnection conn = new SqlConnection(Globals.conn);
            conn.Open();
            SqlCommand addWard = new SqlCommand("delete from [Doctor] where doctorID=@doctorID", conn);
            addWard.Parameters.AddWithValue("@doctorID", doctorID);
            addWard.ExecuteNonQuery();
            conn.Close();
            //Update status and reset fields
            populateDoctorList();
        }


        // ====================Patient Registration===================

        //Purpose: Populate the Patient list box with all the registered patients
        private void populatePatientList()
        {
            patientList.Clear();
            //Open connection and create a dataset from the query
            SqlConnection conn = new SqlConnection(Globals.conn);
            conn.Open();
            
            SqlDataAdapter adapter = new SqlDataAdapter("SELECT *, CONCAT(lastName, ', ', firstName) as fullName FROM [Patient]", conn);
            //Fill the dataset, sort it, and bind it to the list box
            adapter.Fill(patientList);
            patientList.Tables[0].DefaultView.Sort = "fullName asc";
            registerListBox.DataSource = patientList.Tables[0];
            registerListBox.DisplayMember = "fullName";
            conn.Close();
        }

        //Purpose: Reset all reporting and input fields for Patient Registration
        private void resetAddRegisterFields()
        {
            addSINBox.Text = "";
            addPatientTypeBox.SelectedIndex = -1;
            addFirstNameBox.Text = "";
            addLastNameBox.Text = "";
            addStreetBox.Text = "";
            addCityBox.Text = "";
            addProvinceBox.Text = "";
            addCountryBox.Text = "";
            addGenderBox.SelectedIndex = -1;
            addDOBBox.Text = "";
            //addAdmitDateBox.Text = "";
            //addDepartDateBox.Text = "";
            addInsuranceBox.Text = "";
            addHomePhoneBox.Text = "";
            addCellphoneBox.Text = "";
            addNotesBox.Text = "";
            populatePatientList();
        }

        //Purpose: Reset all reporting fields INCLUDING the add user response when reset button is clicked
        private void resetRegisterButton_Click(object sender, EventArgs e)
        {
            resetAddRegisterFields();
            addRegisterInfo.Text = "";
            addRegisterRequestInfo.Text = "";
        }

        //Purpose: Add a new patient registration to the Patient and Registration table in the database when the Add button is clicked
        private void addRegisterButton_Click(object sender, EventArgs e)
        {
            //Clear error/success info everytime add button is clicked
            addRegisterInfo.Text = "";
            addRegisterRequestInfo.Text = "";

            //If all criteria for every field is met, add user to databse
            if (fieldsAreValid())
            {
                //Insert into database
                SqlConnection conn = new SqlConnection(Globals.conn);
                //conn.Open();

                //Check if patient already exists in Patient Table
                if (patientIsValid())
                {
                    conn.Open();
                    SqlCommand addPatient = new SqlCommand(@"INSERT into [Patient] (patientSIN, patientType, firstName, lastName, street, city, province, country, sex, dateOfBirth) 
                                                        VALUES (@SIN, @pType, @fName, @lName, @street, @city, @province, @country, @sex, @dateOfBirth)", conn);
                    addPatient.Parameters.AddWithValue("@SIN", addSINBox.Text);
                    addPatient.Parameters.AddWithValue("@pType", Int32.Parse(addPatientTypeBox.Text));
                    addPatient.Parameters.AddWithValue("@fName", addFirstNameBox.Text);
                    addPatient.Parameters.AddWithValue("@lName", addLastNameBox.Text);
                    addPatient.Parameters.AddWithValue("@street", addStreetBox.Text);
                    addPatient.Parameters.AddWithValue("@city", addCityBox.Text);
                    addPatient.Parameters.AddWithValue("@province", addProvinceBox.Text);
                    addPatient.Parameters.AddWithValue("@country", addCountryBox.Text);
                    addPatient.Parameters.AddWithValue("@sex", addGenderBox.Text);
                    addPatient.Parameters.AddWithValue("@dateOfBirth", addDOBBox.Text);
                    addPatient.ExecuteNonQuery();
                    conn.Close();
                }
                if (checkRegister())
                {
                    conn.Open();
                    SqlCommand addRegister = new SqlCommand(@"INSERT into [Register] (patientSIN, admitDate, notes) VALUES (@SIN, @admitDate, @notes)", conn);
                    addRegister.Parameters.AddWithValue("@SIN", addSINBox.Text);
                    addRegister.Parameters.AddWithValue("@admitDate", DateTime.Now);

                    //Check if notes are null
                    if (addNotesBox.TextLength == 0) { addRegister.Parameters.AddWithValue("@notes", DBNull.Value); }
                    else { addRegister.Parameters.AddWithValue("@notes", addNotesBox.Text); }

                    addRegister.ExecuteNonQuery();
                    conn.Close();
                    //Update status and reset fields
                    addRegisterRequestInfo.Text = "Patient registered successfully";
                    addRegisterRequestInfo.ForeColor = Color.Green;
                    resetAddRegisterFields();

                }
                
            }
        }
        private bool checkRegister()
        {
            SqlConnection conn = new SqlConnection(Globals.conn);
            conn.Open();
            string SIN = addSINBox.Text;
            //checks if the patient is still registered
            SqlCommand checkSIN = new SqlCommand("select count(*) from [Register] where patientSIN = @patientSIN and leaveDate is null", conn);
            checkSIN.Parameters.AddWithValue("@patientSin", SIN);
            int SINExist = (int)checkSIN.ExecuteScalar();
            //Record (SIN) already exists
            if (SINExist > 0)
            {
                addRegisterInfo.Text = "*Patient is already registered,\n no new record was created.";
                addRegisterInfo.ForeColor = Color.Red;
                conn.Close();
                return false;
            }
            conn.Close();
            return true;
        }

        //Check all fields after add button is pressed. If there are any invalid fields, points them out.
        private bool fieldsAreValid()
        {
            string inputedSIN = addSINBox.Text;
            string inputedFName = addFirstNameBox.Text;
            string inputedLName = addLastNameBox.Text;
            string inputedStreet = addStreetBox.Text;
            string inputedCity = addCityBox.Text;
            string inputedProvince = addProvinceBox.Text;
            string inputedCountry = addCountryBox.Text;
            string inputedPType = addPatientTypeBox.SelectedText;
            string inputedGender = addGenderBox.SelectedText;

            addRegisterInfo.ForeColor = Color.Red;

            //Check SIN against SIN constraints (all num, length = 9)
            if (!Regex.IsMatch(inputedSIN, @"^[0-9]+$") | inputedSIN.Length != 9)
            {
                addRegisterInfo.Text += "*SIN must be 9 numbers\n";
            }
            //Check First Name
            if (!Regex.IsMatch(inputedFName, @"^[a-zA-Z]+$") | inputedFName.Length > 32)
            {
                addRegisterInfo.Text += "*First Name must be between 0 and 32 letters long.\n";
            }
            //Check Last Name
            if (!Regex.IsMatch(inputedLName, @"^[a-zA-Z]+$") | inputedLName.Length > 32)
            {
                addRegisterInfo.Text += "*Last Name must be between 0 and 32 letters long.\n";
            }
            //Check Street (Allow for numbers and white space)
            if (!Regex.IsMatch(inputedStreet, @"^[a-zA-Z0-9\s]+$") | inputedStreet.Length > 50)
            {
                addRegisterInfo.Text += "*Street must be between 0 and 50 characters long.\n";
            }
            //Check City (Allow whitespace)
            if (!Regex.IsMatch(inputedCity, @"^[a-zA-Z\s]+$") | inputedCity.Length > 50)
            {
                addRegisterInfo.Text += "*City must be between 0 and 50 characters long.\n";
            }
            //Check Province (Allow whitespace)
            if (!Regex.IsMatch(inputedProvince, @"^[a-zA-Z\s]+$") | inputedProvince.Length > 50)
            {
                addRegisterInfo.Text += "*Province must be between 0 and 50 characters long.\n";
            }
            //Check Country (Allow whitespace)
            if (!Regex.IsMatch(inputedCountry, @"^[a-zA-Z\s]+$") | inputedCountry.Length > 50)
            {
                addRegisterInfo.Text += "*Country must be between 0 and 50 characters long.\n";
            }
            //Check DOB, Admit Date, and Depart Date: 1. Valid date, 1.5 Depart is completely empty or full, 2. DOB < Admit < Depart
            dateIsValid();
            //Check Patient Type, error if nothing chosen
            if (addPatientTypeBox.SelectedItem == null)
            {
                addRegisterInfo.Text += "*Please choose a Patient Type.\n";
            }
            //Check Gender, error if nothing chosen
            if (addGenderBox.SelectedItem == null)
            {
                addRegisterInfo.Text += "*Please choose a Gender.\n";
            }

            //If there are any warnings, return false.
            if (addRegisterInfo.Text.Length > 0) { return false; }
            else { return true; }
        }

        //For Patient Table (no dupes): If SIN already exists, do not add to Patient Table.
        private bool patientIsValid()
        {
            string inputedSIN = addSINBox.Text;
            //Check to see if the patient record already exists in Patient Table
            SqlConnection conn = new SqlConnection(Globals.conn);
            conn.Open();
            SqlCommand checkSIN = new SqlCommand("select count(*) from [Patient] where patientSin = @patientSin", conn);
            checkSIN.Parameters.AddWithValue("@patientSin", inputedSIN);
            int SINExist = (int)checkSIN.ExecuteScalar();
            //Record (SIN) already exists
            if (SINExist > 0)
            {
                addRegisterInfo.Text = "*SIN already exists, no new record was created.";
                addRegisterInfo.ForeColor = Color.Red;
                return false;
            }
            else { return true; }
        }

        //Checks two dates to see if "Later" date is earlier than the "Earlier" date, returns false
        private bool dateIsValid()
        {
            DateTime inputedDOB;
            //DateTime inputedAdmitDate;
            //DateTime inputedDepartDate;
            //Check if entry is empty or incorrect
            if (!addDOBBox.MaskCompleted | !DateTime.TryParse(addDOBBox.Text, out inputedDOB)) { addRegisterInfo.Text += "*Invalid Date of Birth.\n"; }
            return true;
        }

        private void addSINBox_TextChanged(object sender, EventArgs e)
        {
            if (addSINBox.Text.Length == 9)
            {
                DataSet patientSINs = new DataSet();
                DataTable matchingSINs = patientList.Tables[0].Clone();
                foreach (DataRow row in patientList.Tables[0].Rows)
                {
                    if (row["patientSIN"].ToString().StartsWith(addSINBox.Text))
                        matchingSINs.ImportRow(row);
                }
                patientSINs.Tables.Add(matchingSINs);
                patientSINs.Tables[0].DefaultView.Sort = "fullName asc";
                registerListBox.DataSource = patientSINs.Tables[0];
                registerListBox.DisplayMember = "fullName";
            }
            if (addSINBox.Text.Length == 0)
                registerListBox.DataSource = patientList.Tables[0];
        }

        private void registerListBox_DoubleClick(object sender, EventArgs e)
        {
            if (registerListBox.SelectedItem != null && addSINBox.Text.Length == 9)
            {
                DataRowView registrantList = registerListBox.SelectedItem as DataRowView;
                int patientType = Int32.Parse(registrantList["patientType"].ToString());
                addSINBox.Text = registrantList["patientSIN"].ToString();
                if (patientType == 0)
                    addPatientTypeBox.SelectedIndex = 0;
                else if (patientType == 1)
                    addPatientTypeBox.SelectedIndex = 1;
                else
                    addPatientTypeBox.SelectedIndex = 2;

                addFirstNameBox.Text = registrantList["firstName"].ToString();
                addLastNameBox.Text = registrantList["lastName"].ToString();
                addStreetBox.Text = registrantList["street"].ToString();
                addCityBox.Text = registrantList["city"].ToString();
                addProvinceBox.Text = registrantList["province"].ToString();
                addCountryBox.Text = registrantList["country"].ToString();
                if(registrantList["sex"].ToString().Equals("Male"))
                    addGenderBox.SelectedIndex = 0;
                else
                    addGenderBox.SelectedIndex = 1;
                addDOBBox.Text = Convert.ToDateTime(registrantList["dateOfBirth"]).ToString("MM/dd/yyyy");
                //addAdmitDateBox.Text = "";
                //addDepartDateBox.Text = "";
                addInsuranceBox.Text = "";
                addHomePhoneBox.Text = "";
                addCellphoneBox.Text = "";
                addNotesBox.Text = "";
            }
        }

        private void currentWardPatientInfo()
        {
            if (wardListBox.SelectedItem == null)
            {
                selectedWardGridView.Hide();
            }
            else
            {
                DataRowView selectedWard = wardListBox.SelectedItem as DataRowView;
                string wardName = selectedWard["wardName"].ToString();

                DataSet patientsInWard = new DataSet();
                SqlConnection conn = new SqlConnection(Globals.conn);
                conn.Open();

                SqlCommand getWard = new SqlCommand("select concat(firstName, ' ', lastName) as Name from Patient, Register where Patient.patientSIN = Register.patientSIN and Register.leaveDate is null and Register.registerID in (select registerID from Patient_Ward where dateOut is null and wardName = @wardName)", conn);
                getWard.Parameters.AddWithValue("@wardName", wardName);
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = getWard;
                adapter.Fill(patientsInWard);
                selectedWardGridView.AutoGenerateColumns = true;
                selectedWardGridView.DataSource = patientsInWard.Tables[0];

                int rowCount = selectedWardGridView.RowCount;
                if (rowCount > 0)
                {
                    currentWardPatientsInfo.Hide();
                    selectedWardGridView.Show();
                    int totalRowHeight = selectedWardGridView.ColumnHeadersHeight;
                    if (rowCount > 8)
                    {
                        totalRowHeight += (selectedWardGridView.Rows[0].Height * 8) - 20;
                        selectedWardGridView.Height = totalRowHeight;
                    }
                    else
                    {
                        totalRowHeight += (selectedWardGridView.Rows[0].Height * (rowCount + 1)) - 20;
                        selectedWardGridView.Height = totalRowHeight;
                    }
                }
                else
                {
                    selectedWardGridView.Hide();
                    currentWardPatientsInfo.Show();
                }
            }
        }

        private void lougoutAdmin_Click(object sender, EventArgs e)
        {
            loginForm.Show();
            this.Close();
        }

        private void registerListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //addSINBox.Text = "";
            addPatientTypeBox.SelectedIndex = -1;
            addFirstNameBox.Text = "";
            addLastNameBox.Text = "";
            addStreetBox.Text = "";
            addCityBox.Text = "";
            addProvinceBox.Text = "";
            addCountryBox.Text = "";
            addGenderBox.SelectedIndex = -1;
            addDOBBox.Text = "";
            //addAdmitDateBox.Text = "";
            //addDepartDateBox.Text = "";
            addInsuranceBox.Text = "";
            addHomePhoneBox.Text = "";
            addCellphoneBox.Text = "";
            addNotesBox.Text = "";
            addRegisterInfo.Text = "";
        }

        private void filterButton_Click(object sender, EventArgs e)
        {
            string exp = "";
            if (userNameFilter.Text.Length > 0)
                exp += "username like '" + userNameFilter.Text + "%' ";
            if (roleFilter.SelectedIndex == 0)
            {
                if (exp.Length > 0)
                    exp += "and ";
                exp += "isAdmin = true";
            }
            if (roleFilter.SelectedIndex == 1)
            {
                if (exp.Length > 0)
                    exp += "and ";
                exp += "isAdmin = false";
            }
            if (exp.Length > 0 || roleFilter.SelectedIndex == 2)
            {
                //Debug.WriteLine("first check: " + accountList.Tables[0].Rows.Count.ToString());
                DataRow[] foundRows = accountList.Tables[0].Select(exp);
                //Debug.WriteLine("second check: " + accountList.Tables[0].Rows.Count.ToString());
                if (foundRows.Length > 0)
                {
                    accountList.Tables[0].DefaultView.RowFilter = exp;
                    accountListBox.DataSource = accountList.Tables[0];
                    filterError.Text = "";
                }
                else
                    filterError.Text = "no results found";
            }
        }

        private void refreshAccount_Click(object sender, EventArgs e)
        {
            populateAccountList();
            roleFilter.SelectedIndex = -1;
            filterError.Text = "";
            userNameFilter.Text = "";
        }

        private void filterWardButton_Click(object sender, EventArgs e)
        
        {
            string exp = "";
            if (wardNameFilter.Text.Length > 0)
                exp += "wardName like '" + wardNameFilter.Text + "%' ";
            if (vacancyFilter.SelectedIndex == 0)
            {
                if (exp.Length > 0)
                    exp += "and ";
                exp += "current_capacity < overall_capacity";
            }
            if (vacancyFilter.SelectedIndex == 1)
            {
                if (exp.Length > 0)
                    exp += "and ";
                exp += "current_capacity = overall_capacity";
            }
            if (exp.Length > 0)
            {
                //Debug.WriteLine("first check: " + wardList.Tables[0].Rows.Count.ToString());
                DataRow[] foundRows = wardList.Tables[0].Select(exp);
                //Debug.WriteLine("second check: " + wardList.Tables[0].Rows.Count.ToString());
                if (foundRows.Length > 0)
                {
                    wardList.Tables[0].DefaultView.RowFilter = exp;
                    wardListBox.DataSource = wardList.Tables[0];
                    filterErrorWard.Text = "";
                }
                else
                {
                    filterErrorAWard.ForeColor = Color.Red;
                    filterErrorAWard.Text = "No results found.";
                }
            }
        }

        private void refreshWard_Click(object sender, EventArgs e)
        {
            populateWardList();
            vacancyFilter.SelectedIndex = -1;
            filterErrorAWard.Text = "";
            wardNameFilter.Text = "";
        }

        private void applyDocFilter_Click(object sender, EventArgs e)
        {
            string exp = "";
            if (docFirstNameFilter.Text.Length > 0)
                exp += "firstName like '" + docFirstNameFilter.Text + "%' ";
            if (docLastNameFilter.Text.Length > 0)
            {
                if (exp.Length > 0)
                    exp += "and ";
                exp += "lastName like '" + docLastNameFilter.Text + "%' ";
            }
            if (docDeptFilter.SelectedIndex > -1)
            {
                if (exp.Length > 0)
                    exp += "and ";
                exp += "departmentName = '" + docDeptFilter.SelectedItem.ToString() + "' ";
            }
            if (docSpecFilter.Text.Length > 0)
            {
                if (exp.Length > 0)
                    exp += "and ";
                exp += "specialization like '" + docSpecFilter.Text + "%'";
            }
            if (exp.Length > 0)
            {
                Debug.WriteLine(exp);
                DataRow[] foundRows = doctorList.Tables[0].Select(exp);
                if (foundRows.Length > 0)
                {
                    doctorList.Tables[0].DefaultView.RowFilter = exp;
                    doctorList.Tables[0].DefaultView.Sort = "Name asc";
                    DoctorListBox.DataSource = doctorList.Tables[0];
                    docFilterError.Text = "";
                }
                else
                {
                    docFilterError.Text = "No results found";
                    docFilterError.ForeColor = Color.Red;
                }
                docDeptFilter.SelectedIndex = -1;
            }
            else
            {
                docFilterError.Text = "No filters selected";
                docFilterError.ForeColor = Color.Red;
            }
        }

        private void refreshDocList_Click(object sender, EventArgs e)
        {
            populateDoctorList();
            docFilterError.Text = "";
            docFirstNameFilter.Text = "";
            docLastNameFilter.Text = "";
            docSpecFilter.Text = "";
            docDeptFilter.SelectedIndex = -1;
        }

        private void DoctorDeptBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }



        //removed patient records
    }
}
