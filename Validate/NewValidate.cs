﻿using AnlaxPackage.Auth;
using AnlaxPackage;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;

namespace AnlaxBase.Validate
{
    [Serializable]
    public class NewValidate
    {
        private string _connectionString;

        private string _user;

        private string _password;

        private string userInfo;

        private string dataBase = "postgres";

        private string scheme = "public";

        private string _tableName
        {
            get
            {
                if (_user != null)
                {
                    return _user + "table";
                }

                return "Пользователь не надйен";
            }
        }

        public NewValidate(string username, string password, Document doc = null)
        {
            _user = username;
            _password = password;
            _connectionString = "Host=91.245.227.212;Port=5432;Username=" + username + ";Password=" + password + ";Database=" + dataBase;
       
            if (doc != null)
            {
                userInfo = doc.Application.Username;
            }
        }
        public bool CheckLicenseSilence()
        {
            if (AuthSettings.Initialize().NumberLiscence > 0)
            {
                return true;
            }

            if (!CanConnectToDatabase())
            {
                return false;
            }

            if (!CheckAvailableLicense())
            {
                return false;
            }

            int num = SetNumberLLiscence(userInfo);
            if (num == 0)
            {
                return false;
            }
            AuthSettings.Initialize().NumberLiscence = num;
            StaticAuthorization.SetLiscence(num);
            return true;
        }

