using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using BusinessLayer.Data;
using BusinessLayer.Repositories;
using BusinessLayer.Services.Interfaces;
using BusinessLayer.Services;
using SteamProfile.ViewModels;
using BusinessLayer.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SteamProfile.Views;
using Microsoft.Extensions.Configuration;
using BusinessLayer.DataContext;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace SteamProfile
{
    public partial class App : Application
    {
        // Steam Community part
        private static readonly Dictionary<Type, object> Services = new Dictionary<Type, object>();
        private static void ConfigureServices()
        {
            // Build configuration from appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Read connection string
            var connectionString = config.GetConnectionString("DefaultConnection");

            // Build DbContextOptions for the DataContext
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            // Instantiate and register EF Core DataContext
            var dataContext = new ApplicationDbContext(dbContextOptions);
            Services[typeof(ApplicationDbContext)] = dataContext;

            // EF Core repositories
            var sessionRepository = new SessionRepository(dataContext);
            Services[typeof(ISessionRepository)] = sessionRepository;

            var userProfilesRepository = new UserProfilesRepository(dataContext);
            Services[typeof(IUserProfilesRepository)] = userProfilesRepository;

            var walletRepository = new WalletRepository(dataContext);
            Services[typeof(IWalletRepository)] = walletRepository;

            var ownedGamesRepository = new OwnedGamesRepository(dataContext);
            Services[typeof(IOwnedGamesRepository)] = ownedGamesRepository;

            var reviewRepository = new ReviewRepository(dataContext);
            Services[typeof(IReviewRepository)] = reviewRepository;

            var newsRepository = new NewsRepository(dataContext);
            Services[typeof(INewsRepository)] = newsRepository;

            var passwordResetRepository = new PasswordResetRepository(dataContext);
            Services[typeof(IPasswordResetRepository)] = passwordResetRepository;

            var friendshipRepository = new FriendshipsRepository(dataContext);
            Services[typeof(IFriendshipsRepository)] = friendshipRepository;

            var friendRequestRepository = new FriendRequestRepository(dataContext);
            Services[typeof(IFriendRequestRepository)] = friendRequestRepository;

            var userRepository = new UsersRepository(dataContext);
            Services[typeof(IUsersRepository)] = userRepository;

            var featureRepository = new FeaturesRepository(dataContext);
            Services[typeof(IFeaturesRepository)] = featureRepository;

            var collectionsRepository = new CollectionsRepository(dataContext);
            Services[typeof(ICollectionsRepository)] = collectionsRepository;

            var achievementsRepository = new AchievementsRepository(dataContext);
            Services[typeof(IAchievementsRepository)] = achievementsRepository;

            var repository = new Repository(dataContext);
            Services[typeof(IRepository)] = repository;

            Services[typeof(IForumRepository)] = new ForumRepository(GetService<ApplicationDbContext>());

            // This is the old repository that is not used anymore (needs to be removed)
            var friendRepository = new FriendRepository(dataContext);
            Services[typeof(IFriendRepository)] = friendRepository;

            // Configuration
            string currentUsername = "JaneSmith"; // This would come from authentication

            // Register database connection
            var dbConnection = new DatabaseConnection();
            Services[typeof(DatabaseConnection)] = dbConnection;

            // Register repositories

            // Register services
            var friendService = new FriendService(friendRepository);
            Services[typeof(IFriendService)] = friendService;

            var reviewService = new ReviewService(reviewRepository);
            Services[typeof(IReviewService)] = reviewService;

            var sessionService = new SessionService(sessionRepository, userRepository);
            Services[typeof(ISessionService)] = sessionService;

            var userService = new UserService(userRepository, sessionService);
            Services[typeof(IUserService)] = userService;

            var service = new Service(repository);
            Services[typeof(IService)] = service;

            Services[typeof(ICollectionsService)] = new CollectionsService(collectionsRepository);

            var featuresService = new FeaturesService(featureRepository, userService);
            Services[typeof(IFeaturesService)] = featuresService;

            Services[typeof(INewsService)] = new NewsService(GetService<INewsRepository>(), GetService<IUserService>());

            Services[typeof(IForumService)] = new ForumService(GetService<IForumRepository>());

            new FriendRequestService(friendRequestRepository, friendService);
            var friendRequestService = new FriendRequestService(friendRequestRepository, friendService);
            Services[typeof(IFriendRequestService)] = friendRequestService;

            // Register view models
            var friendRequestViewModel = new FriendRequestViewModel(friendRequestService, currentUsername);
            Services[typeof(FriendRequestViewModel)] = friendRequestViewModel;
        }

        public static T GetService<T>()
            where T : class
        {
            if (Services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }

            throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered");
        }

        // Services
        public static readonly IAchievementsService AchievementsService;
        public static readonly FeaturesService FeaturesService;
        public static readonly ICollectionsService CollectionsService;
        public static readonly IWalletService WalletService;
        public static readonly IUserService UserService;
        public static readonly IFriendsService FriendsService;
        public static readonly IOwnedGamesService OwnedGamesService;
        public static readonly AuthenticationService AuthenticationService;
        public static readonly IForumService ForumService;

        // View Models
        public static readonly AddGameToCollectionViewModel AddGameToCollectionViewModel;
        public static readonly CollectionGamesViewModel CollectionGamesViewModel;
        public static readonly CollectionsViewModel CollectionsViewModel;
        public static readonly UsersViewModel UsersViewModel;
        public static readonly FriendsViewModel FriendsViewModel;

        public static PasswordResetService PasswordResetService { get; private set; }
        public static readonly SessionService SessionService;
        public static UserProfilesRepository UserProfileRepository { get; private set; }
        public static ICollectionsRepository CollectionsRepository { get;  }

        public static PasswordResetRepository PasswordResetRepository { get; private set; }

        public static IUsersRepository UserRepository { get; private set; }

        static App()
        {
            // Wire up EF Core and all new repositories and services
            ConfigureServices();

            var dataLink = DataLink.Instance;
            var navigationService = NavigationService.Instance;
            var achievementsRepository = GetService<IAchievementsRepository>();

            // EF-Core repositories
            var sessionRepository = (SessionRepository)GetService<ISessionRepository>();
            var walletRepository = GetService<IWalletRepository>();
            UserRepository = GetService<IUsersRepository>();
            UserProfileRepository = (UserProfilesRepository)GetService<IUserProfilesRepository>();
            PasswordResetRepository = (PasswordResetRepository)GetService<IPasswordResetRepository>();
            CollectionsRepository = (CollectionsRepository)GetService<ICollectionsRepository>();
            var reviewRepository = GetService<IReviewRepository>();
            var ownedGamesRepository = GetService<IOwnedGamesRepository>();
            var newsRepository = GetService<INewsRepository>();
            var friendshipsRepository = GetService<IFriendshipsRepository>();
            var forumRespository = GetService<IForumRepository>();
            var featuresRepository = GetService<IFeaturesRepository>();

            // Initialize all services
            SessionService = new SessionService(sessionRepository, UserRepository);
            UserService = new UserService(UserRepository, SessionService);
            AchievementsService = new AchievementsService(achievementsRepository);
            CollectionsService = new CollectionsService(CollectionsRepository);
            AuthenticationService = new AuthenticationService(UserRepository);
            FriendsService = new FriendsService(friendshipsRepository, UserService);
            OwnedGamesService = new OwnedGamesService(ownedGamesRepository);
            PasswordResetService = new PasswordResetService(PasswordResetRepository, UserService);
            FeaturesService = new FeaturesService(featuresRepository, UserService);
            WalletService = new WalletService(walletRepository, UserService);
            ForumService = new ForumService(forumRespository);
            ForumService.Initialize(GetService<IForumService>());

            // Initialize all view models
            UsersViewModel = UsersViewModel.Instance;
            AddGameToCollectionViewModel = new AddGameToCollectionViewModel(CollectionsService, UserService);
            FriendsViewModel = new FriendsViewModel(FriendsService, UserService);
            CollectionGamesViewModel = new CollectionGamesViewModel(CollectionsService);
            CollectionsViewModel = new CollectionsViewModel(CollectionsService, UserService);

            // Finally, initialize the achivements off of the EF-based AchievementsService
            InitializeAchievements();
        }

        private static void InitializeAchievements()
        {
            AchievementsService.InitializeAchievements();
        }

        private Window mainWindow;

        public App()
        {
            this.InitializeComponent();
        }

        public Window MainWindow { get; set; }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            mainWindow = new MainWindow();
            // NavigationService.Instance.Initialize(m_window.Content as Frame); // Ensure the frame is passed
            mainWindow.Activate();
        }
    }
}
