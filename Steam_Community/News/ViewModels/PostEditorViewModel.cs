using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;

namespace News.ViewModels
{
    public partial class PostEditorViewModel : ObservableObject
    {
        private readonly NewsService service = new();

        private Post? postBeingEdited;
        private bool isEditMode = false;

        public event EventHandler? PostUploaded;

        [ObservableProperty]
        private string username;

        [ObservableProperty]
        private string currentDate;

        [ObservableProperty]
        private BitmapImage profilePicture;

        [ObservableProperty]
        private string rawHtml;

        [ObservableProperty]
        private Visibility rawEditorVisibility = Visibility.Visible;

        [ObservableProperty]
        private Visibility previewVisibility = Visibility.Collapsed;

        [ObservableProperty]
        private string htmlPreviewContent;

        public PostEditorViewModel()
        {
            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            var user = service.activeUser;
            Username = user.username;
            CurrentDate = DateTime.Now.ToString("MMM d, yyyy");

            var image = new BitmapImage();
            image.SetSource(new MemoryStream(user.profilePicture).AsRandomAccessStream());
            ProfilePicture = image;
        }

        public void SetPostToEdit(Post post)
        {
            isEditMode = true;
            postBeingEdited = post;

            string htmlContent = post.Content;
            int startIndex = htmlContent.IndexOf("<body>") + "<body>".Length;
            int endIndex = htmlContent.IndexOf("</body>");
            RawHtml = htmlContent.Substring(startIndex, endIndex - startIndex);
        }

        public void ResetEditor()
        {
            isEditMode = false;
            postBeingEdited = null;
            RawHtml = string.Empty;
            RawEditorVisibility = Visibility.Visible;
            PreviewVisibility = Visibility.Collapsed;
        }

        [RelayCommand]
        private void ShowRaw()
        {
            RawEditorVisibility = Visibility.Visible;
            PreviewVisibility = Visibility.Collapsed;
        }

        [RelayCommand]
        private void ShowPreview()
        {
            RawEditorVisibility = Visibility.Collapsed;
            PreviewVisibility = Visibility.Visible;
            HtmlPreviewContent = service.FormatAsPost(RawHtml);
        }

        [RelayCommand]
        private void Upload()
        {
            if (string.IsNullOrWhiteSpace(RawHtml))
                return;

            string html = service.FormatAsPost(RawHtml);

            if (isEditMode && postBeingEdited != null)
            {
                service.UpdatePost(postBeingEdited.Id, html);
            }
            else
            {
                service.SavePost(html);
            }

            ResetEditor();
            PostUploaded?.Invoke(this, EventArgs.Empty);
        }
    }
}
