using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.UI.Xaml.Controls;
using Forum;

namespace Forum_Lib
{
    public class ViewModelActionCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Predicate<object> canExecute;

        public ViewModelActionCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            execute = execute ?? throw new ArgumentNullException(nameof(execute));
            canExecute = canExecute;
        }

        // Event required by ICommand interface
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => canExecute == null || canExecute(parameter);

        public void Execute(object parameter) => execute(parameter);

        // Method to manually trigger CanExecuteChanged
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class ForumViewModel : INotifyPropertyChanged
    {
        private readonly IForumService forumService;

        private ObservableCollection<ForumPost> posts;
        public ObservableCollection<ForumPost> Posts
        {
            get => posts;
            set => SetProperty(ref posts, value);
        }

        private ForumPost selectedPost;
        public ForumPost SelectedPost
        {
            get => selectedPost;
            set
            {
                if (SetProperty(ref selectedPost, value))
                {
                    // Load comments when a post is selected and notify command state change
                    LoadCommentsForPost(); // Use synchronous method
                    // Manually raise CanExecuteChanged for the command
                    ((ViewModelActionCommand)AddCommentCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private ObservableCollection<ForumComment> comments;
        public ObservableCollection<ForumComment> Comments
        {
            get => comments;
            set => SetProperty(ref comments, value);
        }

        // Commands
        public ICommand CreatePostCommand { get; }
        public ICommand AddCommentCommand { get; }
        public ICommand LoadPostsCommand { get; }

        // Constructor
        public ForumViewModel(IForumService forumService)
        {
            forumService = forumService ?? throw new ArgumentNullException(nameof(forumService));

            Posts = new ObservableCollection<ForumPost>();
            Comments = new ObservableCollection<ForumComment>();

            // Initialize Commands
            LoadPostsCommand = new ViewModelActionCommand(parameter => LoadPosts());
            // Point RelayCommands to the async methods
            CreatePostCommand = new ViewModelActionCommand(async parameter => await CreateNewPostAsync());
            AddCommentCommand = new ViewModelActionCommand(async parameter => await AddNewCommentAsync(), parameter => SelectedPost != null);

            // Load initial posts
            LoadPosts(); // Call synchronous method directly
        }

        // Method to load posts (synchronous to match service)
        private void LoadPosts()
        {
            try
            {
                // Use correct service method GetPagedPosts (load first page for example)
                uint pageNumber = 0;
                uint pageSize = 20; // Example page size
                var posts = forumService.GetPagedPosts(pageNumber, pageSize);
                Posts.Clear();
                foreach (var post in posts)
                {
                    Posts.Add(post);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error loading posts: {exception.Message}");
            }
        }

        // Method to load comments for the selected post (synchronous)
        private void LoadCommentsForPost()
        {
            if (SelectedPost == null)
            {
                Comments.Clear();
                return;
            }

            try
            {
                var comments = forumService.GetComments(SelectedPost.Id);
                Comments.Clear();
                foreach (var comment in comments)
                {
                    Comments.Add(comment);
                }
            }
            catch (Exception exception)
            {
                 Console.WriteLine($"Error loading comments for post {SelectedPost.Id}: {exception.Message}");
            }
        }

        private async Task CreateNewPostAsync()
        {
            // Instantiate the dialog
            var createPostDialog = new CreatePostDialog();

            // Show the dialog asynchronously
            var result = await createPostDialog.ShowAsync();

            // Check if the dialog was submitted successfully
            // AND if the dialog indicated success via its PostCreated property.
            if (result == ContentDialogResult.Primary && createPostDialog.PostCreated)
            {
                LoadPosts(); // Reload posts
            }
        }

        private async Task AddNewCommentAsync()
        {
            if (SelectedPost == null) return;

            // Instantiate the dialog, passing the selected post's ID
            var addCommentDialog = new AddCommentDialog(SelectedPost.Id);

            // Show the dialog asynchronously
            var result = await addCommentDialog.ShowAsync();

            // Check if the dialog was submitted successfully
            // AND if the dialog indicated success via its CommentCreated property.
            if (result == ContentDialogResult.Primary && addCommentDialog.CommentCreated)
            {
                LoadCommentsForPost(); // Reload comments for the current post
            }
        }


        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<Property>(ref Property storage, Property value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<Property>.Default.Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
