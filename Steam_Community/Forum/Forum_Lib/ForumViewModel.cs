using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Forum_Lib
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Predicate<object> canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
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
                    ((RelayCommand)AddCommentCommand).RaiseCanExecuteChanged();
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
            LoadPostsCommand = new RelayCommand(parameter => LoadPosts());
            CreatePostCommand = new RelayCommand(parameter => CreateNewPost());
            AddCommentCommand = new RelayCommand(parameter => AddNewComment(), parameter => SelectedPost != null);

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

        // Placeholder method for Create Post command execution
        private void CreateNewPost()
        {
            // TODO: Implement logic to show Create Post Dialog/View
             Console.WriteLine("Create Post command executed.");
        }

        // Placeholder method for Add Comment command execution
        private void AddNewComment()
        {
            // TODO: Implement logic to show Add Comment Dialog/View for SelectedPost
             Console.WriteLine($"Add Comment command executed for Post ID: {SelectedPost?.Id}.");
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
