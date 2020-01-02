﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using GeoGeometry.Model.Auth;
using GeoGeometry.Model.User;
using Newtonsoft.Json;
using Plugin.Settings;

namespace GeoGeometry.Activity.Auth {
	[Activity(Label = "AuthActivity")]
    public class AuthActivity : AppCompatActivity
    {
        /// <summary>
        /// Почта клиента
        /// </summary>
        private EditText s_login;

        /// <summary>
        /// Пароль клиента.
        /// </summary>
        private EditText s_pass;

        /// <summary>
        /// Пароль клиента.
        /// </summary>
        private CheckBox is_remember;

        /// <summary>
        /// Конпка регистрации.
        /// </summary>
        private Button btn_register;

        /// <summary>
        /// Конпка авторизации.
        /// </summary>
        private Button btn_auth;

        /// <summary>
        /// Конпка назад.
        /// </summary>
        private ImageButton btn_back_a;

        /// <summary>
        /// Кнопка прокрутки.
        /// </summary>
        private ProgressBar preloader;


        /// <summary>
        /// Mobile Box New 
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_auth);

            btn_register = FindViewById<Button>(Resource.Id.btn_register);
            btn_auth = FindViewById<Button>(Resource.Id.btn_auth);
            btn_back_a = FindViewById<ImageButton>(Resource.Id.btn_back_a);

            s_login = FindViewById<EditText>(Resource.Id.s_login);
            s_pass = FindViewById<EditText>(Resource.Id.s_pass);
            is_remember = FindViewById<CheckBox>(Resource.Id.is_remember);
            preloader = FindViewById<ProgressBar>(Resource.Id.loader);


            btn_back_a.Click += (s, e) =>
            {
                Finish();
            };




            string dir_path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string file_data_remember = "";
            // Проверяю запомнил ли пользователя.
            string check = CrossSettings.Current.GetValueOrDefault("check","");
            
            if (check == "1")
            {
                s_login.Text = CrossSettings.Current.GetValueOrDefault("login","");
                s_pass.Text = CrossSettings.Current.GetValueOrDefault("password","");
            }

            is_remember.Checked = true;
            


            //        if (file_data_remember.Substring(0, 1) == "1")
            //        {
            //preloader.Visibility = Android.Views.ViewStates.Visible;
            //file_data_remember = file_data_remember.Remove(0, 1);
            //            AuthResponseData o_data = JsonConvert.DeserializeObject<AuthResponseData>(file_data_remember);

            //            StaticUser.AddInfoAuth(o_data);

            //preloader.Visibility = Android.Views.ViewStates.Gone;

            //// Переход на главную страницу.
            //Intent homeActivity = new Intent(this, typeof(Home.HomeActivity));
            //            StartActivity(homeActivity);
            //            //this.Finish();
            //        }
            //        else
            //        {
            // Переход к форме регистрации.
            btn_register.Click += (s, e) =>
            {
                Intent registerActivity = new Intent(this, typeof(Auth.RegisterActivity));
                StartActivity(registerActivity);
            };

