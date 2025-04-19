using Microsoft.UI.Xaml;
using BusinessLayer.Data;
using BusinessLayer.Repositories;
using BusinessLayer.Services.Interfaces;
using BusinessLayer.Services;
using SteamProfile.ViewModels;
using BusinessLayer.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using SteamProfile.Views;

namespace SteamProfile
{
    public partial class App : Application
    {
        // Services
        public static readonly IAchievementsService AchievementsService;
        public static readonly FeaturesService FeaturesService;
        public static readonly ICollectionsService CollectionsService;
        public static readonly IWalletService WalletService;
        public static readonly IUserService UserService;
        public static readonly IFriendsService FriendsService;
        public static readonly IOwnedGamesService OwnedGamesService;
        public static readonly AuthenticationService AuthenticationService;

        // View Models
        public static readonly AddGameToCollectionViewModel AddGameToCollectionViewModel;
        public static readonly CollectionGamesViewModel CollectionGamesViewModel;
        public static readonly CollectionsViewModel CollectionsViewModel;
        public static readonly UsersViewModel UsersViewModel;
        public static readonly FriendsViewModel FriendsViewModel;

        public static PasswordResetService PasswordResetService { get; private set; }
        public static readonly SessionService SessionService;
        public static UserProfilesRepository UserProfileRepository { get; private set; }
        public static CollectionsRepository CollectionsRepository { get;  }

        public static PasswordResetRepository PasswordResetRepository { get; private set; }

        public static UsersRepository UserRepository { get; private set; }

        static App()
        {
            var dataLink = DataLink.Instance;
            var navigationService = NavigationService.Instance;

            var achievementsRepository = new AchievementsRepository(dataLink);
            var featuresRepository = new FeaturesRepository(dataLink);
            UserRepository = new UsersRepository(dataLink);
            UserProfileRepository = new UserProfilesRepository(dataLink);
            CollectionsRepository = new CollectionsRepository(dataLink);
            IWalletRepository walletRepository = new WalletRepository(dataLink);
            IFriendshipsRepository friendshipsRepository = new FriendshipsRepository(dataLink);
            IOwnedGamesRepository ownedGamesRepossitory = new OwnedGamesRepository(dataLink);
            var sessionRepository = new SessionRepository(dataLink);
            PasswordResetRepository = new PasswordResetRepository(dataLink);

            // Initialize all services
            SessionService = new SessionService(sessionRepository, UserRepository);
            UserService = new UserService(UserRepository, SessionService);
            AchievementsService = new AchievementsService(achievementsRepository);
            CollectionsService = new CollectionsService(CollectionsRepository);
            AuthenticationService = new AuthenticationService(UserRepository);
            FriendsService = new FriendsService(friendshipsRepository, UserService);
            OwnedGamesService = new OwnedGamesService(ownedGamesRepossitory);
            PasswordResetService = new PasswordResetService(PasswordResetRepository, UserService);
            FeaturesService = new FeaturesService(featuresRepository, UserService);
            WalletService = new WalletService(walletRepository, UserService);

            // Initialize all view models
            UsersViewModel = UsersViewModel.Instance;
            AddGameToCollectionViewModel = new AddGameToCollectionViewModel(CollectionsService, UserService);
            FriendsViewModel = new FriendsViewModel(FriendsService, UserService);
            CollectionGamesViewModel = new CollectionGamesViewModel(CollectionsService);
            CollectionsViewModel = new CollectionsViewModel(CollectionsService, UserService);

            // Others
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

        private void ConfigureServices(IServiceCollection services)
        {
            // Register services
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IPasswordResetService>(sp =>
                new PasswordResetService(PasswordResetRepository, sp.GetRequiredService<IUserService>()));
            services.AddSingleton<IFeaturesService>(sp =>
                new FeaturesService(
                    sp.GetRequiredService<IFeaturesRepository>(),
                    sp.GetRequiredService<IUserService>()));

            // Register repositories
            services.AddSingleton<IUsersRepository, UsersRepository>();
            services.AddSingleton<IFeaturesRepository, FeaturesRepository>();
            services.AddSingleton<IPasswordResetRepository, PasswordResetRepository>();

            // Register ViewModels
            services.AddTransient<FeaturesViewModel>();
            services.AddTransient<ForgotPasswordViewModel>();

            // Register pages
            services.AddTransient<MainWindow>();
            services.AddTransient<FeaturesPage>();
            services.AddTransient<ProfilePage>();
        }
    }
}
