using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using News.ViewModels;

namespace News
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            (value is Visibility v && v == Visibility.Visible);
    }

    // InverseBoolToVisibilityConverter.cs
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            (value is bool b && !b) ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            (value is Visibility v && v == Visibility.Visible) == false;
    }

    public sealed partial class CommentControl : UserControl
    {
        public CommentViewModel ViewModel { get; } = new();

        public event RoutedEventHandler? CommentDeleted;
        public event RoutedEventHandler? CommentUpdated;

        public CommentControl()
        {
            this.InitializeComponent();

            this.DataContext = ViewModel;

            EditCommentInput.CommentPosted += EditCommentInput_CommentPosted;
            ViewModel.CommentDeleted += (_, __) => CommentDeleted?.Invoke(this, new RoutedEventArgs());
            ViewModel.CommentUpdated += (_, __) => CommentUpdated?.Invoke(this, new RoutedEventArgs());
        }

        public void SetCommentData(Comment comment)
        {
            ViewModel.LoadComment(comment);
            LoadCommentContent(ViewModel.ContentHtml);
        }

        private async void LoadCommentContent(string content)
        {
            try
            {
                if (CommentContent.CoreWebView2 == null)
                    await CommentContent.EnsureCoreWebView2Async().AsTask();

                CommentContent.CoreWebView2.NavigateToString(content);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading comment content: {ex.Message}");
            }
        }

        private void EditCommentInput_CommentPosted(object sender, RoutedEventArgs e)
        {
            var rawEditor = (TextBox)EditCommentInput.FindName("RawEditor");
            if (rawEditor != null)
            {
                ViewModel.SubmitEdit(rawEditor.Text);
                EditCommentInput.ResetControl();
                LoadCommentContent(ViewModel.ContentHtml);
            }
        }
    }
}
