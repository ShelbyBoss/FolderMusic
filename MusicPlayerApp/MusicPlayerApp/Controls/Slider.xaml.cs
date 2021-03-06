﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using FolderMusic.Converters;

namespace FolderMusic
{
    public sealed partial class Slider : UserControl
    {
        private const double zoomWidth = 0.1;

        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register("IsIndeterminate", typeof(bool),
                typeof(Slider), new PropertyMetadata(false));

        public static readonly DependencyProperty ViewPositionRatioProperty =
            DependencyProperty.Register("ViewPositionRatio", typeof(double), typeof(Slider),
                new PropertyMetadata(TimeSpan.Zero, OnViewPositionRatioPropertyChanged));

        private static void OnViewPositionRatioPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Slider s = (Slider)sender;
            double value = (double)e.NewValue;

            s.ViewPosition = s.Duration.Multiply(value);
        }

        public static readonly DependencyProperty ViewPositionProperty =
            DependencyProperty.Register("ViewPosition", typeof(TimeSpan), typeof(Slider),
                new PropertyMetadata(TimeSpan.Zero, OnViewPositionPropertyChanged));

        private static void OnViewPositionPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Slider s = (Slider)sender;
            TimeSpan value = (TimeSpan)e.NewValue;

            if (s.Duration > TimeSpan.Zero) s.ViewPositionRatio = value.TotalDays / s.Duration.TotalDays;
        }

        public static readonly DependencyProperty PositionRatioProperty =
            DependencyProperty.Register("PositionRatio", typeof(double), typeof(Slider),
                new PropertyMetadata(TimeSpan.Zero, OnPositionRatioPropertyChanged));

        private static void OnPositionRatioPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Slider s = (Slider)sender;
            double value = (double)e.NewValue;

            s.ViewPositionRatio = value;
            s.Position = s.Duration.Multiply(value);
        }

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(TimeSpan), typeof(Slider),
                new PropertyMetadata(TimeSpan.Zero, OnPositionPropertyChanged));

        private static void OnPositionPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Slider s = (Slider)sender;
            TimeSpan value = (TimeSpan)e.NewValue;

            s.ViewPosition = value;

            if (s.Duration > TimeSpan.Zero) s.PositionRatio = value.TotalDays / s.Duration.TotalDays;
        }

        public static readonly DependencyProperty ViewDurationProperty =
            DependencyProperty.Register("ViewDuration", typeof(TimeSpan),
                typeof(Slider), new PropertyMetadata(TimeSpan.Zero));


        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(TimeSpan), typeof(Slider),
                new PropertyMetadata(TimeSpan.Zero, OnDurationPropertyChanged));

        private static void OnDurationPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Slider s = (Slider)sender;
            TimeSpan value = (TimeSpan)e.NewValue;

            s.Position = value.Multiply(s.PositionRatio);

            if (s.playerPositionEnabled) s.ViewDuration = value;
        }

        private bool playerPositionEnabled = true;

        public bool IsIndeterminate
        {
            get { return (bool)GetValue(IsIndeterminateProperty); }
            set { SetValue(IsIndeterminateProperty, value); }
        }

        public double ViewPositionRatio
        {
            get { return (double)GetValue(ViewPositionRatioProperty); }
            set { SetValue(ViewPositionRatioProperty, value); }
        }

        public TimeSpan ViewPosition
        {
            get { return (TimeSpan)GetValue(ViewPositionProperty); }
            set { SetValue(ViewPositionProperty, value); }
        }

        public double PositionRatio
        {
            get { return (double)GetValue(PositionRatioProperty); }
            set { SetValue(PositionRatioProperty, value); }
        }

        public TimeSpan Position
        {
            get { return (TimeSpan)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public TimeSpan ViewDuration
        {
            get { return (TimeSpan)GetValue(ViewDurationProperty); }
            set { SetValue(ViewDurationProperty, value); }
        }

        public TimeSpan Duration
        {
            get { return (TimeSpan)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public Slider()
        {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement highestParent = this;

            while (highestParent.Parent is FrameworkElement) highestParent = (FrameworkElement)highestParent.Parent;

            highestParent.PointerExited += HighestParent_PointerExited;
        }

        private void sld_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            playerPositionEnabled = false;
        }

        private void HighestParent_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (playerPositionEnabled) return;

            playerPositionEnabled = true;

            Position = ViewPosition;

            sld.Minimum = 0;
            sld.Maximum = 1;
            ViewDuration = Duration;

            tblBegin.Visibility = tblEnd.Visibility = Visibility.Collapsed;
        }

        private void sld_Holding(object sender, HoldingRoutedEventArgs e)
        {
            double value = sld.Value;
            sld.Minimum = value - zoomWidth * value;
            sld.Maximum = value + (1 - value) * zoomWidth;

            double min = sld.Minimum;
            double max = sld.Maximum;
            TimeSpan duration = Duration;
            TimeSpan beginTime = duration.Multiply(min);
            TimeSpan endTime = duration.Multiply(max);

            tblBegin.Text = TimeSpanConverter.Convert(beginTime);
            tblEnd.Text = TimeSpanConverter.Convert(endTime);
            tblBegin.Visibility = tblEnd.Visibility = Visibility.Visible;
        }
    }
}