            btn_auth.Click += async delegate
            {
                try
                {
                    preloader.Visibility = Android.Views.ViewStates.Visible;
                    // Авторизируюсь клиентом.
                    
                    AuthModel auth = new AuthModel
                    {
                        Email = s_login.Text,
                        Password = s_pass.Text,
                    };



                    var myHttpClient = new HttpClient();
                    var _authHeader = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", auth.Email, auth.Password))));

                    myHttpClient.DefaultRequestHeaders.Authorization = _authHeader;

                    var uri = new Uri("http://iot.tmc-centert.ru/api/auth/login?email=" + auth.Email + "&password=" + auth.Password);
                        /*
                        //json структура.
                        var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            { "Email", auth.Email },
                            { "Password", auth.Password }
                        });
                        */
                        // Поучаю ответ об авторизации [успех или нет]
                        HttpResponseMessage response = await myHttpClient.PostAsync(uri.ToString(), new StringContent(JsonConvert.SerializeObject(auth), Encoding.UTF8, "application/json"));

                    string s_result;
                    using (HttpContent responseContent = response.Content)
                    {
                        s_result = await responseContent.ReadAsStringAsync();
                    }

                    AuthApiData<AuthResponseData> o_data = JsonConvert.DeserializeObject<AuthApiData<AuthResponseData>>(s_result);

                    
                        //ClearField();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        if (o_data.Status == "0")
                        {
                            Toast.MakeText(this, o_data.Message, ToastLength.Long).Show();

                            AuthResponseData o_user_data = new AuthResponseData();
                            o_user_data = o_data.ResponseData;
                            
                            if (is_remember.Checked == true)
                            {
                                CrossSettings.Current.AddOrUpdateValue("check", "1");
                                CrossSettings.Current.AddOrUpdateValue("login", s_login.Text);                               
                                CrossSettings.Current.AddOrUpdateValue("password", s_pass.Text);
                            }
                            else
                            {
                                CrossSettings.Current.AddOrUpdateValue("check", "0");
                            }

                            StaticUser.Email = s_login.Text;
                            StaticUser.AddInfoAuth(o_user_data);

                                //пример ContainerSelection

                                //using (FileStream fs = new FileStream(dir_path + "user_data.txt", FileMode.OpenOrCreate))
                                //{
                                //    await System.Text.Json.JsonSerializer.SerializeAsync<AuthResponseData>(fs, o_user_data);
                                //}

                            using (FileStream file = new FileStream(dir_path + "user_data.txt", FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                    // преобразуем строку в байты
                                    byte[] array = Encoding.Default.GetBytes(JsonConvert.SerializeObject(o_user_data));// 0 связан с запоминанием 
                                                                                                                             // запись массива байтов в файл
                                    file.Write(array, 0, array.Length);
                            }

                                //var role = o_data.ResponseData.Role;
                                //Начинаю собирать информацию о клиенте
                                preloader.Visibility = Android.Views.ViewStates.Invisible;
                                // Переход на страницу водителя.
                            if (o_data.ResponseData.Role == "driver")
                            {
                                Intent Driver = new Intent(this, typeof(Auth.DriverActivity));
                                StartActivity(Driver);
                                this.Finish();
                            }
                            else if (o_data.ResponseData.Role == "user")
                            {
                                Intent UserActivity = new Intent(this, typeof(Auth.ActivityUserBox));
                                StartActivity(UserActivity);
                                this.Finish();
                            }

                        }
                        else
                        {
                            Toast.MakeText(this, o_data.Message, ToastLength.Long).Show();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Toast.MakeText(this, "" + ex.Message, ToastLength.Long).Show();
                }
            };
        }

        //private async void GetInfoAboutUser(string dir_path, string file_data_remember)
        //{
        //    if (File.Exists(@"" + dir_path + "user_data.txt"))
        //    {
        //        using (FileStream file = new FileStream(dir_path + "user_data.txt", FileMode.Open, FileAccess.Read))
        //        {
        //            // преобразуем строку в байты
        //            byte[] array = new byte[file.Length];
        //            // считываем данные
        //            file.Read(array, 0, array.Length);
        //            // декодируем байты в строку
        //            file_data_remember = Encoding.Default.GetString(array);
        //            file.Close();
        //        }
        //        AuthResponseData user = JsonConvert.DeserializeObject<AuthResponseData>(file_data_remember);

        //        var myHttpClient = new HttpClient();
        //        var uri = new Uri("http://iot.tmc-centert.ru/api/container/getbox?id=" + user.UserId);
        //        HttpResponseMessage response = await myHttpClient.GetAsync(uri);

        //        AuthApiData<AuthResponseData> o_data = new AuthApiData<AuthResponseData>();

        //        string s_result;
        //        using (HttpContent responseContent = response.Content)
        //        {
        //            s_result = await responseContent.ReadAsStringAsync();
        //        }

        //        o_data = JsonConvert.DeserializeObject<AuthApiData<AuthResponseData>>(s_result);
        //        if (response.StatusCode == HttpStatusCode.OK)
        //        {
        //            if (o_data.Status == "0")
        //            {
        //                Toast.MakeText(this, o_data.Message, ToastLength.Long).Show();
        //                AuthResponseData exported_data = new AuthResponseData();
        //                exported_data = o_data.ResponseData;
        //                if (user.Check == "1")
        //                {
        //                    s_login.Text = exported_data.
        //                }

        //            } 
        //        }
                
        //    }
        //}
        /// <summary>
        /// Removes activity from history after navigating to new activity.
        /// </summary>
        protected override void OnStop() {
			base.OnStop();
			Finish();
		}

		/// <summary>
		/// Closes app if back button is pressed.
		/// </summary>
		public override void OnBackPressed()
        {
            if (FragmentManager.BackStackEntryCount > 0)
            {
                FragmentManager.PopBackStack();
            }
            else
            {
                base.OnBackPressed();
            }
        }

        /// <summary>
        /// Метод очистки полей.
        /// </summary>
        void ClearField()
        {
            s_login.Text = "";
            s_pass.Text = "";
        }
    }
}