﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace GeoGeometry.Model.GPSLocation
{
    class GPSLocationApiData
    {
        /// <summary>
        /// Статус ответа.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Сообщение ответа.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Информация о клиенте.
        /// </summary>
        public GPSLocationResponseData ResponseData { get; set; }
    }
}