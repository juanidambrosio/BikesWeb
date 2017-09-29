﻿using BikeSharing.Clients.Core.Effects;
using BikeSharing.Clients.Windows.Effects;
using System;
using System.ComponentModel;
using System.Diagnostics;
using UI = Windows.UI;
using Xaml = Windows.UI.Xaml;
using Media = Windows.UI.Xaml.Media;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;
using Windows.UI.Xaml.Controls;

[assembly: ExportEffect(typeof(DatePickerLineColorEffect), "DatePickerLineColorEffect")]
namespace BikeSharing.Clients.Windows.Effects
{
    public class DatePickerLineColorEffect : PlatformEffect
    {
        TextBox control;

        protected override void OnAttached()
        {
            control = Control as TextBox;
            UpdateLineColor();
        }

        protected override void OnDetached()
        {
            control = null;
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
            if (args.PropertyName == LineColorEffect.LineColorProperty.PropertyName)
            {
                UpdateLineColor();
            }
        }

        private void UpdateLineColor()
        {
            try
            {
                if(control== null)
                {
                    return;
                }

                control.BorderThickness = new Xaml.Thickness(0, 0, 0, 1);
                var lineColor = XamarinFormColorToWindowsColor(LineColorEffect.GetLineColor(Element));
                control.BorderBrush = new Media.SolidColorBrush(lineColor);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cannot set property on attached control. Error: ", ex.Message);
            }
        }

        private UI.Color XamarinFormColorToWindowsColor(Color xamarinColor)
        {
            return UI.Color.FromArgb((byte)(xamarinColor.A * 255),
                                     (byte)(xamarinColor.R * 255),
                                     (byte)(xamarinColor.G * 255),
                                     (byte)(xamarinColor.B * 255));
        }
    }
}