        public bool CheckAvailableLicense()
        {
            try
            {
                long num = 0L;
                using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(_connectionString))
                {
                    npgsqlConnection.Open();
                    string cmdText = "SELECT COUNT(*) FROM " + scheme + "." + _tableName + " WHERE userrevit IS NULL OR userrevit = ''";
                    NpgsqlCommand npgsqlCommand = new NpgsqlCommand(cmdText, npgsqlConnection);
                    num = (long)npgsqlCommand.ExecuteScalar();
                }

                return num > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool CanConnectToDatabase()
        {
            try
            {
                using NpgsqlConnection npgsqlConnection = new NpgsqlConnection(_connectionString);
                npgsqlConnection.Open();
                npgsqlConnection.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool CheckLicense(bool manualAuthorizatiom = false)
        {
            if (manualAuthorizatiom)
            {
                AuthViewNew authView = new AuthViewNew();
                authView.GreetingsBlock.Text = "Я Вас категорически приветствую. Введите логин и пароль.";
                authView.ShowDialog();
                _user = AuthSettings.Initialize().Login;
                _password = AuthSettings.Initialize().Password;
                _connectionString = "Host=91.245.227.212;Port=5432;Username=" + _user + ";Password=" + _password + ";Database=" + dataBase;
            }

            if (AuthSettings.Initialize().NumberLiscence > 0)
            {
                return true;
            }

            if (string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_password))
            {
                AuthViewNew authView2 = new AuthViewNew();
                authView2.GreetingsBlock.Text = "В системе не задан логин или пароль. Введите логин или пароль в форме ниже.Тетовая версия работает до 50 элементов заданий и отверстий";
                authView2.ShowDialog();
                _user = AuthSettings.Initialize().Login;
                _password = AuthSettings.Initialize().Password;
                _connectionString = "Host=91.245.227.212;Port=5432;Username=" + _user + ";Password=" + _password + ";Database=" + dataBase;
            }

            if (!CanConnectToDatabase())
            {
                AuthViewNew authView3 = new AuthViewNew();
                authView3.GreetingsBlock.Text = "Не удалось войти по логину и паролю. Повторите ввод";
                authView3.ShowDialog();
                _user = AuthSettings.Initialize().Login;
                _password = AuthSettings.Initialize().Password;
                _connectionString = "Host=91.245.227.212;Port=5432;Username=" + _user + ";Password=" + _password + ";Database=" + dataBase;
                if (!CanConnectToDatabase())
                {
                    MessageBox.Show("Не удалось подключиться к серверу. Попробуйте повторить попытку через несколько минут.");
                    return false;
                }
            }

            if (!CheckAvailableLicense())
            {
                AuthViewNew authView4 = new AuthViewNew();
                authView4.GreetingsBlock.Text = "Все лицензии заняты. Повторите ввод";
                authView4.ShowDialog();
                _user = AuthSettings.Initialize().Login;
                _password = AuthSettings.Initialize().Password;
                _connectionString = "Host=91.245.227.212;Port=5432;Username=" + _user + ";Password=" + _password + ";Database=" + dataBase;
                if (!CheckAvailableLicense())
                {
                    MessageBox.Show("Все лицензии заняты. Увельчите количество версий. Или вежливо выдерните шнур питания у кого-то из ваших коллег");
                    return false;
                }
            }

            int num = SetNumberLLiscence(userInfo);
            if (num == 0)
            {
                MessageBox.Show("Не удалось активировать лицензию. Обратитесь в тех.поддержку");
                return false;
            }
            else
            {
                MessageBox.Show("Активирована лицензия с номером "+ num);
            }    

            AuthSettings.Initialize().NumberLiscence = num;
            StaticAuthorization.SetLiscence(num);
            return true;
        }

        public bool CheckAvailableLicense()
        {
            try
            {
                long num = 0L;
                using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(_connectionString))
                {
                    npgsqlConnection.Open();
                    string cmdText = "SELECT COUNT(*) FROM " + scheme + "." + _tableName + " WHERE userrevit IS NULL OR userrevit = ''";
                    NpgsqlCommand npgsqlCommand = new NpgsqlCommand(cmdText, npgsqlConnection);
                    num = (long)npgsqlCommand.ExecuteScalar();
                }

                return num > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool CanConnectToDatabase()
        {
            try
            {
                using NpgsqlConnection npgsqlConnection = new NpgsqlConnection(_connectionString);
                npgsqlConnection.Open();
                npgsqlConnection.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public int SetNumberLLiscence(string userInfo)
        {
            try
            {
                using NpgsqlConnection npgsqlConnection = new NpgsqlConnection(_connectionString);
                npgsqlConnection.Open();
                using NpgsqlTransaction npgsqlTransaction = npgsqlConnection.BeginTransaction();
                string cmdText = "SELECT numberliscence FROM " + scheme + "." + _tableName + " WHERE userrevit = @userInfo";
                using (NpgsqlCommand npgsqlCommand = new NpgsqlCommand(cmdText, npgsqlConnection, npgsqlTransaction))
                {
                    npgsqlCommand.Parameters.AddWithValue("@userInfo", userInfo);
                    object obj = npgsqlCommand.ExecuteScalar();
                    if (obj != null)
                    {
                        int result = (int)obj;
                        npgsqlTransaction.Rollback();
                        return result;
                    }
                }

                string cmdText2 = "SELECT numberliscence FROM " + scheme + "." + _tableName + " WHERE userrevit IS NULL OR LENGTH(userrevit) < 1 ORDER BY numberliscence LIMIT 1 FOR UPDATE";
                using NpgsqlCommand npgsqlCommand2 = new NpgsqlCommand(cmdText2, npgsqlConnection, npgsqlTransaction);
                object obj2 = npgsqlCommand2.ExecuteScalar();
                if (obj2 != null)
                {
                    int num = (int)obj2;
                    string cmdText3 = "UPDATE " + scheme + "." + _tableName + " SET userrevit = @userInfo WHERE numberliscence = @numberLiscence";
                    using (NpgsqlCommand npgsqlCommand3 = new NpgsqlCommand(cmdText3, npgsqlConnection, npgsqlTransaction))
                    {
                        npgsqlCommand3.Parameters.AddWithValue("@userInfo", userInfo);
                        npgsqlCommand3.Parameters.AddWithValue("@numberLiscence", num);
                        npgsqlCommand3.ExecuteNonQuery();
                    }

                    npgsqlTransaction.Commit();
                    return num;
                }

                npgsqlTransaction.Rollback();
                return 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
                return 0;
            }
        }

        public bool ReleaseLicense(int num)
        {
            if (!CanConnectToDatabase())
            {
                AuthViewNew authView = new AuthViewNew();
                authView.GreetingsBlock.Text = "Не удалось войти по логину и паролю. Повторите ввод";
                authView.ShowDialog();
                _user = AuthSettings.Initialize().Login;
                _password = AuthSettings.Initialize().Password;
                _connectionString = "Host=91.245.227.212;Port=5432;Username=" + _user + ";Password=" + _password + ";Database=" + dataBase;
                if (!CanConnectToDatabase())
                {
                    MessageBox.Show("Не удалось подключиться к серверу. Попробуйте повторить попытку через несколько минут.");
                    return false;
                }
            }

            if (num == 0)
            {
                using (NpgsqlConnection npgsqlConnection = new NpgsqlConnection(_connectionString))
                {
                    npgsqlConnection.Open();
                    using NpgsqlTransaction npgsqlTransaction = npgsqlConnection.BeginTransaction();
                    string cmdText = "UPDATE " + scheme + "." + _tableName + " SET userrevit = '' WHERE userrevit = @userInfo";
                    using NpgsqlCommand npgsqlCommand = new NpgsqlCommand(cmdText, npgsqlConnection, npgsqlTransaction);
                    npgsqlCommand.Parameters.AddWithValue("@userInfo", userInfo);
                    int num2 = npgsqlCommand.ExecuteNonQuery();
                    if (num2 > 0)
                    {
                        npgsqlTransaction.Commit();
                        MessageBox.Show("Лицензия успешно освобождена.");
                        AuthSettings.Initialize().NumberLiscence = 0;
                        return true;
                    }

                    npgsqlTransaction.Rollback();
                    MessageBox.Show("Лицензия уже освобождена. Город может спать спокойно.");
                    return false;
                }
            }

            try
            {
                using NpgsqlConnection npgsqlConnection2 = new NpgsqlConnection(_connectionString);
                npgsqlConnection2.Open();
                using NpgsqlTransaction npgsqlTransaction2 = npgsqlConnection2.BeginTransaction();
                string cmdText2 = "UPDATE " + scheme + "." + _tableName + " SET userrevit = '' WHERE numberliscence = @num";
                using NpgsqlCommand npgsqlCommand2 = new NpgsqlCommand(cmdText2, npgsqlConnection2, npgsqlTransaction2);
                npgsqlCommand2.Parameters.AddWithValue("@num", num);
                int num3 = npgsqlCommand2.ExecuteNonQuery();
                if (num3 > 0)
                {
                    npgsqlTransaction2.Commit();
                    MessageBox.Show("Лицензия успешно освобождена.");
                    AuthSettings.Initialize().NumberLiscence = 0;
                    return true;
                }

                npgsqlTransaction2.Rollback();
                MessageBox.Show("Не удалось найти лицензию с указанным номером.");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось освободить лицензию. Ошибка: " + ex.Message);
                return false;
            }
        }

        public bool ReleaseSilenceLicense()
        {
            int numberLiscence = AuthSettings.Initialize().NumberLiscence;
            if (!CanConnectToDatabase())
            {
                return false;
            }

            try
            {
                using NpgsqlConnection npgsqlConnection = new NpgsqlConnection(_connectionString);
                npgsqlConnection.Open();
                using NpgsqlTransaction npgsqlTransaction = npgsqlConnection.BeginTransaction();
                string cmdText = "UPDATE " + scheme + "." + _tableName + " SET userrevit = '' WHERE numberliscence = @num";
                using NpgsqlCommand npgsqlCommand = new NpgsqlCommand(cmdText, npgsqlConnection, npgsqlTransaction);
                npgsqlCommand.Parameters.AddWithValue("@num", numberLiscence);
                int num = npgsqlCommand.ExecuteNonQuery();
                if (num > 0)
                {
                    npgsqlTransaction.Commit();
                    AuthSettings.Initialize().NumberLiscence = 0;
                    return true;
                }

                npgsqlTransaction.Rollback();
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}