using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Setup.Services;
using Orchard.Setup.ViewModels;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Caching;

namespace Orchard.Setup.Controllers {
    [ValidateInput(false), Themed]
    public class SetupController : Controller {
        private readonly IViewsBackgroundCompilation _viewsBackgroundCompilation;
        private readonly ShellSettings _shellSettings;
        private readonly INotifier _notifier;
        private readonly ISetupService _setupService;
        private readonly IEnumerable<IExtensionLoader> _extensionLoaders;
        private readonly ICacheManager _cacheManager;
        private readonly ICacheHolder _cacheHolder;
        private readonly IThemeManager _themeManager;
        private const string DefaultRecipe = "Default";

        public SetupController(
            INotifier notifier, 
            ISetupService setupService, 
            IViewsBackgroundCompilation viewsBackgroundCompilation,
            ICacheManager cacheManager,
            ICacheHolder cacheHolder,
            //IThemeManager themeManger,//如果SetupMode中没有注册 ThemeManager的话这里找不到，OrchardHost中也没有注册
            ShellSettings shellSettings)//, IEnumerable<IExtensionLoader> extensionLoaders
        {
            _viewsBackgroundCompilation = viewsBackgroundCompilation;
            _shellSettings = shellSettings;
            _notifier = notifier;
            _cacheHolder = cacheHolder;
            _cacheManager = cacheManager;
            //_themeManager = themeManger;
            //_extensionLoaders = extensionLoaders;//这里可以访问到
            _setupService = setupService;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        private ActionResult IndexViewResult(SetupViewModel model) {
            return View(model);
        }

        public ActionResult Second()
        {
            return View();
        }

        public ActionResult Index() {
            var initialSettings = _setupService.Prime();
            var recipes = OrderRecipes(_setupService.Recipes());
            string recipeDescription = null;
            if (recipes.Count > 0) {
                recipeDescription = recipes[0].Description;
            }
            
            // On the first time installation of Orchard, the user gets to the setup screen, which
            // will take a while to finish (user inputting data and the setup process itself).
            // We use this opportunity to start a background task to "pre-compile" all the known
            // views in the app folder, so that the application is more reponsive when the user
            // hits the homepage and admin screens for the first time.))
            if (StringComparer.OrdinalIgnoreCase.Equals(initialSettings.Name, ShellSettings.DefaultName)) {
                _viewsBackgroundCompilation.Start();
            }

            //
            //调用View(Model);
            return IndexViewResult(new SetupViewModel {
                AdminUsername = "admin",
                DatabaseIsPreconfigured = !string.IsNullOrEmpty(initialSettings.DataProvider),
                Recipes = recipes,
                RecipeDescription = recipeDescription
            });
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(SetupViewModel model) {
            var recipes = OrderRecipes(_setupService.Recipes());

            // if no builtin provider, a connection string is mandatory
            if (model.DatabaseProvider != SetupDatabaseType.Builtin && string.IsNullOrEmpty(model.DatabaseConnectionString))
                ModelState.AddModelError("DatabaseConnectionString", T("A connection string is required").Text);

            if (!String.IsNullOrWhiteSpace(model.ConfirmPassword) && model.AdminPassword != model.ConfirmPassword ) {
                ModelState.AddModelError("ConfirmPassword", T("Password confirmation must match").Text);
            }

            if (model.DatabaseProvider != SetupDatabaseType.Builtin && !String.IsNullOrWhiteSpace(model.DatabaseTablePrefix)) {
                model.DatabaseTablePrefix = model.DatabaseTablePrefix.Trim();
                if(!Char.IsLetter(model.DatabaseTablePrefix[0])) {
                    ModelState.AddModelError("DatabaseTablePrefix", T("The table prefix must begin with a letter").Text);
                }

                if(model.DatabaseTablePrefix.Any(x => !Char.IsLetterOrDigit(x))) {
                    ModelState.AddModelError("DatabaseTablePrefix", T("The table prefix must contain letters or digits").Text);
                }
            }
            if (model.Recipe == null) {
                if (!(recipes.Select(r => r.Name).Contains(DefaultRecipe))) {
                    ModelState.AddModelError("Recipe", T("No recipes were found in the Setup module").Text);
                }
                else {
                    model.Recipe = DefaultRecipe;
                }
            }
            if (!ModelState.IsValid) {
                model.Recipes = recipes;
                foreach (var recipe in recipes.Where(recipe => recipe.Name == model.Recipe)) {
                    model.RecipeDescription = recipe.Description;
                }
                model.DatabaseIsPreconfigured = !string.IsNullOrEmpty(_setupService.Prime().DataProvider);
                
                return IndexViewResult(model);
            }

            try {
                string providerName = null;

                switch (model.DatabaseProvider)
                {
                    case SetupDatabaseType.Builtin:
                        providerName = "SqlCe";
                        break;

                    case SetupDatabaseType.SqlServer:
                        providerName = "SqlServer";
                        break;

                    case SetupDatabaseType.MySql:
                        providerName = "MySql";
                        break;

                    default:
                        throw new ApplicationException("Unknown database type: " + model.DatabaseProvider);
                }

                var setupContext = new SetupContext {
                    SiteName = model.SiteName,
                    AdminUsername = model.AdminUsername,
                    AdminPassword = model.AdminPassword,
                    DatabaseProvider = providerName,
                    DatabaseConnectionString = model.DatabaseConnectionString,
                    DatabaseTablePrefix = model.DatabaseTablePrefix,
                    EnabledFeatures = null, // default list
                    Recipe = model.Recipe
                };

                string executionId = _setupService.Setup(setupContext);

                // First time installation if finally done. Tell the background views compilation
                // process to stop, so that it doesn't interfere with the user (asp.net compilation
                // uses a "single lock" mechanism for compiling views).
                _viewsBackgroundCompilation.Stop();

                // redirect to the welcome page.
                return Redirect("~/" + _shellSettings.RequestUrlPrefix);
            } catch (Exception ex) {
                Logger.Error(ex, "Setup failed");
                _notifier.Error(T("Setup failed: {0}", ex.Message));

                model.Recipes = recipes;
                foreach (var recipe in recipes.Where(recipe => recipe.Name == model.Recipe)) {
                    model.RecipeDescription = recipe.Description;
                }
                model.DatabaseIsPreconfigured = !string.IsNullOrEmpty(_setupService.Prime().DataProvider);

                return IndexViewResult(model);
            }
        }

        private static List<Recipe> OrderRecipes(IEnumerable<Recipe> recipes) {
            var recipeList = new List<Recipe>();
            var tempList = new List<Recipe>();
            foreach (var recipe in recipes) {
                if (recipe.Name == DefaultRecipe) {
                    recipeList.Add(recipe);
                }
                else {
                    tempList.Add(recipe);
                }
            }
            return recipeList.Concat(tempList).ToList();
        }
    }
}